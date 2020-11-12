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
using VErp.Services.Manafacturing.Model.ProductionOrder;
using VErp.Commons.Enums.Manafacturing;
using Microsoft.Data.SqlClient;

namespace VErp.Services.Manafacturing.Service.ProductionOrder.Implement
{
    public class ProductionScheduleService : IProductionScheduleService
    {
        private readonly ManufacturingDBContext _manufacturingDBContext;
        private readonly IActivityLogService _activityLogService;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly ICustomGenCodeHelperService _customGenCodeHelperService;

        public ProductionScheduleService(ManufacturingDBContext manufacturingDB
            , IActivityLogService activityLogService
            , ILogger<ProductionOrderService> logger
            , IMapper mapper
            , ICustomGenCodeHelperService customGenCodeHelperService)
        {
            _manufacturingDBContext = manufacturingDB;
            _activityLogService = activityLogService;
            _logger = logger;
            _mapper = mapper;
            _customGenCodeHelperService = customGenCodeHelperService;
        }

        public async Task<ProductionScheduleModel> CreateProductionSchedule(ProductionScheduleModel data)
        {
            throw new NotImplementedException();
        }

        public async Task<PageData<ProductionScheduleModel>> GetProductionSchedule(string keyword, int page, int size, Clause filters = null)
        {
            throw new NotImplementedException();
        }

        public async Task<ProductionScheduleModel> UpdateProductionSchedule(int productionOrderDetailId, ProductionScheduleModel data)
        {
            throw new NotImplementedException();
        }
    }
}
