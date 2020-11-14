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
using ProductionAssignmentEntity = VErp.Infrastructure.EF.ManufacturingDB.ProductionAssignment;

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

        public async Task<ProductionAssignmentModel> CreateProductionAssignment(ProductionAssignmentModel data)
        {
            var productionSchedule = _manufacturingDBContext.ProductionSchedule.FirstOrDefault(s => s.ProductionScheduleId == data.ProductionScheduleId);
            if (productionSchedule == null)
                throw new BadRequestException(GeneralCode.InvalidParams, "Kế hoạch sản xuất không tồn tại");
            var productionStep = _manufacturingDBContext.ProductionStep.FirstOrDefault(s => s.ProductionStepId == data.ProductionStepId);
            if (productionStep == null)
                throw new BadRequestException(GeneralCode.InvalidParams, "Công đoạn sản xuất không tồn tại");
            if (_manufacturingDBContext.ProductionStep.Any(s => s.ParentId == productionStep.ProductionStepId))
                throw new BadRequestException(GeneralCode.InvalidParams, "Không được phân công công việc cho quy trình con");
            try
            {

                var productionAssignment = _mapper.Map<ProductionAssignmentEntity>(data);
                _manufacturingDBContext.ProductionAssignment.Add(productionAssignment);
                await _activityLogService.CreateLog(EnumObjectType.ProductionAssignment, productionAssignment.ProductionStepId, $"Phân công sản xuất cho kế hoạch {productionSchedule.ProductionScheduleId}", data.JsonSerialize());

                _manufacturingDBContext.SaveChanges();

                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreateProductAssignment");
                throw;
            }
        }

        public async Task<bool> DeleteProductionAssignment(ProductionAssignmentModel data)
        {
            var productionAssignment = _manufacturingDBContext.ProductionAssignment
                .FirstOrDefault(s => s.ProductionScheduleId == data.ProductionScheduleId
                && s.ProductionStepId == data.ProductionStepId
                && s.DepartmentId == data.DepartmentId);
            if (productionAssignment == null)
                throw new BadRequestException(GeneralCode.InvalidParams, "Thông tin phân công công việc không tồn tại");

            try
            {
                _manufacturingDBContext.ProductionAssignment.Remove(productionAssignment);
                await _activityLogService.CreateLog(EnumObjectType.ProductionAssignment, productionAssignment.ProductionStepId, $"Xóa phân công sản xuất cho kế hoạch {productionAssignment.ProductionScheduleId}", data.JsonSerialize());

                _manufacturingDBContext.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeleteProductAssignment");
                throw;
            }
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

        public async Task<ProductionAssignmentModel> UpdateProductionAssignment(ProductionAssignmentModel data)
        {
            var productionAssignment = _manufacturingDBContext.ProductionAssignment
                .FirstOrDefault(s => s.ProductionScheduleId == data.ProductionScheduleId 
                && s.ProductionStepId == data.ProductionStepId
                && s.DepartmentId == data.DepartmentId);
            if (productionAssignment == null)
                throw new BadRequestException(GeneralCode.InvalidParams, "Thông tin phân công công việc không tồn tại");
            
            try
            {
                productionAssignment.AssignmentQuantity = data.AssignmentQuantity;
                await _activityLogService.CreateLog(EnumObjectType.ProductionAssignment, productionAssignment.ProductionStepId, $"Cập nhật phân công sản xuất cho kế hoạch {productionAssignment.ProductionScheduleId}", data.JsonSerialize());

                _manufacturingDBContext.SaveChanges();

                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateProductAssignment");
                throw;
            }
        }
    }
}
