﻿using AutoMapper;
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
            var productionOrderDetailIds = _manufacturingDBContext.ProductionOrderDetail
                .Include(pod => pod.ProductionOrder)
                .Where(pod => pod.ProductionOrder.StartDate <= endDate.UnixToDateTime() && pod.ProductionOrder.EndDate >= startDate.UnixToDateTime())
                .Select(pod => pod.ProductionOrderDetailId)
                .ToList();

            var productionPlans = await _manufacturingDBContext.ProductionWeekPlan
                .Include(p => p.ProductionWeekPlanDetail)
                .Where(p => productionOrderDetailIds.Contains(p.ProductionOrderDetailId))
                .ToListAsync();

            var result = productionPlans
                .GroupBy(p => p.ProductionOrderDetailId)
                .ToDictionary(g => g.Key, g => g.AsQueryable().ProjectTo<ProductionWeekPlanModel>(_mapper.ConfigurationProvider).ToList());

            return result;
        }

        public async Task<IDictionary<long, List<ProductionWeekPlanModel>>> UpdateProductionPlan(IDictionary<long, List<ProductionWeekPlanModel>> data)
        {

            using var trans = await _manufacturingDBContext.Database.BeginTransactionAsync();
            try
            {
                var productionOrderDetailIds = data.Select(d => d.Key).ToList();
                var productionOrderDetails = _manufacturingDBContext.ProductionOrderDetail
                    .Include(pod => pod.ProductionOrder)
                    .Where(pod => productionOrderDetailIds.Contains(pod.ProductionOrderDetailId))
                    .ToList();
                if (productionOrderDetails.Count != productionOrderDetailIds.Count)
                    throw new BadRequestException(GeneralCode.InvalidParams, "Chi tiết lệnh sản xuất không tồn tại");

                var allProductionWeekPlans = _manufacturingDBContext.ProductionWeekPlan
                        .Where(p => productionOrderDetailIds.Contains(p.ProductionOrderDetailId))
                        .ToList();
                var allProductionWeekPlanIds = allProductionWeekPlans.Select(p => p.ProductionWeekPlanId).ToList();
                var allProductionWeekPlanDetails = _manufacturingDBContext.ProductionWeekPlanDetail
                     .Where(pd => allProductionWeekPlanIds.Contains(pd.ProductionWeekPlanId))
                     .ToList();

                foreach (var item in data)
                {
                    var productionOrderDetailId = item.Key;
                    var productionOrderDetail = productionOrderDetails.First(pod => pod.ProductionOrderDetailId == productionOrderDetailId);
                    var currentProductionWeekPlans = allProductionWeekPlans
                        .Where(p => p.ProductionOrderDetailId == productionOrderDetailId)
                        .ToList();

                    foreach (var productionWeekPlanModel in item.Value)
                    {
                        var currentProductionWeekPlan = currentProductionWeekPlans
                            .FirstOrDefault(cp => cp.StartDate == productionWeekPlanModel.StartDate.UnixToDateTime());
                        if (currentProductionWeekPlan == null)
                        {
                            // Tạo mới
                            currentProductionWeekPlan = _mapper.Map<ProductionWeekPlan>(productionWeekPlanModel);
                            currentProductionWeekPlan.ProductionOrderDetailId = productionOrderDetailId;
                            _manufacturingDBContext.ProductionWeekPlan.Add(currentProductionWeekPlan);
                            _manufacturingDBContext.SaveChanges();
                        }
                        else if (currentProductionWeekPlan.ProductQuantity != productionWeekPlanModel.ProductQuantity)
                        {
                            // Update
                            currentProductionWeekPlan.ProductQuantity = productionWeekPlanModel.ProductQuantity;
                        }
                        // Cập nhât detail
                        if (currentProductionWeekPlan != null)
                        {
                            // Xóa dữ liệu cũ
                            var currentProductionWeekPlanDetails = allProductionWeekPlanDetails.Where(pd => pd.ProductionWeekPlanId == currentProductionWeekPlan.ProductionWeekPlanId).ToList();
                            _manufacturingDBContext.ProductionWeekPlanDetail.RemoveRange(currentProductionWeekPlanDetails);
                        }
                        foreach(var detail in productionWeekPlanModel.ProductionWeekPlanDetail)
                        {
                            var productionWeekPlanDetail = _mapper.Map<ProductionWeekPlanDetail>(detail);
                            productionWeekPlanDetail.ProductionWeekPlanId = currentProductionWeekPlan.ProductionWeekPlanId;
                            _manufacturingDBContext.ProductionWeekPlanDetail.AddRange(productionWeekPlanDetail);
                        }
                        
                    }


                    // Xóa kế hoạch tuần 
                    var deleteProductionWeekPlans = currentProductionWeekPlans.Where(cp => !item.Value.Any(p => p.StartDate.UnixToDateTime() == cp.StartDate)).ToList();
                    var deleteProductionWeekPlanIds = deleteProductionWeekPlans.Select(p => p.ProductionWeekPlanId).ToList();
                    var deleteProductionWeekPlanDetails = allProductionWeekPlanDetails.Where(pd => deleteProductionWeekPlanIds.Contains(pd.ProductionWeekPlanId)).ToList();

                    _manufacturingDBContext.ProductionWeekPlanDetail.RemoveRange(deleteProductionWeekPlanDetails);
                    _manufacturingDBContext.ProductionWeekPlan.RemoveRange(deleteProductionWeekPlans);

                    _manufacturingDBContext.SaveChanges();
                }

                trans.Commit();

                foreach (var item in data)
                {
                    var productionOrderDetailId = item.Key;
                    var productionOrderDetail = productionOrderDetails.First(pod => pod.ProductionOrderDetailId == productionOrderDetailId);
                    await _activityLogService.CreateLog(EnumObjectType.ProductionPlan, productionOrderDetail.ProductionOrderId, $"Cập nhật dữ liệu kế hoạch tuần cho lệnh {productionOrderDetail.ProductionOrder.ProductionOrderCode}", data.JsonSerialize());
                }

                var productionPlans = await _manufacturingDBContext.ProductionWeekPlan
                    .Include(p => p.ProductionWeekPlanDetail)
                    .Where(p => productionOrderDetailIds.Contains(p.ProductionOrderDetailId))
                    .ToListAsync();

                var result = productionPlans
                    .GroupBy(p => p.ProductionOrderDetailId)
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
            using var trans = await _manufacturingDBContext.Database.BeginTransactionAsync();
            try
            {
                var productionOrder = _manufacturingDBContext.ProductionOrder
                    .Include(po => po.ProductionOrderDetail)
                    .Where(po => po.ProductionOrderId == productionOrderId).FirstOrDefault();
                if (productionOrder == null) throw new BadRequestException(GeneralCode.InvalidParams, "Lệnh sản xuất không tồn tại");

                var productionOrderDetailIds = productionOrder.ProductionOrderDetail.Select(pod => pod.ProductionOrderDetailId).ToList();

                var currentProductionWeekPlans = _manufacturingDBContext.ProductionWeekPlan
                    .Where(p => productionOrderDetailIds.Contains(p.ProductionOrderDetailId))
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