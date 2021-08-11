using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Infrastructure.EF.OrganizationDB;
using VErp.Infrastructure.EF.StockDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Master.Service.Dictionay;
using VErp.Services.Stock.Model.StockTake;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Verp.Cache.RedisCache;
using VErp.Commons.Enums.Stock;
using StockTakeEntity = VErp.Infrastructure.EF.StockDB.StockTake;

namespace VErp.Services.Stock.Service.StockTake.Implement
{
    public class StockTakeService : IStockTakeService
    {
        private readonly StockDBContext _stockContext;
        private readonly IActivityLogService _activityLogService;
        private readonly IRoleHelperService _roleHelperService;
        private readonly ICurrentContextService _currentContextService;
        private readonly IMapper _mapper;
        private readonly ICustomGenCodeHelperService _customGenCodeHelperService;
        private readonly ILogger _logger;
        public StockTakeService(
            StockDBSubsidiaryContext stockContext
            , IActivityLogService activityLogService
            , IRoleHelperService roleHelperService
            , ICurrentContextService currentContextService
            , IMapper mapper
            , ICustomGenCodeHelperService customGenCodeHelperService
            , ILogger<StockTakeService> logger
            )
        {
            _stockContext = stockContext;
            _activityLogService = activityLogService;
            _roleHelperService = roleHelperService;
            _currentContextService = currentContextService;
            _mapper = mapper;
            _customGenCodeHelperService = customGenCodeHelperService;
            _logger = logger;
        }

        public async Task<StockTakeModel> CreateStockTake(StockTakeModel model)
        {
            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockStockKeyKey(model.StockTakePeriodId));
            using var trans = await _stockContext.Database.BeginTransactionAsync();
            try
            {
                CustomGenCodeOutputModel currentConfig = null;
                if (string.IsNullOrEmpty(model.StockTakeCode))
                {
                    currentConfig = await _customGenCodeHelperService.CurrentConfig(EnumObjectType.StockTake, EnumObjectType.StockTake, 0, null, model.StockTakeCode, null);
                    if (currentConfig == null)
                    {
                        throw new BadRequestException(GeneralCode.ItemNotFound, "Chưa thiết định cấu hình sinh mã");
                    }
                    bool isFirst = true;
                    do
                    {
                        if (!isFirst) await _customGenCodeHelperService.ConfirmCode(currentConfig?.CurrentLastValue);

                        var generated = await _customGenCodeHelperService.GenerateCode(currentConfig.CustomGenCodeId, currentConfig.CurrentLastValue.LastValue, null, model.StockTakeCode, null);
                        if (generated == null)
                        {
                            throw new BadRequestException(GeneralCode.InternalError, "Không thể sinh mã ");
                        }
                        model.StockTakeCode = generated.CustomCode;
                        isFirst = false;
                    } while (_stockContext.StockTake.Any(st => st.StockTakeCode == model.StockTakeCode));
                }
                else
                {
                    // Validate unique
                    if (_stockContext.StockTake.Any(st => st.StockTakeCode == model.StockTakeCode))
                        throw new BadRequestException(GeneralCode.ItemCodeExisted);
                }

                var stockTake = _mapper.Map<StockTakeEntity>(model);
                stockTake.StockStatus = (int)EnumStockTakeStatus.Processing;
                stockTake.AccountancyStatus = (int)EnumStockTakeStatus.Processing;
                _stockContext.StockTake.Add(stockTake);

                await _stockContext.SaveChangesAsync();

                // Thêm chi tiết
                foreach (var detail in model.StockTakeDetail)
                {
                    var entity = _mapper.Map<StockTakeDetail>(detail);
                    entity.StockTakeId = stockTake.StockTakeId;
                    _stockContext.StockTakeDetail.Add(entity);
                }

                await _stockContext.SaveChangesAsync();

                trans.Commit();
                model.StockTakeId = stockTake.StockTakeId;

                await _customGenCodeHelperService.ConfirmCode(currentConfig?.CurrentLastValue);

                await _activityLogService.CreateLog(EnumObjectType.StockTake, stockTake.StockTakeId, $"Thêm mới phiếu kiểm kê {stockTake.StockTakeCode}", model.JsonSerialize());

                return model;
            }
            catch (Exception ex)
            {
                trans.TryRollbackTransaction();
                _logger.LogError(ex, "CreateStockTake");
                throw;
            }
        }

        public async Task<StockTakeModel> GetStockTake(long stockTakeId)
        {
            var stockTake = await _stockContext.StockTake
                .Include(st => st.StockTakeDetail)
                .FirstOrDefaultAsync(st => st.StockTakeId == stockTakeId);
            return _mapper.Map<StockTakeModel>(stockTake);
        }

        public async Task<StockTakeModel> UpdateStockTake(long stockTakeId, StockTakeModel model)
        {
            var stockTake = _stockContext.StockTake
             .FirstOrDefault(st => st.StockTakeId == stockTakeId);

            if (stockTake == null)
                throw new BadRequestException(GeneralCode.ItemNotFound, "Phiếu kiểm kê không tồn tại");

            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockStockKeyKey(model.StockTakePeriodId));
            using var trans = await _stockContext.Database.BeginTransactionAsync();
            try
            {
                _mapper.Map(model, stockTake);
                stockTake.StockStatus = (int)EnumStockTakeStatus.Processing;
                stockTake.AccountancyStatus = (int)EnumStockTakeStatus.Processing;
                // Cập nhật chi tiết
                var currentStockTakeDetails = _stockContext.StockTakeDetail.Where(std => std.StockTakeId == stockTakeId).ToList();

                foreach (var item in model.StockTakeDetail)
                {
                    item.StockTakeId = stockTakeId;
                    var currentItem = currentStockTakeDetails.Where(std => std.StockTakeDetailId == item.StockTakeDetailId).FirstOrDefault();
                    if (currentItem != null)
                    {
                        // Cập nhật
                       _mapper.Map(item, currentItem);
                        // Gỡ khỏi danh sách cũ
                        currentStockTakeDetails.Remove(currentItem);
                    }
                    else
                    {
                        item.StockTakeDetailId = 0;
                        // Tạo mới
                        var entity = _mapper.Map<StockTakeDetail>(item);
                        _stockContext.StockTakeDetail.Add(entity);
                    }
                }

                // Xóa chi tiết
                foreach (var item in currentStockTakeDetails)
                {
                    item.IsDeleted = true;
                }

                await _stockContext.SaveChangesAsync();

                trans.Commit();
                model.StockTakeId = stockTake.StockTakeId;
                await _activityLogService.CreateLog(EnumObjectType.StockTake, stockTake.StockTakeId, $"Cập nhật phiếu kiểm kê {stockTake.StockTakeCode}", model.JsonSerialize());

                return model;
            }
            catch (Exception ex)
            {
                trans.TryRollbackTransaction();
                _logger.LogError(ex, "UpdateStockTake");
                throw;
            }
        }

        public async Task<bool> DeleteStockTake(long stockTakeId)
        {
            var stockTake = _stockContext.StockTake
                    .Where(p => p.StockTakeId == stockTakeId)
                    .FirstOrDefault();

            if (stockTake == null) throw new BadRequestException(GeneralCode.ItemNotFound);

            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockStockKeyKey(stockTake.StockTakePeriodId));
            using var trans = await _stockContext.Database.BeginTransactionAsync();
            try
            {

                stockTake.IsDeleted = true;

                // Danh sách detail
                var stockTakeDetails = _stockContext.StockTakeDetail.Where(std => std.StockTakeId == stockTakeId).ToList();

                // Xóa chi tiết
                foreach (var stockTakeDetail in stockTakeDetails)
                {
                    stockTakeDetail.IsDeleted = true;
                }

                await _stockContext.SaveChangesAsync();
                trans.Commit();

                await _activityLogService.CreateLog(EnumObjectType.StockTake, stockTakeId, $"Xóa phiếu kiểm kê {stockTake.StockTakeCode}", stockTake.JsonSerialize());
                return true;
            }
            catch (Exception ex)
            {
                trans.TryRollbackTransaction();
                _logger.LogError(ex, "DeleteStockTake");
                throw;
            }
        }
    }
}
