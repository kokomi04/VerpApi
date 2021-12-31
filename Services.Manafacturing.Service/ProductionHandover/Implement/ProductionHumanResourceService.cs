using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        private const int STOCK_DEPARTMENT_ID = -1;
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


        public async Task<ProductionHumanResourceModel> CreateProductionHumanResource(long productionOrderId, ProductionHumanResourceInputModel data)
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

        public async Task<bool> DeleteProductionHumanResource(long productionHumanResourceId)
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

        public async Task<IList<ProductionHumanResourceModel>> CreateMultipleProductionHumanResource(long productionOrderId, IList<ProductionHumanResourceInputModel> data)
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


        public async Task<IList<ProductionHumanResourceModel>> GetProductionHumanResources(long productionOrderId)
        {
            return await _manufacturingDBContext.ProductionHumanResource
                .Where(h => h.ProductionOrderId == productionOrderId)
                .ProjectTo<ProductionHumanResourceModel>(_mapper.ConfigurationProvider)
                .ToListAsync();

        }

    }
}
