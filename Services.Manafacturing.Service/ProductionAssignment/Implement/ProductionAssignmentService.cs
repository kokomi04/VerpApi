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
using static VErp.Commons.Enums.Manafacturing.EnumProductionProcess;

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


        public async Task<IList<ProductionAssignmentModel>> GetProductionAssignments(long scheduleTurnId)
        {
            var dataSql = "SELECT * FROM vProductionAssignment v WHERE v.ScheduleTurnId = @ScheduleTurnId";
            var sqlParams = new SqlParameter[]
            {
                new SqlParameter("@ScheduleTurnId", scheduleTurnId)
            };

            var resultData = await _manufacturingDBContext.QueryDataTable(dataSql, sqlParams);
            return resultData.ConvertData<ProductionAssignmentModel>();
        }

        public async Task<bool> UpdateProductionAssignment(long productionStepId, long scheduleTurnId, ProductionAssignmentModel[] data)
        {
            // Validate
            var step = _manufacturingDBContext.ProductionStep
                .Include(s => s.ProductionStepLinkDataRole)
                .ThenInclude(r => r.ProductionStepLinkData)
                .Where(s => s.ProductionStepId == productionStepId)
                .FirstOrDefault();

            if (data.Any(a => a.ScheduleTurnId != scheduleTurnId || a.ProductionStepId != productionStepId))
                new BadRequestException(GeneralCode.InvalidParams, "Thông tin lượt kế hoạch hoặc công đoạn sản xuất giữa các tổ không khớp");


            if (step == null) throw new BadRequestException(GeneralCode.InvalidParams, "Công đoạn sản xuất không tồn tại");

            var productionSchedules = (from s in _manufacturingDBContext.ProductionSchedule
                                       join od in _manufacturingDBContext.ProductionOrderDetail on s.ProductionOrderDetailId equals od.ProductionOrderDetailId
                                       where s.ScheduleTurnId == scheduleTurnId
                                       select new
                                       {
                                           s.ProductionOrderDetailId,
                                           s.ProductionScheduleQuantity,
                                           od.ProductId
                                       }).ToList();

            var productionOrderDetailIds = productionSchedules.Select(s => s.ProductionOrderDetailId).ToList();

            if (productionOrderDetailIds.Count == 0) throw new BadRequestException(GeneralCode.InvalidParams, "Kế hoạch sản xuất không tồn tại");

            if (!_manufacturingDBContext.ProductionStepOrder.Any(so => productionOrderDetailIds.Contains(so.ProductionOrderDetailId) && so.ProductionStepId == productionStepId))
            {
                throw new BadRequestException(GeneralCode.InvalidParams, "Công đoạn sản xuất không tồn tại trong quy trình sản xuất");
            }

            var quantityMap = productionSchedules.GroupBy(s => s.ProductId).ToDictionary(g => g.Key, g => g.Sum(v => v.ProductionScheduleQuantity));

            var linkDatas = step.ProductionStepLinkDataRole
                .Where(r => r.ProductionStepLinkDataRoleTypeId == (int)EnumProductionProcess.ProductionStepLinkDataRoleType.Output)
                .Select(r => new
                {
                    r.ProductionStepLinkData.ObjectTypeId,
                    r.ProductionStepLinkData.ObjectId,
                    Quantity = r.ProductionStepLinkData.Quantity * quantityMap[r.ProductionStepLinkData.ProductId]
                })
                .GroupBy(d => new { d.ObjectTypeId, d.ObjectId })
                .ToDictionary(g => g.Key, g => g.Sum(v => v.Quantity));

            if (data.Any(d => d.AssignmentQuantity <= 0))
                throw new BadRequestException(GeneralCode.InvalidParams, "Số lượng phân công phải lớn hơn 0");

            foreach (var linkData in linkDatas)
            {
                decimal totalAssignmentQuantity = 0;

                foreach (var assignment in data)
                {
                    var sourceData = linkDatas[new { ObjectTypeId = (int)assignment.ObjectTypeId, assignment.ObjectId }];
                    totalAssignmentQuantity += assignment.AssignmentQuantity * linkData.Value / sourceData;
                }

                if (totalAssignmentQuantity > linkData.Value)
                    throw new BadRequestException(GeneralCode.InvalidParams, "Số lượng phân công lớn hơn số lượng trong kế hoạch sản xuất");
            }

            try
            {
                var oldProductionAssignments = _manufacturingDBContext.ProductionAssignment
                    .Where(s => s.ScheduleTurnId == scheduleTurnId && s.ProductionStepId == productionStepId)
                    .ToList();
                if (oldProductionAssignments.Count > 0)
                {
                    _manufacturingDBContext.ProductionAssignment.RemoveRange(oldProductionAssignments);
                    _manufacturingDBContext.SaveChanges();
                }
                var newProductionAssignments = data.AsQueryable().ProjectTo<ProductionAssignmentEntity>(_mapper.ConfigurationProvider).ToList();
                _manufacturingDBContext.ProductionAssignment.AddRange(newProductionAssignments);

                _manufacturingDBContext.SaveChanges();

                await _activityLogService.CreateLog(EnumObjectType.ProductionAssignment, productionStepId, $"Cập nhật phân công sản xuất cho nhóm kế hoạch {scheduleTurnId}", data.JsonSerialize());

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateProductAssignment");
                throw;
            }
        }


        public async Task<PageData<DepartmentProductionAssignmentModel>> DepartmentProductionAssignment(int departmentId, int page, int size, string orderByFieldName, bool asc)
        {
            var assignmentQuery = (
                from a in _manufacturingDBContext.ProductionAssignment
                join t in _manufacturingDBContext.ProductionSchedule on a.ScheduleTurnId equals t.ScheduleTurnId
                join s in _manufacturingDBContext.ProductionStep.Where(s => s.ContainerTypeId == (int)EnumContainerType.ProductionOrder) on a.ProductionStepId equals s.ProductionStepId
                join o in _manufacturingDBContext.ProductionOrder on s.ContainerId equals o.ProductionOrderId
                join od in _manufacturingDBContext.ProductionOrderDetail on t.ProductionOrderDetailId equals od.ProductionOrderDetailId
                where a.DepartmentId == departmentId
                select new
                {
                    a.ScheduleTurnId,
                    o.ProductionOrderId,
                    o.ProductionOrderCode,
                    od.OrderDetailId,
                    od.ProductId,
                    t.StartDate,
                    t.EndDate,
                    t.ProductionScheduleStatus
                })
                .Distinct();
            var total = await assignmentQuery.CountAsync();
            if (string.IsNullOrWhiteSpace(orderByFieldName))
            {
                orderByFieldName = nameof(DepartmentProductionAssignmentModel.StartDate);
            }
            var pagedData = await assignmentQuery.SortByFieldName(orderByFieldName, asc).Skip((page - 1) * size).Take(size).ToListAsync();
            return (pagedData.Select(d => new DepartmentProductionAssignmentModel()
            {
                ScheduleTurnId = d.ScheduleTurnId,
                ProductionOrderId = d.ProductionOrderId,
                ProductionOrderCode = d.ProductionOrderCode,
                OrderDetailId = d.OrderDetailId,
                ProductId = d.ProductId,
                StartDate = d.StartDate.GetUnix(),
                EndDate = d.EndDate.GetUnix(),
                ProductionScheduleStatus = (EnumScheduleStatus)d.ProductionScheduleStatus
            }).ToList(), total);
        }

        public async Task<IList<DepartmentProductionAssignmentDetailModel>> DepartmentScheduleTurnAssignment(int departmentId, long scheduleTurnId)
        {
            var assignmentQuery = (
                from a in _manufacturingDBContext.ProductionAssignment
                join t in _manufacturingDBContext.ProductionSchedule on a.ScheduleTurnId equals t.ScheduleTurnId
                join s in _manufacturingDBContext.ProductionStep.Where(s => s.ContainerTypeId == (int)EnumContainerType.ProductionOrder) on a.ProductionStepId equals s.ProductionStepId
                join o in _manufacturingDBContext.ProductionOrder on s.ContainerId equals o.ProductionOrderId
                join od in _manufacturingDBContext.ProductionOrderDetail on t.ProductionOrderDetailId equals od.ProductionOrderDetailId
                where a.DepartmentId == departmentId && a.ScheduleTurnId == scheduleTurnId
                select new
                {
                    a.ScheduleTurnId,
                    o.ProductionOrderId,
                    o.ProductionOrderCode,
                    od.OrderDetailId,
                    od.ProductId,
                    t.StartDate,
                    t.EndDate,
                    t.ProductionScheduleStatus,

                    a.ProductionStepId,
                    t.ProductionScheduleQuantity,
                    a.AssignmentQuantity,
                    a.ObjectTypeId,
                    a.ObjectId
                });

            return (await assignmentQuery.ToListAsync())
                .Select(d => new DepartmentProductionAssignmentDetailModel()
                {
                    ScheduleTurnId = d.ScheduleTurnId,
                    ProductionOrderId = d.ProductionOrderId,
                    ProductionOrderCode = d.ProductionOrderCode,
                    OrderDetailId = d.OrderDetailId,
                    ProductId = d.ProductId,
                    StartDate = d.StartDate.GetUnix(),
                    EndDate = d.EndDate.GetUnix(),
                    ProductionScheduleStatus = (EnumScheduleStatus)d.ProductionScheduleStatus,

                    ProductionStepId = d.ProductionStepId,
                    ProductionScheduleQuantity = d.ProductionScheduleQuantity,
                    AssignmentQuantity = d.AssignmentQuantity,
                    ObjectTypeId = d.ObjectTypeId,
                    ObjectId = d.ObjectId
                })
                .ToList();
        }
    }
}
