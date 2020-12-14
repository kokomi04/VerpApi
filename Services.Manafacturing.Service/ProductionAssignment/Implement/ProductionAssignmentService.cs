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
using VErp.Services.Manafacturing.Model.ProductionStep;

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
                                           od.ProductId,
                                           ProductionOrderQuantity = od.Quantity + od.ReserveQuantity
                                       }).ToList();

            var previousScheduleQuantities = (from s in _manufacturingDBContext.ProductionSchedule
                                              join od in _manufacturingDBContext.ProductionOrderDetail on s.ProductionOrderDetailId equals od.ProductionOrderDetailId
                                              where s.ProductionOrderDetailId == productionSchedules[0].ProductionOrderDetailId && s.ScheduleTurnId < scheduleTurnId
                                              select new
                                              {
                                                  ProductionOrderQuantity = od.Quantity + od.ReserveQuantity,
                                                  s.ProductionScheduleQuantity
                                              }).ToList();

            var isFinal = previousScheduleQuantities.Sum(s => s.ProductionScheduleQuantity) + productionSchedules[0].ProductionScheduleQuantity >= productionSchedules[0].ProductionOrderQuantity;

            var productionOrderDetailIds = productionSchedules.Select(s => s.ProductionOrderDetailId).ToList();

            if (productionOrderDetailIds.Count == 0) throw new BadRequestException(GeneralCode.InvalidParams, "Kế hoạch sản xuất không tồn tại");

            if (!_manufacturingDBContext.ProductionStepOrder
                .Any(so => productionOrderDetailIds.Contains(so.ProductionOrderDetailId) && so.ProductionStepId == productionStepId))
            {
                throw new BadRequestException(GeneralCode.InvalidParams, "Công đoạn sản xuất không tồn tại trong quy trình sản xuất");
            }

            var linkDatas = step.ProductionStepLinkDataRole
                .Where(r => r.ProductionStepLinkDataRoleTypeId == (int)EnumProductionStepLinkDataRoleType.Output)
                .ToDictionary(r => r.ProductionStepLinkDataId,
                r =>
                {
                    if(isFinal)
                    {
                        return r.ProductionStepLinkData.Quantity - previousScheduleQuantities.Sum(s => Math.Round(r.ProductionStepLinkData.Quantity * s.ProductionScheduleQuantity / s.ProductionOrderQuantity.Value, 5));
                    }
                    else
                    {
                        return Math.Round(r.ProductionStepLinkData.Quantity * productionSchedules[0].ProductionScheduleQuantity / productionSchedules[0].ProductionOrderQuantity.Value,5);
                    }
                });

            if (data.Any(d => d.AssignmentQuantity <= 0))
                throw new BadRequestException(GeneralCode.InvalidParams, "Số lượng phân công phải lớn hơn 0");

            // Lấy thông tin outsource
            var outSource = step.ProductionStepLinkDataRole
                .Where(r => r.ProductionStepLinkDataRoleTypeId == (int)EnumProductionStepLinkDataRoleType.Input && r.ProductionStepLinkData.OutsourceQuantity.HasValue)
                .FirstOrDefault();

            foreach (var linkData in linkDatas)
            {
                decimal totalAssignmentQuantity = 0;

                if(outSource != null)
                {
                    totalAssignmentQuantity += linkData.Value * outSource.ProductionStepLinkData.OutsourceQuantity.Value / outSource.ProductionStepLinkData.Quantity;
                }

                foreach (var assignment in data)
                {
                    var sourceData = linkDatas[assignment.ProductionStepLinkDataId];
                    totalAssignmentQuantity += assignment.AssignmentQuantity * linkData.Value / sourceData;
                }

                if (totalAssignmentQuantity > linkData.Value)
                    throw new BadRequestException(GeneralCode.InvalidParams, "Số lượng phân công lớn hơn số lượng trong kế hoạch sản xuất");
            }

            var oldProductionAssignments = _manufacturingDBContext.ProductionAssignment
                    .Where(s => s.ScheduleTurnId == scheduleTurnId && s.ProductionStepId == productionStepId)
                    .ToList();

            var updateAssignments = new List<(ProductionAssignmentEntity Entity, ProductionAssignmentModel Model)>();
            var newAssignments = new List<ProductionAssignmentModel>();
            foreach (var item in data)
            {
                var entity = oldProductionAssignments.FirstOrDefault(a => a.DepartmentId == item.DepartmentId);
                if (entity == null)
                {
                    newAssignments.Add(item);
                }
                else
                {
                    if (entity.AssignmentQuantity != item.AssignmentQuantity || entity.ProductionStepLinkDataId != item.ProductionStepLinkDataId)
                    {
                        updateAssignments.Add((entity, item));
                    }
                    oldProductionAssignments.Remove(entity);
                }
            }

            // Validate khai báo chi phí
            var deleteAssignDepartmentIds = oldProductionAssignments.Select(a => a.DepartmentId).ToList();
            if (_manufacturingDBContext.ProductionScheduleTurnShift
                .Any(s => s.ScheduleTurnId == scheduleTurnId && s.ProductionStepId == productionStepId && deleteAssignDepartmentIds.Contains(s.DepartmentId)))
            {
                throw new BadRequestException(GeneralCode.InvalidParams, "Không thể xóa phân công cho tổ đã khai báo chi phí");
            }

            try
            {
                // Xóa phân công
                if (oldProductionAssignments.Count > 0)
                {
                    _manufacturingDBContext.ProductionAssignment.RemoveRange(oldProductionAssignments);
                }
                // Thêm mới phân công
                var newEntities = newAssignments.AsQueryable().ProjectTo<ProductionAssignmentEntity>(_mapper.ConfigurationProvider).ToList();
                _manufacturingDBContext.ProductionAssignment.AddRange(newEntities);
                // Cập nhật phân công
                foreach (var tuple in updateAssignments)
                {
                    tuple.Entity.ProductionStepLinkDataId = (int)tuple.Model.ProductionStepLinkDataId;
                    tuple.Entity.AssignmentQuantity = tuple.Model.AssignmentQuantity;
                }
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

        public async Task<PageData<DepartmentProductionAssignmentModel>> DepartmentProductionAssignment(int departmentId, long? scheduleTurnId, int page, int size, string orderByFieldName, bool asc)
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
                    t.ProductionScheduleStatus,
                    t.ProductionScheduleQuantity
                })
                .Distinct();
            if (scheduleTurnId.HasValue)
            {
                assignmentQuery = assignmentQuery.Where(a => a.ScheduleTurnId == scheduleTurnId);
            }

            var total = await assignmentQuery.CountAsync();
            if (string.IsNullOrWhiteSpace(orderByFieldName))
            {
                orderByFieldName = nameof(DepartmentProductionAssignmentModel.StartDate);
            }

            var pagedData = size > 0 || total > 10000 ? await assignmentQuery.SortByFieldName(orderByFieldName, asc).Skip((page - 1) * size).Take(size).ToListAsync() : await assignmentQuery.ToListAsync();

            return (pagedData.Select(d => new DepartmentProductionAssignmentModel()
            {
                ScheduleTurnId = d.ScheduleTurnId,
                ProductionOrderId = d.ProductionOrderId,
                ProductionOrderCode = d.ProductionOrderCode,
                OrderDetailId = d.OrderDetailId,
                ProductId = d.ProductId,
                StartDate = d.StartDate.GetUnix(),
                EndDate = d.EndDate.GetUnix(),
                ProductionScheduleStatus = (EnumScheduleStatus)d.ProductionScheduleStatus,
                ProductionScheduleQuantity = d.ProductionScheduleQuantity
            }).ToList(), total);
        }

        public async Task<bool> SetProductionStepWorldload(IList<ProductionStepWorkload> productionStepWorldload)
        {
            var productionSteps = await _manufacturingDBContext.ProductionStep
                .Where(y => productionStepWorldload.Select(x => x.ProductionStepId).Contains(y.ProductionStepId))
                .ToListAsync();

            foreach(var productionStep in productionSteps)
            {
                var w = productionStepWorldload.FirstOrDefault(x => x.ProductionStepId == productionStep.ProductionStepId);
                if (w != null)
                    _mapper.Map(w, productionStep);
            }

            await _manufacturingDBContext.SaveChangesAsync();
            return true;
        }
    }
}
