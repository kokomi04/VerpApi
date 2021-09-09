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
using VErp.Infrastructure.ServiceCore.Facade;
using Verp.Resources.Stock.StockTake;

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
        private readonly ObjectActivityLogFacade _stockTakeActivityLog;


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
            _stockTakeActivityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.StockTake);
        }

        public async Task<StockTakeModel> CreateStockTake(StockTakeModel model)
        {
            var stockTakePeriod = _stockContext.StockTakePeriod
           .FirstOrDefault(stp => stp.StockTakePeriodId == model.StockTakePeriodId);

            if (_stockContext.StockTakeAcceptanceCertificate.Any(ac => ac.StockTakePeriodId == model.StockTakePeriodId))
                throw new BadRequestException(GeneralCode.ItemNotFound, "Đã tồn tại phiếu xử lý chênh lệch. Cần xóa phiếu xử lý chênh lệch trước khi thay đổi thông tin kiểm kê.");

            if (stockTakePeriod == null)
                throw new BadRequestException(GeneralCode.ItemNotFound, "Kỳ kiểm kê không tồn tại");

            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockStockKeyKey(model.StockTakePeriodId));
            using var trans = await _stockContext.Database.BeginTransactionAsync();
            try
            {
                var ctx = await GenerateStockTakeCode(null, model, stockTakePeriod);

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
                // Đổi trạng thái kỳ kiểm kê
                stockTakePeriod.Status = (int)EnumStockTakePeriodStatus.Processing;
                //Kiểm tra có chênh lệch
                stockTakePeriod.IsDifference = CheckDifference(stockTakePeriod);
                await _stockContext.SaveChangesAsync();

                trans.Commit();
                model.StockTakeId = stockTake.StockTakeId;
                model.StockStatus = EnumStockTakeStatus.Processing;
                model.AccountancyStatus = EnumStockTakeStatus.Processing;

                await ctx.ConfirmCode();

                await _stockTakeActivityLog.LogBuilder(() => StockTakeActivityLogMessage.Create)
                .MessageResourceFormatDatas(stockTake.StockTakeCode)
                .ObjectId(stockTake.StockTakeId)
                .JsonData(model.JsonSerialize())
                .CreateLog();
                return model;
            }
            catch (Exception ex)
            {
                trans.TryRollbackTransaction();
                _logger.LogError(ex, "CreateStockTake");
                throw;
            }
        }


        private async Task<GenerateCodeContext> GenerateStockTakeCode(int? stockTakeId, StockTakeModel model, StockTakePeriod period)
        {
            model.StockTakeCode = (model.StockTakeCode ?? "").Trim();


            var ctx = _customGenCodeHelperService.CreateGenerateCodeContext();

            var code = await ctx
                .SetConfig(EnumObjectType.StockTake)
                .SetConfigData(period.StockTakePeriodId, model.StockTakeDate, period.StockTakePeriodCode)
                .TryValidateAndGenerateCode(_stockContext.StockTake, model.StockTakeCode, (s, code) => s.StockTakeId != stockTakeId && s.StockTakeCode == code);

            model.StockTakeCode = code;

            return ctx;
        }

        public async Task<StockTakeModel> GetStockTake(long stockTakeId)
        {
            var stockTake = await _stockContext.StockTake
                .Include(st => st.StockTakeDetail)
                .FirstOrDefaultAsync(st => st.StockTakeId == stockTakeId);

            var packageIds = stockTake.StockTakeDetail.Where(d => d.PackageId.HasValue).Select(d => d.PackageId).ToList();

            var packages = _stockContext.Package.Where(p => packageIds.Contains(p.PackageId)).ToDictionary(p => p.PackageId, p => p.PackageCode);
            var result = _mapper.Map<StockTakeModel>(stockTake);
            foreach (var item in result.StockTakeDetail)
            {
                if (item.PackageId.HasValue) item.PackageCode = packages[item.PackageId.Value];
            }
            return result;
        }

        public async Task<StockTakeModel> UpdateStockTake(long stockTakeId, StockTakeModel model)
        {
            var stockTakePeriod = _stockContext.StockTakePeriod
                .FirstOrDefault(stp => stp.StockTakePeriodId == model.StockTakePeriodId);

            if (stockTakePeriod == null)
                throw new BadRequestException(GeneralCode.ItemNotFound, "Kỳ kiểm kê không tồn tại");

            if (_stockContext.StockTakeAcceptanceCertificate.Any(ac => ac.StockTakePeriodId == model.StockTakePeriodId))
                throw new BadRequestException(GeneralCode.ItemNotFound, "Đã tồn tại phiếu xử lý chênh lệch. Cần xóa phiếu xử lý chênh lệch trước khi thay đổi thông tin kiểm kê.");

            var stockTake = _stockContext.StockTake
             .FirstOrDefault(st => st.StockTakeId == stockTakeId);

            if (stockTake == null)
                throw new BadRequestException(GeneralCode.ItemNotFound, "Phiếu kiểm kê không tồn tại");

            if (string.IsNullOrEmpty(model.StockTakeCode)) new BadRequestException(GeneralCode.InvalidParams, "Mã phiếu kiểm kê không được để trống");
            if (_stockContext.StockTake.Any(stp => stp.StockTakeCode == model.StockTakeCode && stp.StockTakeId != stockTakeId))
                throw new BadRequestException(GeneralCode.ItemCodeExisted);

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

                // Đổi trạng thái kỳ kiểm kê
                stockTakePeriod.Status = (int)EnumStockTakePeriodStatus.Processing;
                //Kiểm tra có chênh lệch
                stockTakePeriod.IsDifference = CheckDifference(stockTakePeriod);
                await _stockContext.SaveChangesAsync();

                trans.Commit();
                model.StockTakeId = stockTake.StockTakeId;
                model.StockStatus = EnumStockTakeStatus.Processing;
                model.AccountancyStatus = EnumStockTakeStatus.Processing;

                await _stockTakeActivityLog.LogBuilder(() => StockTakeActivityLogMessage.Update)
                   .MessageResourceFormatDatas(stockTake.StockTakeCode)
                   .ObjectId(stockTake.StockTakeId)
                   .JsonData(model.JsonSerialize())
                   .CreateLog();

                return model;
            }
            catch (Exception ex)
            {
                trans.TryRollbackTransaction();
                _logger.LogError(ex, "UpdateStockTake");
                throw;
            }
        }


        private bool CheckDifference(StockTakePeriod stockTakePeriod)
        {
            // Thông tin kiểm kê
            var stockTakeResult = (from std in _stockContext.StockTakeDetail
                                   join st in _stockContext.StockTake on std.StockTakeId equals st.StockTakeId
                                   where st.StockTakePeriodId == stockTakePeriod.StockTakePeriodId
                                   select new
                                   {
                                       std.ProductId,
                                       std.ProductUnitConversionId,
                                       std.PrimaryQuantity
                                   })
                                     .GroupBy(d => new { d.ProductId, d.ProductUnitConversionId })
                                     .Select(g => new StockTakeResultModel
                                     {
                                         ProductId = g.Key.ProductId,
                                         ProductUnitConversionId = g.Key.ProductUnitConversionId,
                                         PrimaryQuantity = g.Sum(d => d.PrimaryQuantity)
                                     }).ToList();

            var productIds = stockTakeResult.Select(p => p.ProductId).ToList();

            // Lấy thông tin tồn kho
            var remainSystem = (from id in _stockContext.InventoryDetail
                                join i in _stockContext.Inventory on id.InventoryId equals i.InventoryId
                                where i.IsDeleted == false && id.IsDeleted == false && i.IsApproved == true && i.Date <= stockTakePeriod.StockTakePeriodDate && i.StockId == stockTakePeriod.StockId && productIds.Contains(id.ProductId)
                                select new
                                {
                                    i.InventoryTypeId,
                                    id.ProductId,
                                    id.ProductUnitConversionId,
                                    id.PrimaryQuantity
                                })
                         .GroupBy(id => new
                         {
                             id.ProductId,
                             id.ProductUnitConversionId
                         }).Select(g => new
                         {
                             ProductId = g.Key.ProductId,
                             ProductUnitConversionId = g.Key.ProductUnitConversionId,
                             RemainQuantity = g.Sum(d => d.InventoryTypeId == (int)EnumInventoryType.Input ? d.PrimaryQuantity : -d.PrimaryQuantity)
                         }).ToList();

            foreach (var item in stockTakeResult)
            {
                var remain = remainSystem.FirstOrDefault(r => r.ProductId == item.ProductId && r.ProductUnitConversionId == item.ProductUnitConversionId);
                if (remain == null || item.PrimaryQuantity.SubProductionDecimal(remain?.RemainQuantity ?? 0) != 0) return true;
            }
            return false;
        }

        public async Task<bool> DeleteStockTake(long stockTakeId)
        {
            var stockTake = _stockContext.StockTake
                    .Where(p => p.StockTakeId == stockTakeId)
                    .FirstOrDefault();

            if (stockTake == null) throw new BadRequestException(GeneralCode.ItemNotFound);

            var stockTakePeriod = _stockContext.StockTakePeriod
                 .FirstOrDefault(stp => stp.StockTakePeriodId == stockTake.StockTakePeriodId);

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

                // Đổi trạng thái kỳ kiểm kê
                stockTakePeriod.Status = (int)EnumStockTakePeriodStatus.Processing;
                //Kiểm tra có chênh lệch
                stockTakePeriod.IsDifference = CheckDifference(stockTakePeriod);

                await _stockContext.SaveChangesAsync();
                trans.Commit();

                await _stockTakeActivityLog.LogBuilder(() => StockTakeActivityLogMessage.Delete)
                   .MessageResourceFormatDatas(stockTake.StockTakeCode)
                   .ObjectId(stockTake.StockTakeId)
                   .JsonData(stockTake.JsonSerialize())
                   .CreateLog();

                return true;
            }
            catch (Exception ex)
            {
                trans.TryRollbackTransaction();
                _logger.LogError(ex, "DeleteStockTake");
                throw;
            }
        }

        public async Task<bool> ApproveStockTake(long stockTakeId)
        {
            var stockTake = _stockContext.StockTake
                    .Where(p => p.StockTakeId == stockTakeId)
                    .FirstOrDefault();

            if (stockTake == null) throw new BadRequestException(GeneralCode.ItemNotFound);

            if ((stockTake.AccountancyRepresentativeId != _currentContextService.UserId || stockTake.AccountancyStatus != (int)EnumStockTakeStatus.Processing)
                && (stockTake.StockRepresentativeId != _currentContextService.UserId || stockTake.StockStatus != (int)EnumStockTakeStatus.Processing))
                throw new BadRequestException(GeneralCode.Forbidden);
            try
            {
                if (stockTake.AccountancyRepresentativeId == _currentContextService.UserId && stockTake.AccountancyStatus == (int)EnumStockTakeStatus.Processing)
                    stockTake.AccountancyStatus = (int)EnumStockTakeStatus.Finish;
                if (stockTake.StockRepresentativeId == _currentContextService.UserId && stockTake.StockStatus == (int)EnumStockTakeStatus.Processing)
                    stockTake.StockStatus = (int)EnumStockTakeStatus.Finish;

                await _stockContext.SaveChangesAsync();

                await _stockTakeActivityLog.LogBuilder(() => StockTakeActivityLogMessage.Approve)
                 .MessageResourceFormatDatas(stockTake.StockTakeCode)
                 .ObjectId(stockTake.StockTakeId)
                 .JsonData(stockTake.JsonSerialize())
                 .CreateLog();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ApproveStockTake");
                throw;
            }

        }
    }
}
