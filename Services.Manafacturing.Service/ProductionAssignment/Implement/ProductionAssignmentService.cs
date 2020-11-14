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

namespace VErp.Services.Manafacturing.Service.ProductionAssignment.Implement
{
    public class ProductionAssignmentService : IProductionAssignmentService
    {
        private readonly ManufacturingDBContext _manufacturingDBContext;
        private readonly IActivityLogService _activityLogService;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly ICustomGenCodeHelperService _customGenCodeHelperService;

        public ProductionAssignmentService(ManufacturingDBContext manufacturingDB
            , IActivityLogService activityLogService
            , ILogger<ProductionAssignmentService> logger
            , IMapper mapper
            , ICustomGenCodeHelperService customGenCodeHelperService)
        {
            _manufacturingDBContext = manufacturingDB;
            _activityLogService = activityLogService;
            _logger = logger;
            _mapper = mapper;
            _customGenCodeHelperService = customGenCodeHelperService;
        }

        public async Task<IList<ProductionAssignmentModel>> CreateProductionAssignment(ProductionAssignmentModel data)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> DeleteProductionAssignment(int productionAssignmentId)
        {
            throw new NotImplementedException();
        }

        public async Task<IList<ProductionAssignmentModel>> GetProductionAssignments(long scheduleTurnId)
        {
            var scheduleIds = _manufacturingDBContext.ProductionSchedule
                .Where(s => s.ScheduleTurnId == scheduleTurnId)
                .Select(s => s.ProductionScheduleId)
                .ToList();
            return await _manufacturingDBContext.ProductionAssignment
                .Where(a => scheduleIds.Contains(a.ProductionScheduleId))
                .ProjectTo<ProductionAssignmentModel>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        public async Task<IList<ProductionAssignmentModel>> UpdateProductionAssignment(ProductionAssignmentModel data)
        {
            throw new NotImplementedException();
        }
    }
}
