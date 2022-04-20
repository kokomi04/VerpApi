using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.ManufacturingDB;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Manafacturing.Model.ProductionHandover;

namespace VErp.Services.Manafacturing.Service.ProductionHandover.Implement
{
    public class ProductionHumanResourceService : IProductionHumanResourceService
    {
        private readonly ManufacturingDBContext _manufacturingDBContext;
        private readonly IActivityLogService _activityLogService;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        public ProductionHumanResourceService(ManufacturingDBContext manufacturingDB
            , IActivityLogService activityLogService
            , ILogger<ProductionHumanResourceService> logger
            , IMapper mapper)
        {
            _manufacturingDBContext = manufacturingDB;
            _activityLogService = activityLogService;
            _logger = logger;
            _mapper = mapper;
        }


        public async Task<ProductionHumanResourceModel> Create(long productionOrderId, ProductionHumanResourceInputModel data)
        {
            try
            {
                var productionHumanResource = _mapper.Map<ProductionHumanResource>(data);
                productionHumanResource.ProductionOrderId = productionOrderId;
                _manufacturingDBContext.ProductionHumanResource.Add(productionHumanResource);
                _manufacturingDBContext.SaveChanges();

                await _activityLogService.CreateLog(EnumObjectType.ProductionHumanResource, productionHumanResource.ProductionHumanResourceId, $"Tạo thống kê nhân công sản xuất", data.JsonSerialize());
                return _mapper.Map<ProductionHumanResourceModel>(productionHumanResource);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreateProductionHumanResource");
                throw;
            }
        }

        public async Task<bool> Delete(long productionHumanResourceId)
        {
            try
            {
                var productionHumanResource = _manufacturingDBContext.ProductionHumanResource
                    .Where(h => h.ProductionHumanResourceId == productionHumanResourceId)
                    .FirstOrDefault();

                if (productionHumanResource == null)
                    throw new BadRequestException(GeneralCode.InvalidParams, "Không tồn tại thống kê nhân công");
                productionHumanResource.IsDeleted = true;
                _manufacturingDBContext.SaveChanges();
                await _activityLogService.CreateLog(EnumObjectType.ProductionHumanResource, productionHumanResourceId, $"Xoá thống kê nhân công", productionHumanResource.JsonSerialize());
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeleteProductionHumanResource");
                throw;
            }
        }

        public async Task<IList<ProductionHumanResourceModel>> CreateMultiple(long productionOrderId, IList<ProductionHumanResourceInputModel> data)
        {
            var insertData = new List<ProductionHumanResource>();
            using var trans = await _manufacturingDBContext.Database.BeginTransactionAsync();
            try
            {
                var currentProductionHumanResources = _manufacturingDBContext.ProductionHumanResource.Where(r => r.ProductionOrderId == productionOrderId).ToList();


                foreach (var item in data)
                {
                    var current = currentProductionHumanResources.FirstOrDefault(r => r.ProductionHumanResourceId == item.ProductionHumanResourceId);
                    // Thêm mới
                    if (current == null)
                    {
                        var productionHumanResource = _mapper.Map<ProductionHumanResource>(item);
                        productionHumanResource.ProductionOrderId = productionOrderId;
                        _manufacturingDBContext.ProductionHumanResource.Add(productionHumanResource);
                        insertData.Add(productionHumanResource);
                    }
                    else // Cập nhật
                    {
                        _mapper.Map(item, current);
                        currentProductionHumanResources.Remove(current);
                    }
                }

                // Xóa
                _manufacturingDBContext.ProductionHumanResource.RemoveRange(currentProductionHumanResources);

                _manufacturingDBContext.SaveChanges();

                var result = insertData.AsQueryable().ProjectTo<ProductionHumanResourceModel>(_mapper.ConfigurationProvider).ToList();

                foreach (var item in insertData)
                {
                    await _activityLogService.CreateLog(EnumObjectType.ProductionHumanResource, item.ProductionHumanResourceId, $"Tạo thống kê nhân công", data.JsonSerialize());
                }

                trans.Commit();

                return result;
            }
            catch (Exception ex)
            {
                trans.Rollback();
                _logger.LogError(ex, "CreateMultipleProductionHumanResource");
                throw;
            }
        }


        public async Task<IList<ProductionHumanResourceModel>> GetByProductionOrder(long productionOrderId)
        {
            return await _manufacturingDBContext.ProductionHumanResource
                .Where(h => h.ProductionOrderId == productionOrderId)
                .ProjectTo<ProductionHumanResourceModel>(_mapper.ConfigurationProvider)
                .ToListAsync();

        }

        public async Task<IList<ProductionHumanResourceModel>> GetByDepartment(int departmentId, long startDate, long endDate)
        {
            DateTime start = startDate.UnixToDateTime().Value;
            DateTime end = endDate.UnixToDateTime().Value;

            var result = await _manufacturingDBContext.ProductionHumanResource
               .Where(r => r.DepartmentId == departmentId && r.Date >= start && r.Date <= end)
               .ProjectTo<ProductionHumanResourceModel>(_mapper.ConfigurationProvider)
               .ToListAsync();

            var productionOrderIds = result.Select(r => r.ProductionOrderId).Distinct().ToList();
            var productionStepIds = result.Select(r => r.ProductionStepId).Distinct().ToList();

            var productioStepTiltes = _manufacturingDBContext.ProductionStep
                .Where(ps => productionStepIds.Contains(ps.ProductionStepId))
                .Select(ps => new
                {
                    ps.ProductionStepId,
                    ps.Title
                })
                .ToDictionary(ps => ps.ProductionStepId, po => po.Title);


            var productionOrderCodes = _manufacturingDBContext.ProductionOrder
                .Where(po => productionOrderIds.Contains(po.ProductionOrderId))
                .Select(po => new
                {
                    po.ProductionOrderId,
                    po.ProductionOrderCode
                })
                .ToDictionary(po => po.ProductionOrderId, po => po.ProductionOrderCode);

            foreach (var item in result)
            {
                if (productionOrderCodes.ContainsKey(item.ProductionOrderId))
                {
                    item.ProductionOrderCode = productionOrderCodes[item.ProductionOrderId];
                }
                if (productioStepTiltes.ContainsKey(item.ProductionStepId))
                {
                    item.ProductionStepTitle = productioStepTiltes[item.ProductionStepId];
                }
            }

            return result;
        }

        public async Task<IList<UnFinishProductionInfo>> GetUnFinishProductionInfo(int departmentId, long startDate, long endDate)
        {
            DateTime start = startDate.UnixToDateTime().Value;
            DateTime end = endDate.UnixToDateTime().Value;

            var productionAssignemts = await _manufacturingDBContext.ProductionAssignment
                    .Where(a => a.DepartmentId == departmentId && a.EndDate >= start && a.StartDate <= end && a.AssignedProgressStatus != (int)EnumAssignedProgressStatus.Finish)
                    .Select(a => new
                    {
                        a.ProductionOrderId,
                        a.ProductionStepId
                    })
                    .ToListAsync();

            var productionOrderIds = productionAssignemts.Select(a => a.ProductionOrderId).Distinct().ToList();

            var productionOrders = _manufacturingDBContext.ProductionOrder
                .Where(po => productionOrderIds.Contains(po.ProductionOrderId) && po.ProductionOrderStatus != (int)EnumProductionStatus.Finished)
                .Select(po => new
                {
                    po.ProductionOrderId,
                    po.ProductionOrderCode
                })
                .ToList();

            var groupIds = productionAssignemts.Select(a => a.ProductionStepId).Distinct().ToList();
            var productionStepIds = _manufacturingDBContext.ProductionStep
               .Where(ps => groupIds.Contains(ps.ProductionStepId) && ps.ParentId.HasValue)
               .Select(ps => ps.ParentId)
               .Distinct()
               .ToList();

            var productionSteps = _manufacturingDBContext.ProductionStep
               .Where(ps => productionStepIds.Contains(ps.ProductionStepId))
               .Select(ps => new
               {
                   ps.ProductionStepId,
                   ps.ContainerId,
                   ps.Title
               })
               .ToList()
               .GroupBy(ps => ps.ContainerId)
               .ToDictionary(a => a.Key, g => g.Select(ps => new UnFinishProductionStepInfo
               {
                   ProductionStepId = ps.ProductionStepId,
                   ProductionStepTitle = ps.Title
               })
               .ToList());

            var result = new List<UnFinishProductionInfo>();

            foreach (var productionOrder in productionOrders)
            {
                if(productionSteps.ContainsKey(productionOrder.ProductionOrderId) && productionSteps[productionOrder.ProductionOrderId].Count > 0)
                {
                    result.Add(new UnFinishProductionInfo
                    {
                        ProductionOrderId = productionOrder.ProductionOrderId,
                        ProductionOrderCode = productionOrder.ProductionOrderCode,
                        ProductionStep = productionSteps[productionOrder.ProductionOrderId]
                    });
                }
            }
            return result;
        }

        public async Task<IList<ProductionHumanResourceModel>> CreateMultipleByDepartment(int departmentId, long startDate, long endDate, IList<ProductionHumanResourceInputModel> data)
        {
            var insertData = new List<ProductionHumanResource>();
            using var trans = await _manufacturingDBContext.Database.BeginTransactionAsync();
            try
            {
                DateTime start = startDate.UnixToDateTime().Value;
                DateTime end = endDate.UnixToDateTime().Value;

                var currentProductionHumanResources = _manufacturingDBContext.ProductionHumanResource
                    .Where(r => r.DepartmentId == departmentId && r.Date >= start && r.Date <= end).ToList();

                foreach (var item in data)
                {
                    var current = currentProductionHumanResources.FirstOrDefault(r => r.ProductionHumanResourceId == item.ProductionHumanResourceId);
                    // Thêm mới
                    if (current == null)
                    {
                        var productionHumanResource = _mapper.Map<ProductionHumanResource>(item);
                        _manufacturingDBContext.ProductionHumanResource.Add(productionHumanResource);
                        insertData.Add(productionHumanResource);
                    }
                    else // Cập nhật
                    {
                        _mapper.Map(item, current);
                        currentProductionHumanResources.Remove(current);
                    }
                }

                // Xóa
                _manufacturingDBContext.ProductionHumanResource.RemoveRange(currentProductionHumanResources);

                _manufacturingDBContext.SaveChanges();

                var result = insertData.AsQueryable().ProjectTo<ProductionHumanResourceModel>(_mapper.ConfigurationProvider).ToList();

                foreach (var item in insertData)
                {
                    await _activityLogService.CreateLog(EnumObjectType.ProductionHumanResource, item.ProductionHumanResourceId, $"Tạo thống kê nhân công", data.JsonSerialize());
                }

                trans.Commit();

                return result;
            }
            catch (Exception ex)
            {
                trans.Rollback();
                _logger.LogError(ex, "CreateMultipleProductionHumanResource");
                throw;
            }
        }

    }

}
