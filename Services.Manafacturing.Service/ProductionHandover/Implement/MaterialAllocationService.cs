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
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Manafacturing.Model.ProductionAssignment;
using VErp.Commons.Enums.Manafacturing;
using Microsoft.Data.SqlClient;
using VErp.Services.Manafacturing.Model.ProductionHandover;

namespace VErp.Services.Manafacturing.Service.ProductionHandover.Implement
{
    public class MaterialAllocationService : IMaterialAllocationService
    {
        private readonly ManufacturingDBContext _manufacturingDBContext;
        private readonly IActivityLogService _activityLogService;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private const int STOCK_DEPARTMENT_ID = -1;
        public MaterialAllocationService(ManufacturingDBContext manufacturingDB
            , IActivityLogService activityLogService
            , ILogger<ProductionHandoverService> logger
            , IMapper mapper
            , ICustomGenCodeHelperService customGenCodeHelperService)
        {
            _manufacturingDBContext = manufacturingDB;
            _activityLogService = activityLogService;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<IList<MaterialAllocationModel>> GetMaterialAllocations(long productionOrderId)
        {
            var materialAllocations = await _manufacturingDBContext.MaterialAllocation
                .Where(ma => ma.ProductionOrderId == productionOrderId)
                .ProjectTo<MaterialAllocationModel>(_mapper.ConfigurationProvider)
                .ToListAsync();
            return materialAllocations;
        }

        public async Task<AllocationModel> UpdateMaterialAllocation(long productionOrderId, AllocationModel data)
        {
            using var trans = await _manufacturingDBContext.Database.BeginTransactionAsync();
            try
            {
                var currentMaterialAllocations = _manufacturingDBContext.MaterialAllocation.Where(ma => ma.ProductionOrderId == productionOrderId).ToList();

                foreach (var item in data.MaterialAllocations)
                {
                    var currentMaterialAllocation = currentMaterialAllocations.FirstOrDefault(ma => ma.MaterialAllocationId == item.MaterialAllocationId);

                    if (currentMaterialAllocation == null)
                    {
                        currentMaterialAllocation = _mapper.Map<MaterialAllocation>(item);
                        _manufacturingDBContext.MaterialAllocation.Add(currentMaterialAllocation);
                    }
                    else
                    {
                        currentMaterialAllocations.Remove(currentMaterialAllocation);
                        _mapper.Map(item, currentMaterialAllocation);
                    }
                }

                _manufacturingDBContext.MaterialAllocation.RemoveRange(currentMaterialAllocations);

                _manufacturingDBContext.SaveChanges();


                var currentIgnoreAllocations = _manufacturingDBContext.IgnoreAllocation
                   .Where(ia => ia.ProductionOrderId == productionOrderId)
                   .ToList();

                _manufacturingDBContext.IgnoreAllocation.RemoveRange(currentIgnoreAllocations);

                foreach (var item in data.IgnoreAllocations)
                {
                    var entity = _mapper.Map<IgnoreAllocation>(item);
                    entity.ProductionOrderId = productionOrderId;
                    _manufacturingDBContext.IgnoreAllocation.Add(entity);

                }

                _manufacturingDBContext.SaveChanges();
                trans.Commit();

                await _activityLogService.CreateLog(EnumObjectType.MaterialAllocation, productionOrderId, $"Cập nhật phân bổ vật tư sản xuât", data.JsonSerialize());

                data.MaterialAllocations = await _manufacturingDBContext.MaterialAllocation
                    .Where(ma => ma.ProductionOrderId == productionOrderId)
                    .ProjectTo<MaterialAllocationModel>(_mapper.ConfigurationProvider)
                    .ToListAsync();

                data.IgnoreAllocations = await _manufacturingDBContext.IgnoreAllocation
                    .Where(ma => ma.ProductionOrderId == productionOrderId)
                    .ProjectTo<IgnoreAllocationModel>(_mapper.ConfigurationProvider)
                    .ToListAsync();

                return data;
            }
            catch (Exception ex)
            {
                trans.Rollback();
                _logger.LogError(ex, "UpdateMaterialAllocation");
                throw;
            }
        }

        public async Task<IList<IgnoreAllocationModel>> GetIgnoreAllocations(long productionOrderId)
        {
            var ignoreAllocations = await _manufacturingDBContext.IgnoreAllocation
               .Where(ma => ma.ProductionOrderId == productionOrderId)
               .ProjectTo<IgnoreAllocationModel>(_mapper.ConfigurationProvider)
               .ToListAsync();
            return ignoreAllocations;
        }

    }
}
