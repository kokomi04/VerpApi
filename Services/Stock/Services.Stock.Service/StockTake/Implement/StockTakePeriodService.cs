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

namespace VErp.Services.Stock.Service.StockTake.Implement
{
    public class StockTakePeriodService : IStockTakePeriodService
    {
        private readonly StockDBContext _stockContext;
        private readonly IActivityLogService _activityLogService;
        private readonly IRoleHelperService _roleHelperService;
        private readonly ICurrentContextService _currentContextService;
        private readonly IMapper _mapper;
        private readonly ICustomGenCodeHelperService _customGenCodeHelperService;
        private readonly ILogger _logger;
        public StockTakePeriodService(
            StockDBSubsidiaryContext stockContext
            , IActivityLogService activityLogService
            , IRoleHelperService roleHelperService
            , ICurrentContextService currentContextService
            , IMapper mapper
            , ICustomGenCodeHelperService customGenCodeHelperService
            , ILogger<StockTakePeriodService> logger
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


        public async Task<PageData<StockTakePeriotListModel>> GetStockTakePeriods(string keyword, int page, int size, long fromDate, long toDate, int[] stockIds)
        {

            var stockTakePeriods = _stockContext.StockTakePeriod.AsQueryable();
            var from = fromDate.UnixToDateTime();
            var to = toDate.UnixToDateTime();

            if (!string.IsNullOrEmpty(keyword))
            {
                keyword = keyword.Trim();
                stockTakePeriods = stockTakePeriods.Where(stp => stp.StockTakePeriodCode.Contains(keyword) || stp.Content.Contains(keyword));
            }
            if (fromDate > 0 && toDate > 0)
            {
                stockTakePeriods = stockTakePeriods.Where(stp => stp.StockTakePeriodDate >= from && stp.StockTakePeriodDate <= to);
            }
            if (stockIds != null && stockIds.Length > 0)
            {
                stockTakePeriods = stockTakePeriods.Where(stp => stockIds.Contains(stp.StockId));
            }
            var total = await stockTakePeriods.CountAsync();
            var paged = (size > 0 ? stockTakePeriods.Skip((page - 1) * size).Take(size) : stockTakePeriods).ProjectTo<StockTakePeriotListModel>(_mapper.ConfigurationProvider).ToList();
            return (paged, total);
        }
        public async Task<StockTakePeriotModel> CreateStockTakePeriod(StockTakePeriotModel model)
        {
            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockStockKeyKey(0));
            using var trans = await _stockContext.Database.BeginTransactionAsync();
            try
            {
                CustomGenCodeOutputModel currentConfig = null;
                if (string.IsNullOrEmpty(model.StockTakePeriodCode))
                {
                    currentConfig = await _customGenCodeHelperService.CurrentConfig(EnumObjectType.StockTakePeriod, EnumObjectType.StockTakePeriod, 0, null, model.StockTakePeriodCode, null);
                    if (currentConfig == null)
                    {
                        throw new BadRequestException(GeneralCode.ItemNotFound, "Chưa thiết định cấu hình sinh mã");
                    }
                    bool isFirst = true;
                    do
                    {
                        if (!isFirst) await _customGenCodeHelperService.ConfirmCode(currentConfig?.CurrentLastValue);

                        var generated = await _customGenCodeHelperService.GenerateCode(currentConfig.CustomGenCodeId, currentConfig.CurrentLastValue.LastValue, null, model.StockTakePeriodCode, null);
                        if (generated == null)
                        {
                            throw new BadRequestException(GeneralCode.InternalError, "Không thể sinh mã ");
                        }
                        model.StockTakePeriodCode = generated.CustomCode;
                        isFirst = false;
                    } while (_stockContext.StockTakePeriod.Any(o => o.StockTakePeriodCode == model.StockTakePeriodCode));
                }
                else
                {
                    // Validate unique
                    if (_stockContext.StockTakePeriod.Any(o => o.StockTakePeriodCode == model.StockTakePeriodCode))
                        throw new BadRequestException(GeneralCode.ItemCodeExisted);
                }

                var stockTakePeriod = _mapper.Map<StockTakePeriod>(model);

                stockTakePeriod.Status = (int)EnumStockTakePeriodStatus.Waiting;
                _stockContext.StockTakePeriod.Add(stockTakePeriod);
                await _stockContext.SaveChangesAsync();

                trans.Commit();
                model.StockTakePeriodId = stockTakePeriod.StockTakePeriodId;

                await _customGenCodeHelperService.ConfirmCode(currentConfig?.CurrentLastValue);

                await _activityLogService.CreateLog(EnumObjectType.StockTakePeriod, stockTakePeriod.StockTakePeriodId, $"Thêm mới kỳ kiểm kê {stockTakePeriod.StockTakePeriodCode}", model.JsonSerialize());

                return model;
            }
            catch (Exception ex)
            {
                trans.TryRollbackTransaction();
                _logger.LogError(ex, "CreateStockTakePeriod");
                throw;
            }
        }

        public async Task<bool> DeleteStockTakePeriod(long stockTakePeriodId)
        {
            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockStockKeyKey(stockTakePeriodId));
            using var trans = await _stockContext.Database.BeginTransactionAsync();
            try
            {
                var stockTakePeriod = _stockContext.StockTakePeriod
                    .Where(p => p.StockTakePeriodId == stockTakePeriodId)
                    .FirstOrDefault();
                if (stockTakePeriod == null) throw new BadRequestException(GeneralCode.ItemNotFound);
                stockTakePeriod.IsDeleted = true;

                // Phiếu kiểm kê
                var stockTakes = _stockContext.StockTake.Where(st => st.StockTakePeriodId == stockTakePeriodId).ToList();
                var stockTakeIds = stockTakes.Select(st => st.StockTakeId).ToList();

                // Danh sách detail
                var stockTakeDetails = _stockContext.StockTakeDetail.Where(std => stockTakeIds.Contains(std.StockTakeId)).ToList();

                // Xóa chi stockTakeDetails
                foreach (var stockTakeDetail in stockTakes)
                {
                    stockTakeDetail.IsDeleted = true;
                }

                // Xóa phiếu
                foreach (var stockTake in stockTakes)
                {
                    stockTake.IsDeleted = true;
                }

                await _stockContext.SaveChangesAsync();
                trans.Commit();

                await _activityLogService.CreateLog(EnumObjectType.StockTakePeriod, stockTakePeriodId, $"Xóa kỳ kiểm kê {stockTakePeriod.StockTakePeriodCode}", stockTakePeriod.JsonSerialize());
                return true;
            }
            catch (Exception ex)
            {
                trans.TryRollbackTransaction();
                _logger.LogError(ex, "DeleteStockTakePeriod");
                throw;
            }
        }

        public async Task<StockTakePeriotModel> GetStockTakePeriod(long stockTakePeriodId)
        {
            var stockTakePeriod = await _stockContext.StockTakePeriod
                .Include(p => p.StockTakeRepresentative)
                .Include(p => p.StockTake)
                .ThenInclude(st => st.StockTakeDetail)
                .FirstOrDefaultAsync(p => p.StockTakePeriodId == stockTakePeriodId);

            var result = _mapper.Map<StockTakePeriotModel>(stockTakePeriod);

            result.StockTakeResult = stockTakePeriod.StockTake
                .SelectMany(s => s.StockTakeDetail)
                .GroupBy(d => new { d.ProductId, d.ProductUnitConversionId })
                .Select(g => new StockTakeResultModel
                {
                    ProductId = g.Key.ProductId,
                    ProductUnitConversionId = g.Key.ProductUnitConversionId,
                    PrimaryQuantity = g.Sum(d => d.PrimaryQuantity),
                    ProductUnitConversionQuantity = g.Sum(d => d.ProductUnitConversionQuantity.GetValueOrDefault())
                }).ToList();

            return result;
        }

        public async Task<StockTakePeriotModel> UpdateStockTakePeriod(long stockTakePeriodId, StockTakePeriotModel model)
        {
            var stockTakePeriod = _stockContext.StockTakePeriod
               .FirstOrDefault(p => p.StockTakePeriodId == stockTakePeriodId);

            if (stockTakePeriod == null)
                throw new BadRequestException(GeneralCode.ItemNotFound, "Kỳ kiểm kê không tồn tại");

            try
            {
                _mapper.Map(model, stockTakePeriod);
                await _stockContext.SaveChangesAsync();

                await _activityLogService.CreateLog(EnumObjectType.StockTakePeriod, stockTakePeriod.StockTakePeriodId, $"Cập nhật kỳ kiểm kê {stockTakePeriod.StockTakePeriodCode}", model.JsonSerialize());

                return model;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateStockTakePeriod");
                throw;
            }
        }

        public async Task<IList<StockRemainQuantity>> CalcStockRemainQuantity(CalcStockRemainInputModel data)
        {
            var stockTakePeriodDate = data.StockTakePeriodDate.UnixToDateTime();
            var result = await (from id in _stockContext.InventoryDetail 
                        join i in _stockContext.Inventory on id.InventoryId equals i.InventoryId
                        where i.IsDeleted == false && id.IsDeleted == false && i.IsApproved == true && i.Date <= stockTakePeriodDate && i.StockId == data.StockId && data.ProductIds.Contains(id.ProductId)
                              select new
                        {
                            i.InventoryTypeId,
                            id.ProductId,
                            id.ProductUnitConversionId,
                            id.PrimaryQuantity,
                            id.ProductUnitConversionQuantity
                        }).GroupBy(id => new
                        {
                            id.ProductId,
                            id.ProductUnitConversionId
                        }).Select(g => new StockRemainQuantity
                        {
                            ProductId = g.Key.ProductId,
                            ProductUnitConversionId = g.Key.ProductUnitConversionId,
                            RemainQuantity = g.Sum(d => d.InventoryTypeId == (int) EnumInventoryType.Input? d.PrimaryQuantity : -d.PrimaryQuantity),
                            ProductUnitConversionRemainQuantity = g.Sum(d => d.InventoryTypeId == (int)EnumInventoryType.Input ? d.ProductUnitConversionQuantity : -d.ProductUnitConversionQuantity),
                        }).ToListAsync();

            return result;
        }
    }
}
