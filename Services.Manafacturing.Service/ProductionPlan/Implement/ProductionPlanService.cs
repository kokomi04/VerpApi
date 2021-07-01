using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verp.Cache.RedisCache;
using VErp.Commons.Enums.ErrorCodes;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.ManufacturingDB;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Manafacturing.Model.ProductionPlan;

namespace VErp.Services.Manafacturing.Service.ProductionPlan.Implement
{
    public class ProductionPlanService : IProductionPlanService
    {
        private readonly ManufacturingDBContext _manufacturingDBContext;
        private readonly IActivityLogService _activityLogService;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        public ProductionPlanService(ManufacturingDBContext manufacturingDB
            , IActivityLogService activityLogService
            , ILogger<ProductionPlanService> logger
            , IMapper mapper)
        {
            _manufacturingDBContext = manufacturingDB;
            _activityLogService = activityLogService;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<IDictionary<long, List<ProductionWeekPlanModel>>> GetProductionPlan(long startDate, long endDate)
        {
            var productionOrderIds = _manufacturingDBContext.ProductionOrder
                .Where(po => po.StartDate <= endDate.UnixToDateTime() && po.EndDate >= startDate.UnixToDateTime())
                .Select(po => po.ProductionOrderId)
                .ToList();

            var productionPlans = await _manufacturingDBContext.ProductionWeekPlan
                .Include(p => p.ProductionWeekPlanDetail)
                .Where(p => productionOrderIds.Contains(p.ProductionOrderId))
                .ToListAsync();

            var result = productionPlans
                .GroupBy(p => p.ProductionOrderId)
                .ToDictionary(g => g.Key, g => g.AsQueryable().ProjectTo<ProductionWeekPlanModel>(_mapper.ConfigurationProvider).ToList());

            return result;
        }

        public async Task<IDictionary<long, List<ProductionWeekPlanModel>>> UpdateProductionPlan(IDictionary<long, List<ProductionWeekPlanModel>> data)
        {
           
            using var trans = await _manufacturingDBContext.Database.BeginTransactionAsync();
            try
            {
                foreach (var item in data)
                {
                    var productionOrderId = item.Key;

                    using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockProductionOrderKey(productionOrderId));
                    var productionOrder = _manufacturingDBContext.ProductionOrder.Where(po => po.ProductionOrderId == productionOrderId).FirstOrDefault();
                    if (productionOrder == null) throw new BadRequestException(GeneralCode.InvalidParams, "Lệnh sản xuất không tồn tại");

                    var currentProductionWeekPlans = _manufacturingDBContext.ProductionWeekPlan
                        .Where(p => p.ProductionOrderId == productionOrderId)
                        .ToList();

                    var productionWeekPlans = item.Value.AsQueryable().ProjectTo<ProductionWeekPlan>(_mapper.ConfigurationProvider).ToList();

                    foreach (var productionWeekPlan in productionWeekPlans)
                    {
                        var currentProductionWeekPlan = currentProductionWeekPlans
                            .FirstOrDefault(cp => cp.StartDate == productionWeekPlan.StartDate && cp.EndDate == productionWeekPlan.EndDate);
                        if (currentProductionWeekPlan == null)
                        {
                            // Tạo mới
                            productionWeekPlan.ProductionOrderId = productionOrderId;
                            _manufacturingDBContext.ProductionWeekPlan.Add(productionWeekPlan);
                        }
                        else if (currentProductionWeekPlan.ProductQuantity != productionWeekPlan.ProductQuantity)
                        {
                            // Update
                            currentProductionWeekPlan.ProductQuantity = productionWeekPlan.ProductQuantity;
                        }

                        // Cập nhât detail
                        if (currentProductionWeekPlan != null)
                        {
                            // Xóa dữ liệu cũ
                            var currentProductionWeekPlanDetails = _manufacturingDBContext.ProductionWeekPlanDetail.Where(pd => pd.ProductionWeekPlanId == currentProductionWeekPlan.ProductionWeekPlanId).ToList();
                            _manufacturingDBContext.ProductionWeekPlanDetail.RemoveRange(currentProductionWeekPlanDetails);
                        }
                        var productionWeekPlanDetails = productionWeekPlan.ProductionWeekPlanDetail
                            .AsQueryable()
                            .ProjectTo<ProductionWeekPlanDetail>(_mapper.ConfigurationProvider)
                            .ToList();
                        _manufacturingDBContext.ProductionWeekPlanDetail.AddRange(productionWeekPlanDetails);
                    }

                    // Xóa kế hoạch tuần 
                    var deleteProductionWeekPlans = currentProductionWeekPlans.Where(cp => !productionWeekPlans.Any(p => p.StartDate == cp.StartDate && p.EndDate == cp.EndDate)).ToList();
                    var deleteProductionWeekPlanIds = deleteProductionWeekPlans.Select(p => p.ProductionWeekPlanId).ToList();
                    var deleteProductionWeekPlanDetails = _manufacturingDBContext.ProductionWeekPlanDetail.Where(pd => deleteProductionWeekPlanIds.Contains(pd.ProductionWeekPlanId)).ToList();

                    _manufacturingDBContext.ProductionWeekPlanDetail.RemoveRange(deleteProductionWeekPlanDetails);
                    _manufacturingDBContext.ProductionWeekPlan.RemoveRange(deleteProductionWeekPlans);

                    _manufacturingDBContext.SaveChanges();
                    trans.Commit();
                    await _activityLogService.CreateLog(EnumObjectType.ProductionPlan, productionOrderId, $"Cập nhật dữ liệu kế hoạch tuần cho lệnh {productionOrder.ProductionOrderCode}", data.JsonSerialize());
                }


                var productionOrderIds = data.Select(g => g.Key).ToList();

                var productionPlans = await _manufacturingDBContext.ProductionWeekPlan
                    .Include(p => p.ProductionWeekPlanDetail)
                    .Where(p => productionOrderIds.Contains(p.ProductionOrderId))
                    .ToListAsync();

                var result = productionPlans
                    .GroupBy(p => p.ProductionOrderId)
                    .ToDictionary(g => g.Key, g => g.AsQueryable().ProjectTo<ProductionWeekPlanModel>(_mapper.ConfigurationProvider).ToList());

                return data;
            }
            catch (Exception ex)
            {
                trans.TryRollbackTransaction();
                _logger.LogError(ex, "UpdateProductPlan");
                throw;
            }
        }

        public async Task<bool> DeleteProductionPlan(long productionOrderId)
        {
            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockProductionOrderKey(productionOrderId));
            using var trans = await _manufacturingDBContext.Database.BeginTransactionAsync();
            try
            {
                var productionOrder = _manufacturingDBContext.ProductionOrder.Where(po => po.ProductionOrderId == productionOrderId).FirstOrDefault();
                if (productionOrder == null) throw new BadRequestException(GeneralCode.InvalidParams, "Lệnh sản xuất không tồn tại");

                var currentProductionWeekPlans = _manufacturingDBContext.ProductionWeekPlan
                    .Where(p => p.ProductionOrderId == productionOrderId)
                    .ToList();

                var currentProductionWeekPlanIds = currentProductionWeekPlans.Select(p => p.ProductionWeekPlanId).ToList();
                var currentProductionWeekPlanDetails = _manufacturingDBContext.ProductionWeekPlanDetail
                    .Where(pd => currentProductionWeekPlanIds.Contains(pd.ProductionWeekPlanId))
                    .ToList();

                _manufacturingDBContext.ProductionWeekPlanDetail.RemoveRange(currentProductionWeekPlanDetails);
                _manufacturingDBContext.ProductionWeekPlan.RemoveRange(currentProductionWeekPlans);

                await _activityLogService.CreateLog(EnumObjectType.ProductionPlan, productionOrderId, $"Xóa dữ liệu kế hoạch tuần cho lệnh {productionOrder.ProductionOrderCode}", currentProductionWeekPlans.JsonSerialize());
                return true;
            }
            catch (Exception ex)
            {
                trans.TryRollbackTransaction();
                _logger.LogError(ex, "DeleteProductWeekPlan");
                throw;
            }
        }
    }
}
