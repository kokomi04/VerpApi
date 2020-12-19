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
using VErp.Services.Manafacturing.Model.ProductionHandover;

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
                throw new BadRequestException(GeneralCode.InvalidParams, "Thông tin lượt kế hoạch hoặc công đoạn sản xuất giữa các tổ không khớp");

            if (step == null) throw new BadRequestException(GeneralCode.InvalidParams, "Công đoạn sản xuất không tồn tại");

            if (data.Any(a => a.Productivity <= 0)) throw new BadRequestException(GeneralCode.InvalidParams, "Năng suất không hợp lệ");

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
                    if (isFinal)
                    {
                        return r.ProductionStepLinkData.Quantity - previousScheduleQuantities.Sum(s => Math.Round(r.ProductionStepLinkData.Quantity * s.ProductionScheduleQuantity / s.ProductionOrderQuantity.Value, 5));
                    }
                    else
                    {
                        return Math.Round(r.ProductionStepLinkData.Quantity * productionSchedules[0].ProductionScheduleQuantity / productionSchedules[0].ProductionOrderQuantity.Value, 5);
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

                if (outSource != null)
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

            // Validate vật tư tiêu hao
            if (_manufacturingDBContext.ProductionConsumMaterial
                .Any(m => m.ScheduleTurnId == scheduleTurnId && m.ProductionStepId == productionStepId && deleteAssignDepartmentIds.Contains(m.DepartmentId)))
            {
                throw new BadRequestException(GeneralCode.InvalidParams, "Không thể xóa phân công cho tổ đã khai báo vật tư tiêu hao");
            }

            // Validate tổ đã thực hiện sản xuất
            var parammeters = new SqlParameter[]
            {
                new SqlParameter("@ScheduleTurnId", scheduleTurnId)
            };
            var resultData = await _manufacturingDBContext.ExecuteDataProcedure("asp_ProductionHandover_GetInventoryRequirementByScheduleTurn", parammeters);

            var inputInventorys = resultData.ConvertData<ProductionInventoryRequirementEntity>()
                .Where(r => r.Status != (int)EnumProductionInventoryRequirementStatus.Rejected)
                .ToList();

            var handovers = _manufacturingDBContext.ProductionHandover
                .Where(h => h.ScheduleTurnId == scheduleTurnId
                && (h.FromProductionStepId == productionStepId || h.ToProductionStepId == productionStepId)
                && h.Status != (int)EnumHandoverStatus.Rejected)
                .ToList();

            // Validate xóa tổ đã tham gia sản xuất
            if (inputInventorys.Any(r => r.ProductionStepId == productionStepId && deleteAssignDepartmentIds.Contains(r.DepartmentId.Value))
                || handovers.Any(h => deleteAssignDepartmentIds.Contains(h.FromDepartmentId) || deleteAssignDepartmentIds.Contains(h.ToDepartmentId)))
            {
                throw new BadRequestException(GeneralCode.InvalidParams, "Không thể xóa phân công cho tổ đã tham gia sản xuất");
            }

            // Validate sửa tổ đã tham gia sản xuất
            //foreach (var tuple in updateAssignments)
            //{
                

            //}

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

        public async Task<IDictionary<int, decimal>> GetCapacityDepartments(long scheduleTurnId, long productionStepId)
        {
            var scheduleTime = (from s in _manufacturingDBContext.ProductionSchedule
                                join od in _manufacturingDBContext.ProductionOrderDetail on s.ProductionOrderDetailId equals od.ProductionOrderDetailId
                                where s.ScheduleTurnId == scheduleTurnId
                                select new
                                {
                                    s.StartDate,
                                    s.EndDate
                                }).FirstOrDefault();

            if (scheduleTime == null)
                throw new BadRequestException(GeneralCode.InvalidParams, "Kế hoạch sản xuất không tồn tại");

            var productionStep = _manufacturingDBContext.ProductionStep.Where(s => s.ProductionStepId == productionStepId).FirstOrDefault();

            if (productionStep == null)
                throw new BadRequestException(GeneralCode.InvalidParams, "Công đoạn sản xuất không tồn tại");

            List<int> departmentIds = (from sd in _manufacturingDBContext.StepDetail
                                       join ps in _manufacturingDBContext.ProductionStep on sd.StepId equals ps.StepId
                                       where ps.ProductionStepId == productionStepId
                                       select sd.DepartmentId).ToList();

            var includeAssignments = _manufacturingDBContext.ProductionAssignment
                .Where(a => a.ProductionStepId == productionStepId && a.ScheduleTurnId == scheduleTurnId && !departmentIds.Contains(a.DepartmentId))
                .Select(a => a.DepartmentId)
                .ToList();
            departmentIds.AddRange(includeAssignments);

            if (departmentIds.Count == 0)
                throw new BadRequestException(GeneralCode.InvalidParams, "Công đoạn chưa thiết lập tổ sản xuất");

            var allScheduleTurns = _manufacturingDBContext.ProductionSchedule
                .Where(s => s.ProductionScheduleStatus != (int)EnumScheduleStatus.Finished && s.StartDate <= scheduleTime.EndDate && s.EndDate >= scheduleTime.StartDate)
                .Join(_manufacturingDBContext.ProductionOrderDetail, s => s.ProductionOrderDetailId, od => od.ProductionOrderDetailId, (s, od) => new
                {
                    s.ScheduleTurnId,
                    s.ProductionScheduleQuantity,
                    ProductionOrderQuantity = od.Quantity.GetValueOrDefault() + od.ReserveQuantity.GetValueOrDefault(),
                    s.StartDate,
                    s.EndDate
                })
                .Select(s => new
                {
                    s.ScheduleTurnId,
                    s.ProductionScheduleQuantity,
                    s.ProductionOrderQuantity,
                    s.StartDate,
                    s.EndDate
                })
                .ToList()
                .GroupBy(s => s.ScheduleTurnId)
                .ToDictionary(g => g.Key, g => g.First());

            var scheduleTurnIds = allScheduleTurns.Select(s => s.Key).ToList();
            var otherAssignments = (from a in _manufacturingDBContext.ProductionAssignment
                                    where scheduleTurnIds.Contains(a.ScheduleTurnId) && departmentIds.Contains(a.DepartmentId) && (a.ProductionStepId != productionStepId || a.ScheduleTurnId != scheduleTurnId)
                                    join d in _manufacturingDBContext.ProductionStepLinkData
                                    on a.ProductionStepLinkDataId equals d.ProductionStepLinkDataId
                                    join r in _manufacturingDBContext.ProductionStepLinkDataRole
                                    on new { a.ProductionStepId, ProductionStepLinkDataRoleTypeId = (int)EnumProductionStepLinkDataRoleType.Output } equals new { r.ProductionStepId, r.ProductionStepLinkDataRoleTypeId }
                                    join td in _manufacturingDBContext.ProductionStepLinkData
                                    on new { r.ProductionStepLinkDataId, d.ObjectTypeId, d.ObjectId } equals new { td.ProductionStepLinkDataId, td.ObjectTypeId, td.ObjectId }
                                    select new
                                    {
                                        a.ProductionStepId,
                                        a.DepartmentId,
                                        a.AssignmentQuantity,
                                        a.ScheduleTurnId,
                                        a.Productivity,
                                        d.ObjectId,
                                        d.ObjectTypeId,
                                        d.Quantity,
                                        TotalQuantity = td.Quantity
                                    }).GroupBy(a => new
                                    {
                                        a.ProductionStepId,
                                        a.DepartmentId,
                                        a.AssignmentQuantity,
                                        a.ScheduleTurnId,
                                        a.Productivity,
                                        a.ObjectId,
                                        a.ObjectTypeId,
                                        a.Quantity,
                                    })
                                    .Select(g => new
                                    {
                                        g.Key.ProductionStepId,
                                        g.Key.DepartmentId,
                                        g.Key.AssignmentQuantity,
                                        g.Key.ScheduleTurnId,
                                        g.Key.Productivity,
                                        g.Key.ObjectId,
                                        g.Key.ObjectTypeId,
                                        g.Key.Quantity,
                                        TotalQuantity = g.Sum(a => a.TotalQuantity)
                                    })
                                    .ToList();

            var productionStepIds = otherAssignments.Select(a => a.ProductionStepId).Distinct().ToList();
            if (!productionStepIds.Contains(productionStepId))
            {
                productionStepIds.Add(productionStepId);
            }

            var workloadMap = _manufacturingDBContext.ProductionStep
                .Where(s => productionStepIds.Contains(s.ProductionStepId))
                .ToDictionary(s => s.ProductionStepId, s => s.Workload.GetValueOrDefault());


            if (workloadMap.Count < productionStepIds.Count) throw new BadRequestException(GeneralCode.InvalidParams, "Công đoạn sản xuất chưa thiết lập khối lượng công việc");

            var handovers = _manufacturingDBContext.ProductionHandover
            .Where(h => scheduleTurnIds.Contains(h.ScheduleTurnId) && departmentIds.Contains(h.FromDepartmentId) && productionStepIds.Contains(h.FromProductionStepId))
            .Where(h => h.Status == (int)EnumHandoverStatus.Accepted)
            .ToList();

            var capacityDepartments = departmentIds.ToDictionary(d => d, d => (decimal)0);

            foreach (var turnId in scheduleTurnIds)
            {
                var scheduleAssignments = otherAssignments.Where(a => a.ScheduleTurnId == turnId).ToList();
                if (scheduleAssignments.Count == 0) continue;
                var parammeters = new SqlParameter[]
                {
                    new SqlParameter("@ScheduleTurnId", turnId)
                };
                var resultData = await _manufacturingDBContext.ExecuteDataProcedure("asp_ProductionHandover_GetInventoryRequirementByScheduleTurn", parammeters);

                var inputInventorys = resultData.ConvertData<ProductionInventoryRequirementEntity>()
                    .Where(r => r.InventoryTypeId == (int)EnumInventoryType.Input && r.Status == (int)EnumProductionInventoryRequirementStatus.Accepted)
                    .ToList();

                var scheduleDays = allScheduleTurns[turnId].EndDate.Subtract(allScheduleTurns[turnId].StartDate).TotalDays + 1;

                foreach (var assignment in scheduleAssignments)
                {
                    var totalAssignQuantity = assignment.AssignmentQuantity * assignment.TotalQuantity / assignment.Quantity;

                    var handoverQuantity = handovers
                        .Where(h => h.FromDepartmentId == assignment.DepartmentId && h.FromProductionStepId == assignment.ProductionStepId && h.ObjectId == assignment.ObjectId && h.ObjectTypeId == h.ObjectTypeId)
                        .Sum(h => h.HandoverQuantity);

                    var inputInventoryQuantity = inputInventorys
                        .Where(h => h.DepartmentId == assignment.DepartmentId && h.ProductionStepId == assignment.ProductionStepId && h.ProductId == assignment.ObjectId)
                        .Sum(h => h.ActualQuantity)
                        .GetValueOrDefault();

                    if (totalAssignQuantity <= handoverQuantity + inputInventoryQuantity) continue;

                    var startMax = scheduleTime.StartDate > allScheduleTurns[assignment.ScheduleTurnId].StartDate ? scheduleTime.StartDate : allScheduleTurns[assignment.ScheduleTurnId].StartDate;
                    var endMin = scheduleTime.EndDate < allScheduleTurns[assignment.ScheduleTurnId].EndDate ? scheduleTime.EndDate : allScheduleTurns[assignment.ScheduleTurnId].EndDate;
                    var matchDays = endMin.Subtract(startMax).TotalDays + 1;

                    var workload = (workloadMap[assignment.ProductionStepId]
                        * allScheduleTurns[assignment.ScheduleTurnId].ProductionScheduleQuantity
                        * Convert.ToDecimal(matchDays / scheduleDays)
                        * (totalAssignQuantity - handoverQuantity - inputInventoryQuantity))
                        / (allScheduleTurns[assignment.ScheduleTurnId].ProductionOrderQuantity
                        * assignment.Productivity
                        * totalAssignQuantity);

                    capacityDepartments[assignment.DepartmentId] += workload;
                }
            }
            return capacityDepartments;
        }

        public async Task<IDictionary<int, Dictionary<long, decimal>>> GetCapacity(long startDate, long endDate)
        {
            DateTime startDateTime = startDate.UnixToDateTime().GetValueOrDefault();
            DateTime endDateTime = endDate.UnixToDateTime().GetValueOrDefault();

            var allScheduleTurns = _manufacturingDBContext.ProductionSchedule
                .Where(s => s.ProductionScheduleStatus != (int)EnumScheduleStatus.Finished && s.StartDate <= endDateTime && s.EndDate >= startDateTime)
                .Join(_manufacturingDBContext.ProductionOrderDetail, s => s.ProductionOrderDetailId, od => od.ProductionOrderDetailId, (s, od) => new
                {
                    s.ScheduleTurnId,
                    s.ProductionScheduleQuantity,
                    ProductionOrderQuantity = od.Quantity.GetValueOrDefault() + od.ReserveQuantity.GetValueOrDefault(),
                    s.StartDate,
                    s.EndDate
                })
                .Select(s => new
                {
                    s.ScheduleTurnId,
                    s.ProductionScheduleQuantity,
                    s.ProductionOrderQuantity,
                    s.StartDate,
                    s.EndDate
                })
                .ToList()
                .GroupBy(s => s.ScheduleTurnId)
                .ToDictionary(g => g.Key, g => g.First());

            var scheduleTurnIds = allScheduleTurns.Select(s => s.Key).ToList();
            var allAssignments = (from a in _manufacturingDBContext.ProductionAssignment
                                  where scheduleTurnIds.Contains(a.ScheduleTurnId)
                                  join d in _manufacturingDBContext.ProductionStepLinkData
                                  on a.ProductionStepLinkDataId equals d.ProductionStepLinkDataId
                                  join r in _manufacturingDBContext.ProductionStepLinkDataRole
                                  on new { a.ProductionStepId, ProductionStepLinkDataRoleTypeId = (int)EnumProductionStepLinkDataRoleType.Output } equals new { r.ProductionStepId, r.ProductionStepLinkDataRoleTypeId }
                                  join td in _manufacturingDBContext.ProductionStepLinkData
                                  on new { r.ProductionStepLinkDataId, d.ObjectTypeId, d.ObjectId } equals new { td.ProductionStepLinkDataId, td.ObjectTypeId, td.ObjectId }
                                  select new
                                  {
                                      a.ProductionStepId,
                                      a.DepartmentId,
                                      a.AssignmentQuantity,
                                      a.ScheduleTurnId,
                                      a.Productivity,
                                      d.ObjectId,
                                      d.ObjectTypeId,
                                      d.Quantity,
                                      TotalQuantity = td.Quantity
                                  }).GroupBy(a => new
                                  {
                                      a.ProductionStepId,
                                      a.DepartmentId,
                                      a.AssignmentQuantity,
                                      a.ScheduleTurnId,
                                      a.Productivity,
                                      a.ObjectId,
                                      a.ObjectTypeId,
                                      a.Quantity,
                                  })
                                    .Select(g => new
                                    {
                                        g.Key.ProductionStepId,
                                        g.Key.DepartmentId,
                                        g.Key.AssignmentQuantity,
                                        g.Key.ScheduleTurnId,
                                        g.Key.Productivity,
                                        g.Key.ObjectId,
                                        g.Key.ObjectTypeId,
                                        g.Key.Quantity,
                                        TotalQuantity = g.Sum(a => a.TotalQuantity)
                                    })
                                    .ToList();

            var productionStepIds = allAssignments.Select(a => a.ProductionStepId).Distinct().ToList();
            var departmentIds = allAssignments.Select(a => a.DepartmentId).Distinct().ToList();

            var workloadMap = _manufacturingDBContext.ProductionStep
                .Where(s => productionStepIds.Contains(s.ProductionStepId))
                .ToDictionary(s => s.ProductionStepId, s => s.Workload.GetValueOrDefault());

            if (workloadMap.Count < productionStepIds.Count) throw new BadRequestException(GeneralCode.InvalidParams, "Công đoạn sản xuất chưa thiết lập khối lượng công việc");

            var handovers = _manufacturingDBContext.ProductionHandover
                .Where(h => scheduleTurnIds.Contains(h.ScheduleTurnId) && departmentIds.Contains(h.FromDepartmentId) && productionStepIds.Contains(h.FromProductionStepId))
                .Where(h => h.Status == (int)EnumHandoverStatus.Accepted)
                .ToList();

            var capacityDepartments = departmentIds.ToDictionary(d => d, d => scheduleTurnIds.ToDictionary(s => s, s => (decimal)0));

            foreach (var scheduleTurnId in scheduleTurnIds)
            {
                var scheduleAssignments = allAssignments.Where(a => a.ScheduleTurnId == scheduleTurnId).ToList();
                if (scheduleAssignments.Count == 0) continue;
                var parammeters = new SqlParameter[]
                {
                    new SqlParameter("@ScheduleTurnId", scheduleTurnId)
                };
                var resultData = await _manufacturingDBContext.ExecuteDataProcedure("asp_ProductionHandover_GetInventoryRequirementByScheduleTurn", parammeters);

                var inputInventorys = resultData.ConvertData<ProductionInventoryRequirementEntity>()
                    .Where(r => r.InventoryTypeId == (int)EnumInventoryType.Input && r.Status == (int)EnumProductionInventoryRequirementStatus.Accepted)
                    .ToList();

                var scheduleDays = allScheduleTurns[scheduleTurnId].EndDate.Subtract(allScheduleTurns[scheduleTurnId].StartDate).TotalDays + 1;

                foreach (var assignment in scheduleAssignments)
                {
                    var totalAssignQuantity = assignment.AssignmentQuantity * assignment.TotalQuantity / assignment.Quantity;

                    var handoverQuantity = handovers
                        .Where(h => h.FromDepartmentId == assignment.DepartmentId && h.FromProductionStepId == assignment.ProductionStepId && h.ObjectId == assignment.ObjectId && h.ObjectTypeId == h.ObjectTypeId)
                        .Sum(h => h.HandoverQuantity);

                    var inputInventoryQuantity = inputInventorys
                        .Where(h => h.DepartmentId == assignment.DepartmentId && h.ProductionStepId == assignment.ProductionStepId && h.ProductId == assignment.ObjectId)
                        .Sum(h => h.ActualQuantity)
                        .GetValueOrDefault();

                    if (totalAssignQuantity <= handoverQuantity + inputInventoryQuantity) continue;

                    var startMax = startDateTime > allScheduleTurns[assignment.ScheduleTurnId].StartDate ? startDateTime : allScheduleTurns[assignment.ScheduleTurnId].StartDate;
                    var endMin = endDateTime < allScheduleTurns[assignment.ScheduleTurnId].EndDate ? endDateTime : allScheduleTurns[assignment.ScheduleTurnId].EndDate;
                    var matchDays = endMin.Subtract(startMax).TotalDays + 1;

                    var workload = (workloadMap[assignment.ProductionStepId]
                        * allScheduleTurns[assignment.ScheduleTurnId].ProductionScheduleQuantity
                        * Convert.ToDecimal(matchDays / scheduleDays)
                        * (totalAssignQuantity - handoverQuantity - inputInventoryQuantity))
                        / (allScheduleTurns[assignment.ScheduleTurnId].ProductionOrderQuantity
                        * assignment.Productivity
                        * totalAssignQuantity);

                    capacityDepartments[assignment.DepartmentId][scheduleTurnId] += workload;
                }
            }
            return capacityDepartments;
        }

        public async Task<IDictionary<int, decimal>> GetProductivityDepartments(long productionStepId)
        {
            return await (from sd in _manufacturingDBContext.StepDetail
                          join ps in _manufacturingDBContext.ProductionStep on sd.StepId equals ps.StepId
                          where ps.ProductionStepId == productionStepId
                          select new
                          {
                              sd.DepartmentId,
                              sd.Quantity
                          }).ToDictionaryAsync(sd => sd.DepartmentId, sd => sd.Quantity);
        }
    }
}
