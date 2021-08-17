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

            var stockTakePeriods = _stockContext.StockTakePeriod.Include(stp => stp.StockTakeAcceptanceCertificate).AsQueryable();
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

            var productIds = result.StockTakeResult.Select(p => p.ProductId).ToList();

            // Lấy thông tin tồn kho
            var remainSystem = (from id in _stockContext.InventoryDetail
                                join i in _stockContext.Inventory on id.InventoryId equals i.InventoryId
                                where i.IsDeleted == false && id.IsDeleted == false && i.IsApproved == true && i.Date <= stockTakePeriod.StockTakePeriodDate && i.StockId == stockTakePeriod.StockId && productIds.Contains(id.ProductId)
                                select new
                                {
                                    i.InventoryTypeId,
                                    id.ProductId,
                                    id.ProductUnitConversionId,
                                    id.PrimaryQuantity,
                                    id.ProductUnitConversionQuantity
                                })
                         .GroupBy(id => new
                         {
                             id.ProductId,
                             id.ProductUnitConversionId
                         }).Select(g => new StockRemainQuantity
                         {
                             ProductId = g.Key.ProductId,
                             ProductUnitConversionId = g.Key.ProductUnitConversionId,
                             RemainQuantity = g.Sum(d => d.InventoryTypeId == (int)EnumInventoryType.Input ? d.PrimaryQuantity : -d.PrimaryQuantity),
                             ProductUnitConversionRemainQuantity = g.Sum(d => d.InventoryTypeId == (int)EnumInventoryType.Input ? d.ProductUnitConversionQuantity : -d.ProductUnitConversionQuantity)
                         }).ToList();

            foreach (var item in result.StockTakeResult)
            {
                var remain = remainSystem.FirstOrDefault(r => r.ProductId == item.ProductId && r.ProductUnitConversionId == item.ProductUnitConversionId);
                item.StockRemainQuantity = remain?.RemainQuantity ?? 0;
                item.StockProductUnitConversionRemainQuantity = remain?.ProductUnitConversionRemainQuantity;
                item.StockQuantityDifference = item.PrimaryQuantity.SubProductionDecimal(remain?.RemainQuantity ?? 0);
                item.StockProductUnitConversionQuantityDifference = item.ProductUnitConversionQuantity.GetValueOrDefault().SubProductionDecimal(remain?.ProductUnitConversionRemainQuantity ?? 0);
            }
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
                if (string.IsNullOrEmpty(model.StockTakePeriodCode)) new BadRequestException(GeneralCode.InvalidParams, "Mã kỳ kiểm kê không được để trống");
                if (_stockContext.StockTakePeriod.Any(stp => stp.StockTakePeriodCode == model.StockTakePeriodCode && stp.StockTakePeriodId != stockTakePeriodId))
                    throw new BadRequestException(GeneralCode.ItemCodeExisted);

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
                                    RemainQuantity = g.Sum(d => d.InventoryTypeId == (int)EnumInventoryType.Input ? d.PrimaryQuantity : -d.PrimaryQuantity),
                                    ProductUnitConversionRemainQuantity = g.Sum(d => d.InventoryTypeId == (int)EnumInventoryType.Input ? d.ProductUnitConversionQuantity : -d.ProductUnitConversionQuantity),
                                }).ToListAsync();

            return result;
        }

        public async Task<StockTakeAcceptanceCertificateModel> GetStockTakeAcceptanceCertificate(long stockTakePeriodId)
        {
            var stockTakeAcceptanceCertificate = await _stockContext.StockTakeAcceptanceCertificate
                .FirstOrDefaultAsync(ac => ac.StockTakePeriodId == stockTakePeriodId);
            if (stockTakeAcceptanceCertificate == null) return null;
            var result = _mapper.Map<StockTakeAcceptanceCertificateModel>(stockTakeAcceptanceCertificate);
            return result;
        }

        public async Task<StockTakeAcceptanceCertificateModel> UpdateStockTakeAcceptanceCertificate(long stockTakePeriodId, StockTakeAcceptanceCertificateModel model)
        {
            var stockTakePeriod = _stockContext.StockTakePeriod
               .FirstOrDefault(p => p.StockTakePeriodId == stockTakePeriodId);

            if (stockTakePeriod == null)
                throw new BadRequestException(GeneralCode.ItemNotFound, "Kỳ kiểm kê không tồn tại");

            var stockTakeAcceptanceCertificate = await _stockContext.StockTakeAcceptanceCertificate
                .FirstOrDefaultAsync(ac => ac.StockTakePeriodId == stockTakePeriodId);

            if(_stockContext.StockTake.Any(st => st.StockTakePeriodId == stockTakePeriodId && (st.AccountancyStatus != (int)EnumStockTakeStatus.Finish || st.StockStatus != (int)EnumStockTakeStatus.Finish)))
                throw new BadRequestException(GeneralCode.ItemNotFound, "Tồn tại phiếu kiểm kê chưa hoàn thành");

            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockStockKeyKey(0));
            using var trans = await _stockContext.Database.BeginTransactionAsync();
            try
            {

                model.StockTakePeriodId = stockTakePeriodId;
                if (stockTakeAcceptanceCertificate == null)
                {
                    CustomGenCodeOutputModel currentConfig = null;
                    if (string.IsNullOrEmpty(model.StockTakeAcceptanceCertificateCode))
                    {
                        currentConfig = await _customGenCodeHelperService.CurrentConfig(EnumObjectType.StockTakeAcceptanceCertificate, EnumObjectType.StockTakeAcceptanceCertificate, 0, null, model.StockTakeAcceptanceCertificateCode, null);
                        if (currentConfig == null)
                        {
                            throw new BadRequestException(GeneralCode.ItemNotFound, "Chưa thiết định cấu hình sinh mã");
                        }
                        bool isFirst = true;
                        do
                        {
                            if (!isFirst) await _customGenCodeHelperService.ConfirmCode(currentConfig?.CurrentLastValue);

                            var generated = await _customGenCodeHelperService.GenerateCode(currentConfig.CustomGenCodeId, currentConfig.CurrentLastValue.LastValue, null, model.StockTakeAcceptanceCertificateCode, null);
                            if (generated == null)
                            {
                                throw new BadRequestException(GeneralCode.InternalError, "Không thể sinh mã ");
                            }
                            model.StockTakeAcceptanceCertificateCode = generated.CustomCode;
                            isFirst = false;
                        } while (_stockContext.StockTakeAcceptanceCertificate.Any(o => o.StockTakeAcceptanceCertificateCode == model.StockTakeAcceptanceCertificateCode));
                    }
                    else
                    {
                        // Validate unique
                        if (_stockContext.StockTakeAcceptanceCertificate.Any(o => o.StockTakeAcceptanceCertificateCode == model.StockTakeAcceptanceCertificateCode))
                            throw new BadRequestException(GeneralCode.ItemCodeExisted);
                    }
                    stockTakeAcceptanceCertificate = _mapper.Map<StockTakeAcceptanceCertificate>(model);
                    _stockContext.StockTakeAcceptanceCertificate.Add(stockTakeAcceptanceCertificate);
                }
                else
                {
                    if (string.IsNullOrEmpty(model.StockTakeAcceptanceCertificateCode)) new BadRequestException(GeneralCode.InvalidParams, "Mã phiếu xử lý không được để trống");
                    if (_stockContext.StockTakeAcceptanceCertificate.Any(stp => stp.StockTakeAcceptanceCertificateCode == model.StockTakeAcceptanceCertificateCode && stp.StockTakePeriodId != stockTakePeriodId))
                        throw new BadRequestException(GeneralCode.ItemCodeExisted);
                    _mapper.Map(model, stockTakeAcceptanceCertificate);
                }
                stockTakePeriod.Status = (int)EnumStockTakeAcceptanceCertificateStatus.Waiting;
                await _stockContext.SaveChangesAsync();
                trans.Commit();
                await _activityLogService.CreateLog(EnumObjectType.StockTakePeriod, stockTakePeriod.StockTakePeriodId, $"Update phiếu xử lý chênh lệch cho kỳ kiểm kê {stockTakePeriod.StockTakePeriodCode}", model.JsonSerialize());

                return model;
            }
            catch (Exception ex)
            {
                trans.Rollback();
                _logger.LogError(ex, "UpdateStockTakePeriod");
                throw;
            }
        }

        public async Task<bool> ConfirmStockTakeAcceptanceCertificate(long stockTakePeriodId, ConfirmAcceptanceCertificateModel status)
        {

            var stockTakePeriod = _stockContext.StockTakePeriod
                    .Where(p => p.StockTakePeriodId == stockTakePeriodId)
                    .FirstOrDefault();

            if (stockTakePeriod == null) throw new BadRequestException(GeneralCode.ItemNotFound, "Kỳ kiểm kê không tồn tại");

            var acceptanceCertificate = _stockContext.StockTakeAcceptanceCertificate
                   .Where(ac => ac.StockTakePeriodId == stockTakePeriodId)
                   .FirstOrDefault();

            if (acceptanceCertificate == null) throw new BadRequestException(GeneralCode.ItemNotFound, "Phiếu xử lý không tồn tại");
            try
            {
                acceptanceCertificate.StockTakeAcceptanceCertificateStatus = (int)status.StockTakeAcceptanceCertificateStatus;
                await _stockContext.SaveChangesAsync();

                await _activityLogService.CreateLog(EnumObjectType.StockTakeAcceptanceCertificate, stockTakePeriodId, $"Xác nhận phiếu xử lý {acceptanceCertificate.StockTakeAcceptanceCertificateCode}", status.ToString());
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ApproveStockTakeAcceptanceCertificate");
                throw;
            }

        }

        public async Task<bool> DeleteStockTakeAcceptanceCertificate(long stockTakePeriodId)
        {
            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockStockKeyKey(stockTakePeriodId));
            using var trans = await _stockContext.Database.BeginTransactionAsync();
            try
            {
                var acceptanceCertificate = _stockContext.StockTakeAcceptanceCertificate
                    .Where(p => p.StockTakePeriodId == stockTakePeriodId)
                    .FirstOrDefault();

                if (acceptanceCertificate == null) throw new BadRequestException(GeneralCode.ItemNotFound);
                _stockContext.StockTakeAcceptanceCertificate.Remove(acceptanceCertificate);

                await _stockContext.SaveChangesAsync();
                trans.Commit();

                await _activityLogService.CreateLog(EnumObjectType.StockTakePeriod, stockTakePeriodId, $"Xóa phiếu xử lý chênh lệch {acceptanceCertificate.StockTakeAcceptanceCertificateCode}", acceptanceCertificate.JsonSerialize());
                return true;
            }
            catch (Exception ex)
            {
                trans.TryRollbackTransaction();
                _logger.LogError(ex, "DeleteStockTakeAcceptanceCertificate");
                throw;
            }
        }
    }
}
