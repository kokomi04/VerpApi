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
using VErp.Commons.Constants;
using VErp.Services.Manafacturing.Model.ProductionOrder.Materials;

namespace VErp.Services.Manafacturing.Service.ProductionAssignment.Implement
{
    public class ProductionAssignmentService : IProductionAssignmentService
    {
        private readonly ManufacturingDBContext _manufacturingDBContext;
        private readonly IActivityLogService _activityLogService;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly ICustomGenCodeHelperService _customGenCodeHelperService;
        private readonly IOrganizationHelperService _organizationHelperService;
        public ProductionAssignmentService(ManufacturingDBContext manufacturingDB
            , IActivityLogService activityLogService
            , ILogger<ProductionAssignmentService> logger
            , IMapper mapper
            , ICustomGenCodeHelperService customGenCodeHelperService
            , IOrganizationHelperService organizationHelperService)
        {
            _manufacturingDBContext = manufacturingDB;
            _activityLogService = activityLogService;
            _logger = logger;
            _mapper = mapper;
            _customGenCodeHelperService = customGenCodeHelperService;
            _organizationHelperService = organizationHelperService;
        }


        public async Task<IList<ProductionAssignmentModel>> GetProductionAssignments(long productionOrderId)
        {
            return await _manufacturingDBContext.ProductionAssignment
                .Include(a => a.ProductionAssignmentDetail)
                .Where(a => a.ProductionOrderId == productionOrderId)
                .ProjectTo<ProductionAssignmentModel>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        public async Task<ProductionAssignmentModel> GetProductionAssignment(long productionOrderId, long productionStepId, int departmentId)
        {
            var assignment = await _manufacturingDBContext.ProductionAssignment
                .Include(a => a.ProductionAssignmentDetail)
                .Where(a => a.ProductionOrderId == productionOrderId
                && a.ProductionStepId == productionStepId
                && a.DepartmentId == departmentId)
                .FirstOrDefaultAsync();
            return _mapper.Map<ProductionAssignmentModel>(assignment);
        }

        public async Task<bool> UpdateProductionAssignment(long productionOrderId, GeneralAssignmentModel data)
        {
            // 
            var productionOrder = _manufacturingDBContext.ProductionOrder.FirstOrDefault(po => po.ProductionOrderId == productionOrderId);
            if (productionOrder == null) throw new BadRequestException(GeneralCode.InvalidParams, "Lệnh sản xuất không tồn tại");

            // Validate
            var steps = _manufacturingDBContext.ProductionStep
                .Include(s => s.ProductionStepLinkDataRole)
                .ThenInclude(r => r.ProductionStepLinkData)
                .Where(s => s.ContainerTypeId == (int)EnumContainerType.ProductionOrder && s.ContainerId == productionOrderId)
                .ToList();

            var productionOderDetails = (
                from po in _manufacturingDBContext.ProductionOrder
                join pod in _manufacturingDBContext.ProductionOrderDetail on po.ProductionOrderId equals pod.ProductionOrderId
                where po.ProductionOrderId == productionOrderId
                select new
                {
                    pod.ProductionOrderDetailId,
                    pod.ProductId,
                    ProductionOrderQuantity = pod.Quantity + pod.ReserveQuantity,
                    po.StartDate,
                    po.EndDate
                }).ToList();

            if (productionOderDetails.Count == 0) throw new BadRequestException(GeneralCode.InvalidParams, "Lệnh sản xuất không tồn tại");

            var productionOrderDetailIds = productionOderDetails.Select(s => s.ProductionOrderDetailId).ToList();

            var oldProductionAssignments = _manufacturingDBContext.ProductionAssignment
                   .Include(a => a.ProductionAssignmentDetail)
                   .Where(s => s.ProductionOrderId == productionOrderId)
                   .ToList();

            var scheduleTurnShifts = _manufacturingDBContext.ProductionScheduleTurnShift
                    .Where(s => s.ProductionOrderId == productionOrderId)
                    .ToList();

            // Validate tổ đã thực hiện sản xuất
            var parammeters = new SqlParameter[]
            {
                new SqlParameter("@ProductionOrderId", productionOrderId)
            };
            var resultData = await _manufacturingDBContext.ExecuteDataProcedure("asp_ProductionHandover_GetInventoryRequirementByProductionOrder", parammeters);

            var inputInventorys = resultData.ConvertData<ProductionInventoryRequirementEntity>()
                .Where(r => r.Status != (int)EnumProductionInventoryRequirementStatus.Rejected)
                .ToList();

            var handovers = _manufacturingDBContext.ProductionHandover
                .Where(h => h.ProductionOrderId == productionOrderId && h.Status != (int)EnumHandoverStatus.Rejected)
                .ToList();

            var mapData = new Dictionary<long,
                (List<ProductionAssignmentEntity> DeleteProductionStepAssignments,
                List<(ProductionAssignmentEntity Entity, ProductionAssignmentModel Model)> UpdateProductionStepAssignments,
                List<ProductionAssignmentModel> CreateProductionStepAssignments)>();

            foreach (var productionStepAssignments in data.ProductionStepAssignment)
            {
                var step = steps.FirstOrDefault(s => s.ProductionStepId == productionStepAssignments.ProductionStepId);
                if (productionStepAssignments.ProductionAssignments.Any(a => a.ProductionOrderId != productionOrderId || a.ProductionStepId != productionStepAssignments.ProductionStepId))
                    throw new BadRequestException(GeneralCode.InvalidParams, "Thông tin kế công đoạn sản xuất giữa các tổ không khớp");
                if (step == null) throw new BadRequestException(GeneralCode.InvalidParams, "Công đoạn sản xuất không tồn tại");
                if (productionStepAssignments.ProductionAssignments.Any(a => a.Productivity <= 0)) throw new BadRequestException(GeneralCode.InvalidParams, "Năng suất không hợp lệ");

                var linkDatas = step.ProductionStepLinkDataRole
                .Where(r => r.ProductionStepLinkDataRoleTypeId == (int)EnumProductionStepLinkDataRoleType.Output)
                .ToDictionary(r => r.ProductionStepLinkDataId,
                r =>
                {
                    return Math.Round(r.ProductionStepLinkData.QuantityOrigin - r.ProductionStepLinkData.OutsourcePartQuantity.GetValueOrDefault(), 5);
                });

                if (productionStepAssignments.ProductionAssignments.Any(d => d.AssignmentQuantity <= 0))
                    throw new BadRequestException(GeneralCode.InvalidParams, "Số lượng phân công phải lớn hơn 0");

                // Lấy thông tin outsource
                var outSource = step.ProductionStepLinkDataRole
                    .Where(r => r.ProductionStepLinkDataRoleTypeId == (int)EnumProductionStepLinkDataRoleType.Output && r.ProductionStepLinkData.OutsourceQuantity.HasValue)
                    .FirstOrDefault();

                foreach (var linkData in linkDatas)
                {
                    decimal totalAssignmentQuantity = 0;

                    if (outSource != null && (outSource.ProductionStepLinkData.QuantityOrigin - outSource.ProductionStepLinkData.OutsourcePartQuantity.GetValueOrDefault()) > 0)
                    {
                        totalAssignmentQuantity += linkData.Value * outSource.ProductionStepLinkData.OutsourceQuantity.GetValueOrDefault() / (outSource.ProductionStepLinkData.QuantityOrigin - outSource.ProductionStepLinkData.OutsourcePartQuantity.GetValueOrDefault());
                    }

                    foreach (var assignment in productionStepAssignments.ProductionAssignments)
                    {
                        var sourceData = linkDatas[assignment.ProductionStepLinkDataId];
                        totalAssignmentQuantity += assignment.AssignmentQuantity * linkData.Value / sourceData;
                    }

                    if (totalAssignmentQuantity.SubProductionDecimal(linkData.Value) > 0)
                        throw new BadRequestException(GeneralCode.InvalidParams, "Số lượng phân công lớn hơn số lượng trong kế hoạch sản xuất");
                }

                var oldProductionStepAssignments = oldProductionAssignments
                    .Where(s => s.ProductionStepId == productionStepAssignments.ProductionStepId)
                    .ToList();

                var updateAssignments = new List<(ProductionAssignmentEntity Entity, ProductionAssignmentModel Model)>();
                var newAssignments = new List<ProductionAssignmentModel>();
                foreach (var item in productionStepAssignments.ProductionAssignments)
                {
                    var entity = oldProductionStepAssignments.FirstOrDefault(a => a.DepartmentId == item.DepartmentId);
                    if (entity == null)
                    {
                        newAssignments.Add(item);
                    }
                    else
                    {
                        if (item.IsChange(entity))
                        {
                            updateAssignments.Add((entity, item));
                        }
                        oldProductionStepAssignments.Remove(entity);
                    }
                }

                // Validate khai báo chi phí
                var deleteAssignDepartmentIds = oldProductionStepAssignments.Select(a => a.DepartmentId).ToList();
                if (_manufacturingDBContext.ProductionScheduleTurnShift
                    .Any(s => s.ProductionOrderId == productionOrderId && s.ProductionStepId == productionStepAssignments.ProductionStepId && deleteAssignDepartmentIds.Contains(s.DepartmentId)))
                {
                    throw new BadRequestException(GeneralCode.InvalidParams, "Không thể xóa phân công cho tổ đã khai báo chi phí");
                }

                // Validate vật tư tiêu hao
                if (scheduleTurnShifts.Any(m => m.ProductionStepId == productionStepAssignments.ProductionStepId && deleteAssignDepartmentIds.Contains(m.DepartmentId)))
                {
                    throw new BadRequestException(GeneralCode.InvalidParams, "Không thể xóa phân công cho tổ đã khai báo vật tư tiêu hao");
                }

                // Validate xóa tổ đã tham gia sản xuất
                var productIds = step.ProductionStepLinkDataRole
                    .Where(r => r.ProductionStepLinkDataRoleTypeId == (int)EnumProductionStepLinkDataRoleType.Output && r.ProductionStepLinkData.ObjectTypeId == (int)EnumProductionStepLinkDataObjectType.Product)
                    .Select(r => r.ProductionStepLinkData.ObjectId)
                    .ToList();
                if (inputInventorys.Any(r => productIds.Contains(r.ProductId) && deleteAssignDepartmentIds.Contains(r.DepartmentId.Value))
                    || handovers.Any(h => deleteAssignDepartmentIds.Contains(h.FromDepartmentId) || deleteAssignDepartmentIds.Contains(h.ToDepartmentId)))
                {
                    throw new BadRequestException(GeneralCode.InvalidParams, "Không thể xóa phân công cho tổ đã tham gia sản xuất");
                }

                mapData.Add(productionStepAssignments.ProductionStepId, (oldProductionStepAssignments, updateAssignments, newAssignments));
            }

            try
            {
                // Thêm thông tin thời gian biểu làm việc
                var startDate = productionOderDetails[0].StartDate;
                var endDate = productionOderDetails[0].EndDate;

                //// Xử lý thông tin làm việc của tổ theo từng ngày
                //var departmentIds = data.DepartmentTimeTable.Select(d => d.DepartmentId).ToList();

                //var oldTimeTable = _manufacturingDBContext.DepartmentTimeTable
                //    .Where(t => departmentIds.Contains(t.DepartmentId) && t.WorkDate >= startDate && t.WorkDate <= endDate).ToList();

                //_manufacturingDBContext.DepartmentTimeTable.RemoveRange(oldTimeTable);

                //foreach (var item in data.DepartmentTimeTable)
                //{
                //    var entity = _mapper.Map<DepartmentTimeTable>(item);
                //    _manufacturingDBContext.DepartmentTimeTable.Add(entity);
                //}

                var productionStepIds = data.ProductionStepAssignment.Select(a => a.ProductionStepId).ToList();
                var productionStepWorkInfos = _manufacturingDBContext.ProductionStepWorkInfo.Where(w => productionStepIds.Contains(w.ProductionStepId)).ToList();

                foreach (var productionStepAssignments in data.ProductionStepAssignment)
                {
                    // Thêm thông tin công việc
                    var productionStepWorkInfo = productionStepWorkInfos.FirstOrDefault(w => w.ProductionStepId == productionStepAssignments.ProductionStepId);
                    if (productionStepWorkInfo == null)
                    {
                        productionStepWorkInfo = _mapper.Map<ProductionStepWorkInfo>(productionStepAssignments.ProductionStepWorkInfo);
                        productionStepWorkInfo.ProductionStepId = productionStepAssignments.ProductionStepId;
                        _manufacturingDBContext.ProductionStepWorkInfo.Add(productionStepWorkInfo);
                    }
                    else
                    {
                        _mapper.Map(productionStepAssignments.ProductionStepWorkInfo, productionStepWorkInfo);
                    }

                    // Xóa phân công
                    if (mapData[productionStepAssignments.ProductionStepId].DeleteProductionStepAssignments.Count > 0)
                    {
                        foreach (var oldProductionAssignment in mapData[productionStepAssignments.ProductionStepId].DeleteProductionStepAssignments)
                        {
                            oldProductionAssignment.ProductionAssignmentDetail.Clear();
                        }
                        _manufacturingDBContext.SaveChanges();
                        _manufacturingDBContext.ProductionAssignment.RemoveRange(mapData[productionStepAssignments.ProductionStepId].DeleteProductionStepAssignments);
                    }

                    // Thêm mới phân công
                    if (mapData[productionStepAssignments.ProductionStepId].CreateProductionStepAssignments.Count > 0)
                    {
                        var newEntities = mapData[productionStepAssignments.ProductionStepId].CreateProductionStepAssignments.AsQueryable()
                           .ProjectTo<ProductionAssignmentEntity>(_mapper.ConfigurationProvider)
                           .ToList();
                        foreach (var newEntitie in newEntities)
                        {
                            newEntitie.AssignedProgressStatus = (int)EnumAssignedProgressStatus.Waiting;
                        }
                        _manufacturingDBContext.ProductionAssignment.AddRange(newEntities);
                    }

                    // Cập nhật phân công
                    foreach (var tuple in mapData[productionStepAssignments.ProductionStepId].UpdateProductionStepAssignments)
                    {
                        tuple.Entity.ProductionAssignmentDetail.Clear();
                        _mapper.Map(tuple.Model, tuple.Entity);
                    }
                }

                // Update reset process status
                productionOrder.IsResetProductionProcess = true;

                _manufacturingDBContext.SaveChanges();

                await _activityLogService.CreateLog(EnumObjectType.ProductionAssignment, productionOrderId, $"Cập nhật phân công sản xuất cho lệnh sản xuất {productionOrderId}", data.JsonSerialize());

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateProductAssignment");
                throw;
            }
        }

        public async Task<bool> UpdateProductionAssignment(long productionOrderId, long productionStepId, ProductionAssignmentModel[] data, ProductionStepWorkInfoInputModel info)
        {
            var productionOrder = _manufacturingDBContext.ProductionOrder.FirstOrDefault(po => po.ProductionOrderId == productionOrderId);
            if (productionOrder == null) throw new BadRequestException(GeneralCode.InvalidParams, "Lệnh sản xuất không tồn tại");

            // Validate
            var step = _manufacturingDBContext.ProductionStep
                .Include(s => s.ProductionStepLinkDataRole)
                .ThenInclude(r => r.ProductionStepLinkData)
                .Where(s => s.ProductionStepId == productionStepId)
                .FirstOrDefault();

            if (data.Any(a => a.ProductionOrderId != productionOrderId || a.ProductionStepId != productionStepId))
                throw new BadRequestException(GeneralCode.InvalidParams, "Thông tin kế hoạch hoặc công đoạn sản xuất giữa các tổ không khớp");

            if (step == null) throw new BadRequestException(GeneralCode.InvalidParams, "Công đoạn sản xuất không tồn tại");

            if (data.Any(a => a.Productivity <= 0)) throw new BadRequestException(GeneralCode.InvalidParams, "Năng suất không hợp lệ");

            var productionOderDetails = (
                from po in _manufacturingDBContext.ProductionOrder
                join pod in _manufacturingDBContext.ProductionOrderDetail on po.ProductionOrderId equals pod.ProductionOrderId
                where po.ProductionOrderId == productionOrderId
                select new
                {
                    pod.ProductionOrderDetailId,
                    pod.ProductId,
                    ProductionOrderQuantity = pod.Quantity + pod.ReserveQuantity,
                    po.StartDate,
                    po.EndDate
                }).ToList();

            if (productionOderDetails.Count == 0) throw new BadRequestException(GeneralCode.InvalidParams, "Lệnh sản xuất không tồn tại");

            var productionOrderDetailIds = productionOderDetails.Select(s => s.ProductionOrderDetailId).ToList();

            //if (!_manufacturingDBContext.ProductionStepOrder
            //    .Any(so => productionOrderDetailIds.Contains(so.ProductionOrderDetailId) && so.ProductionStepId == productionStepId))
            //{
            //    throw new BadRequestException(GeneralCode.InvalidParams, "Công đoạn sản xuất không tồn tại trong quy trình sản xuất");
            //}

            var linkDatas = step.ProductionStepLinkDataRole
                .Where(r => r.ProductionStepLinkDataRoleTypeId == (int)EnumProductionStepLinkDataRoleType.Output)
                .ToDictionary(r => r.ProductionStepLinkDataId,
                r =>
                {
                    return Math.Round(r.ProductionStepLinkData.QuantityOrigin - r.ProductionStepLinkData.OutsourcePartQuantity.GetValueOrDefault(), 5);
                });

            if (data.Any(d => d.AssignmentQuantity <= 0))
                throw new BadRequestException(GeneralCode.InvalidParams, "Số lượng phân công phải lớn hơn 0");

            // Lấy thông tin outsource
            var outSource = step.ProductionStepLinkDataRole
                .Where(r => r.ProductionStepLinkDataRoleTypeId == (int)EnumProductionStepLinkDataRoleType.Output && r.ProductionStepLinkData.OutsourceQuantity.HasValue)
                .FirstOrDefault();

            foreach (var linkData in linkDatas)
            {
                decimal totalAssignmentQuantity = 0;

                if (outSource != null)
                {
                    totalAssignmentQuantity += linkData.Value * outSource.ProductionStepLinkData.OutsourceQuantity.GetValueOrDefault()
                        / (outSource.ProductionStepLinkData.QuantityOrigin - outSource.ProductionStepLinkData.OutsourcePartQuantity.GetValueOrDefault());
                }

                foreach (var assignment in data)
                {
                    var sourceData = linkDatas[assignment.ProductionStepLinkDataId];
                    totalAssignmentQuantity += assignment.AssignmentQuantity * linkData.Value / sourceData;
                }

                if (totalAssignmentQuantity.SubProductionDecimal(linkData.Value) > 0)
                    throw new BadRequestException(GeneralCode.InvalidParams, "Số lượng phân công lớn hơn số lượng trong kế hoạch sản xuất");
            }

            var oldProductionAssignments = _manufacturingDBContext.ProductionAssignment
                .Include(a => a.ProductionAssignmentDetail)
                .Where(s => s.ProductionOrderId == productionOrderId && s.ProductionStepId == productionStepId)
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
                    if (item.IsChange(entity))
                    {
                        updateAssignments.Add((entity, item));
                    }
                    oldProductionAssignments.Remove(entity);
                }
            }

            // Validate khai báo chi phí
            var deleteAssignDepartmentIds = oldProductionAssignments.Select(a => a.DepartmentId).ToList();
            if (_manufacturingDBContext.ProductionScheduleTurnShift
                .Any(s => s.ProductionOrderId == productionOrderId && s.ProductionStepId == productionStepId && deleteAssignDepartmentIds.Contains(s.DepartmentId)))
            {
                throw new BadRequestException(GeneralCode.InvalidParams, "Không thể xóa phân công cho tổ đã khai báo chi phí");
            }

            // Validate vật tư tiêu hao
            if (_manufacturingDBContext.ProductionConsumMaterial
                .Any(m => m.ProductionOrderId == productionOrderId && m.ProductionStepId == productionStepId && deleteAssignDepartmentIds.Contains(m.DepartmentId)))
            {
                throw new BadRequestException(GeneralCode.InvalidParams, "Không thể xóa phân công cho tổ đã khai báo vật tư tiêu hao");
            }

            // Validate tổ đã thực hiện sản xuất
            var parammeters = new SqlParameter[]
            {
                new SqlParameter("@ProductionOrderId", productionOrderId)
            };
            var resultData = await _manufacturingDBContext.ExecuteDataProcedure("asp_ProductionHandover_GetInventoryRequirementByProductionOrder", parammeters);

            var inputInventorys = resultData.ConvertData<ProductionInventoryRequirementEntity>()
                .Where(r => r.Status != (int)EnumProductionInventoryRequirementStatus.Rejected)
                .ToList();

            var handovers = _manufacturingDBContext.ProductionHandover
                .Where(h => h.ProductionOrderId == productionOrderId
                && (h.FromProductionStepId == productionStepId || h.ToProductionStepId == productionStepId)
                && h.Status != (int)EnumHandoverStatus.Rejected)
                .ToList();

            // Validate xóa tổ đã tham gia sản xuất
            var productIds = step.ProductionStepLinkDataRole
                    .Where(r => r.ProductionStepLinkDataRoleTypeId == (int)EnumProductionStepLinkDataRoleType.Output && r.ProductionStepLinkData.ObjectTypeId == (int)EnumProductionStepLinkDataObjectType.Product)
                    .Select(r => r.ProductionStepLinkData.ObjectId)
                    .ToList();
            if (inputInventorys.Any(r => productIds.Contains(r.ProductId) && deleteAssignDepartmentIds.Contains(r.DepartmentId.Value))
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
                // Thêm thông tin thời gian biểu làm việc
                var startDate = productionOderDetails[0].StartDate;
                var endDate = productionOderDetails[0].EndDate;

                //var startDateUnix = startDate.GetUnix();
                //var endDateUnix = endDate.GetUnix();

                var departmentIds = data.Select(a => a.DepartmentId).ToList();

                //var oldTimeTable = _manufacturingDBContext.DepartmentTimeTable.Where(t => departmentIds.Contains(t.DepartmentId) && t.WorkDate >= startDate && t.WorkDate <= endDate).ToList();
                //_manufacturingDBContext.DepartmentTimeTable.RemoveRange(oldTimeTable);

                //foreach (var item in timeTable)
                //{
                //    var entity = _mapper.Map<DepartmentTimeTable>(item);
                //    _manufacturingDBContext.DepartmentTimeTable.Add(entity);
                //}

                // Thêm thông tin công việc
                var productionStepWorkInfo = _manufacturingDBContext.ProductionStepWorkInfo
                    .FirstOrDefault(w => w.ProductionStepId == productionStepId);
                if (productionStepWorkInfo == null)
                {
                    productionStepWorkInfo = _mapper.Map<ProductionStepWorkInfo>(info);
                    productionStepWorkInfo.ProductionStepId = productionStepId;
                    _manufacturingDBContext.ProductionStepWorkInfo.Add(productionStepWorkInfo);
                }
                else
                {
                    _mapper.Map(info, productionStepWorkInfo);
                }

                // Xóa phân công
                if (oldProductionAssignments.Count > 0)
                {
                    foreach (var oldProductionAssignment in oldProductionAssignments)
                    {
                        oldProductionAssignment.ProductionAssignmentDetail.Clear();
                    }
                    _manufacturingDBContext.SaveChanges();
                    _manufacturingDBContext.ProductionAssignment.RemoveRange(oldProductionAssignments);
                }
                // Thêm mới phân công
                var newEntities = newAssignments.AsQueryable()
                    .ProjectTo<ProductionAssignmentEntity>(_mapper.ConfigurationProvider)
                    .ToList();
                foreach (var newEntitie in newEntities)
                {
                    newEntitie.AssignedProgressStatus = (int)EnumAssignedProgressStatus.Waiting;
                }
                _manufacturingDBContext.ProductionAssignment.AddRange(newEntities);
                // Cập nhật phân công
                foreach (var tuple in updateAssignments)
                {
                    tuple.Entity.ProductionAssignmentDetail.Clear();
                    _mapper.Map(tuple.Model, tuple.Entity);
                }

                // Update reset process status
                productionOrder.IsResetProductionProcess = true;

                _manufacturingDBContext.SaveChanges();

                await _activityLogService.CreateLog(EnumObjectType.ProductionAssignment, productionStepId, $"Cập nhật phân công sản xuất cho lệnh sản xuất {productionOrderId}", data.JsonSerialize());

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateProductAssignment");
                throw;
            }
        }

        public async Task<PageData<DepartmentProductionAssignmentModel>> DepartmentProductionAssignment(int departmentId, long? productionOrderId, int page, int size, string orderByFieldName, bool asc, long? fromDate, long? toDate)
        {
            var fDate = fromDate.UnixToDateTime();
            var tDate = toDate.UnixToDateTime();

            var assignmentQuery = (
                from a in _manufacturingDBContext.ProductionAssignment
                join s in _manufacturingDBContext.ProductionStep.Where(s => s.ContainerTypeId == (int)EnumContainerType.ProductionOrder) on a.ProductionStepId equals s.ProductionStepId
                join o in _manufacturingDBContext.ProductionOrder on a.ProductionOrderId equals o.ProductionOrderId
                join od in _manufacturingDBContext.ProductionOrderDetail on o.ProductionOrderId equals od.ProductionOrderId
                where a.DepartmentId == departmentId && (fDate != null && tDate != null ? o.Date >= fDate && o.Date <= tDate : true)
                select new
                {
                    o.ProductionOrderId,
                    o.ProductionOrderCode,
                    od.OrderDetailId,
                    od.ProductId,
                    o.StartDate,
                    o.EndDate,
                    TotalQuantity = od.Quantity + od.ReserveQuantity,
                    o.ProductionOrderStatus
                })
                .Distinct();
            if (productionOrderId.HasValue)
            {
                assignmentQuery = assignmentQuery.Where(a => a.ProductionOrderId == productionOrderId);
            }

            var total = await assignmentQuery.CountAsync();
            if (string.IsNullOrWhiteSpace(orderByFieldName))
            {
                orderByFieldName = nameof(DepartmentProductionAssignmentModel.StartDate);
            }

            var pagedData = size > 0 || total > 10000 ? await assignmentQuery.SortByFieldName(orderByFieldName, asc).Skip((page - 1) * size).Take(size).ToListAsync() : await assignmentQuery.ToListAsync();

            return (pagedData.Select(d => new DepartmentProductionAssignmentModel()
            {
                ProductionOrderId = d.ProductionOrderId,
                ProductionOrderCode = d.ProductionOrderCode,
                OrderDetailId = d.OrderDetailId,
                ProductId = d.ProductId,
                StartDate = d.StartDate.GetUnix(),
                EndDate = d.EndDate.GetUnix(),
                ProductQuantity = d.TotalQuantity.Value,
                ProductionOrderStatus = (EnumProductionStatus)d.ProductionOrderStatus
            }).ToList(), total);
        }

        public async Task<CapacityOutputModel> GetGeneralCapacityDepartments(long productionOrderId)
        {
            var productionTime = await (
                from o in _manufacturingDBContext.ProductionOrder
                where o.ProductionOrderId == productionOrderId
                select new
                {
                    o.StartDate,
                    o.EndDate
                }).FirstOrDefaultAsync();

            if (productionTime == null)
                throw new BadRequestException(GeneralCode.InvalidParams, "Kế hoạch sản xuất không tồn tại");

            var productionSteps = _manufacturingDBContext.ProductionStep
                .Where(ps => ps.ContainerTypeId == (int)EnumContainerType.ProductionOrder && ps.ContainerId == productionOrderId && ps.StepId.HasValue).ToList();

            if (productionSteps.Count == 0)
                throw new BadRequestException(GeneralCode.InvalidParams, "Công đoạn sản xuất không tồn tại");

            var departmentIdMap = (from sd in _manufacturingDBContext.StepDetail
                                   join ps in _manufacturingDBContext.ProductionStep on sd.StepId equals ps.StepId
                                   where ps.ContainerTypeId == (int)EnumContainerType.ProductionOrder && ps.ContainerId == productionOrderId
                                   select new
                                   {
                                       ps.ProductionStepId,
                                       sd.DepartmentId
                                   })
                                   .ToList()
                                   .GroupBy(sd => sd.ProductionStepId)
                                   .ToDictionary(sd => sd.Key, sd => sd.Select(sd => sd.DepartmentId)
                                   .ToList());

            var productionStepIds = productionSteps.Select(ps => ps.ProductionStepId).ToList();
            var departmentIds = departmentIdMap.SelectMany(sd => sd.Value).ToList();

            var includeAssignments = _manufacturingDBContext.ProductionAssignment
                .Where(a => productionStepIds.Contains(a.ProductionStepId)
                    && a.ProductionOrderId == productionOrderId
                    && !departmentIds.Contains(a.DepartmentId)
                )
                .Select(a => new
                {
                    a.ProductionStepId,
                    a.DepartmentId
                })
                .ToList()
                .GroupBy(sd => sd.ProductionStepId)
                .ToDictionary(sd => sd.Key, sd => sd.Select(sd => sd.DepartmentId).ToList());

            departmentIds.AddRange(includeAssignments.SelectMany(sd => sd.Value).ToList());
            foreach (var includeAssignment in includeAssignments)
            {
                departmentIdMap[includeAssignment.Key].AddRange(includeAssignment.Value);
            }

            if (departmentIdMap.Any(sd => sd.Value.Count == 0))
                throw new BadRequestException(GeneralCode.InvalidParams, "Tồn tại công đoạn chưa thiết lập tổ sản xuất");

            var capacityDepartments = departmentIds.Distinct().ToDictionary(d => d, d => new List<CapacityModel>());

            var otherAssignments = (
                from a in _manufacturingDBContext.ProductionAssignment
                where departmentIds.Contains(a.DepartmentId)
                    && a.ProductionOrderId != productionOrderId
                    && a.StartDate <= productionTime.EndDate
                    && a.EndDate >= productionTime.StartDate
                join ps in _manufacturingDBContext.ProductionStep on a.ProductionStepId equals ps.ProductionStepId
                join s in _manufacturingDBContext.Step on ps.StepId equals s.StepId
                join sd in _manufacturingDBContext.StepDetail on new { s.StepId, a.DepartmentId } equals new { sd.StepId, sd.DepartmentId }
                join po in _manufacturingDBContext.ProductionOrder on ps.ContainerId equals po.ProductionOrderId
                join d in _manufacturingDBContext.ProductionStepLinkData on a.ProductionStepLinkDataId equals d.ProductionStepLinkDataId
                join ldr in _manufacturingDBContext.ProductionStepLinkDataRole on new
                {
                    ps.ProductionStepId,
                    ProductionStepLinkDataRoleTypeId = (int)EnumProductionStepLinkDataRoleType.Output
                }
                equals new
                {
                    ldr.ProductionStepId,
                    ldr.ProductionStepLinkDataRoleTypeId
                }
                join ld in _manufacturingDBContext.ProductionStepLinkData on ldr.ProductionStepLinkDataId equals ld.ProductionStepLinkDataId
                join ildr in _manufacturingDBContext.ProductionStepLinkDataRole on new
                {
                    ldr.ProductionStepLinkDataId,
                    ProductionStepLinkDataRoleTypeId = (int)EnumProductionStepLinkDataRoleType.Input
                }
                equals new
                {
                    ildr.ProductionStepLinkDataId,
                    ildr.ProductionStepLinkDataRoleTypeId
                } into ildrs
                from ildr in ildrs.DefaultIfEmpty()
                join psw in _manufacturingDBContext.ProductionStepWorkInfo on ps.ProductionStepId equals psw.ProductionStepId into psws
                from psw in psws.DefaultIfEmpty()
                orderby a.CreatedDatetimeUtc ascending
                select new
                {
                    a.DepartmentId,
                    a.ProductionStepId,
                    a.AssignmentQuantity,
                    a.StartDate,
                    a.EndDate,
                    a.CreatedDatetimeUtc,
                    TotalQuantity = d.QuantityOrigin - d.OutsourcePartQuantity.GetValueOrDefault(),
                    d.ObjectId,
                    d.ObjectTypeId,
                    s.StepName,
                    ProductivityPerPerson = sd.Quantity,
                    sd.NumberOfPerson,
                    po.ProductionOrderCode,
                    po.ProductionOrderId,
                    Workload = (ld.Quantity - ld.OutsourcePartQuantity.GetValueOrDefault()) * ld.WorkloadConvertRate,
                    OutputQuantity = ld.Quantity - ld.OutsourcePartQuantity.GetValueOrDefault(),
                    IsImport = ildr == null,
                    psw.MinHour,
                    psw.MaxHour,
                })
                .GroupBy(a => new
                {
                    a.DepartmentId,
                    a.ProductionStepId
                })
                .Select(g => new AssignmentCapacityInfo
                {
                    DepartmentId = g.Key.DepartmentId,
                    ProductionStepId = g.Key.ProductionStepId,
                    AssignmentQuantity = g.Max(a => a.AssignmentQuantity),
                    StartDate = g.Max(a => a.StartDate),
                    EndDate = g.Max(a => a.EndDate),
                    CreatedDatetimeUtc = g.Max(a => a.CreatedDatetimeUtc),
                    TotalQuantity = g.Max(a => a.TotalQuantity),
                    Workload = g.Sum(a => a.Workload),
                    ObjectId = g.Max(a => a.ObjectId),
                    ObjectTypeId = g.Max(a => a.ObjectTypeId),
                    StepName = g.Max(a => a.StepName),
                    ProductivityPerPerson = g.Max(a => a.ProductivityPerPerson),
                    NumberOfPerson = g.Max(a => a.NumberOfPerson),
                    ProductionOrderCode = g.Max(a => a.ProductionOrderCode),
                    ProductionOrderId = g.Max(a => a.ProductionOrderId),
                    OutputQuantity = g.Sum(a => a.OutputQuantity),
                    ImportStockQuantity = g.Sum(a => a.IsImport ? a.OutputQuantity : 0),
                    MinHour = g.Max(a => a.MinHour),
                    MaxHour = g.Max(a => a.MaxHour),
                })
                .ToList();

            var otherAssignmentDetails = (
                from a in _manufacturingDBContext.ProductionAssignment
                where departmentIds.Contains(a.DepartmentId)
                    && a.ProductionOrderId != productionOrderId
                    && a.StartDate <= productionTime.EndDate
                    && a.EndDate >= productionTime.StartDate
                join ad in _manufacturingDBContext.ProductionAssignmentDetail on new { a.ProductionOrderId, a.ProductionStepId, a.DepartmentId } equals new { ad.ProductionOrderId, ad.ProductionStepId, ad.DepartmentId }
                select new
                {
                    a.DepartmentId,
                    a.ProductionStepId,
                    ad.QuantityPerDay,
                    ad.WorkDate
                })
                .ToList();

            foreach (var otherAssignment in otherAssignments)
            {
                otherAssignment.ProductionAssignmentDetail = otherAssignmentDetails
                    .Where(d => d.DepartmentId == otherAssignment.DepartmentId && d.ProductionStepId == otherAssignment.ProductionStepId)
                    .Select(d => new AssignmentCapacityDetail
                    {
                        QuantityPerDay = d.QuantityPerDay,
                        WorkDate = d.WorkDate
                    })
                    .ToList();
            }

            var otherProductionStepIds = otherAssignments.Select(a => a.ProductionStepId).Distinct().ToList();
            var otherProductionOrderIds = otherAssignments.Select(a => a.ProductionOrderId).Distinct().ToList();

            var handovers = _manufacturingDBContext.ProductionHandover
                .Where(h => otherProductionStepIds.Contains(h.FromProductionStepId) && departmentIds.Contains(h.FromDepartmentId) && h.Status == (int)EnumHandoverStatus.Accepted)
                .ToList();

            var inventoryRequirements = new Dictionary<long, List<ProductionInventoryRequirementModel>>();

            foreach (var otherProductionOrderId in otherProductionOrderIds)
            {
                var parammeters = new SqlParameter[]
                {
                    new SqlParameter("@ProductionOrderId", otherProductionOrderId)
                };
                var resultData = await _manufacturingDBContext.ExecuteDataProcedure("asp_ProductionHandover_GetInventoryRequirementByProductionOrder", parammeters);

                inventoryRequirements.Add(otherProductionOrderId, resultData.ConvertData<ProductionInventoryRequirementEntity>()
                    .Where(ir => ir.DepartmentId.HasValue && departmentIds.Contains(ir.DepartmentId.Value) && ir.Status == (int)EnumProductionInventoryRequirementStatus.Accepted)
                    .AsQueryable()
                    .ProjectTo<ProductionInventoryRequirementModel>(_mapper.ConfigurationProvider)
                    .ToList());
            }

            departmentIds = otherAssignments.Select(a => a.DepartmentId).Distinct().ToList();
            var startDateUnix = productionTime.StartDate.GetUnix();
            var endDateUnix = productionTime.EndDate.GetUnix();
            if (otherAssignments.Count > 0)
            {
                var minDate = otherAssignments.Min(a => a.StartDate).GetUnix();
                var maxDate = otherAssignments.Max(a => a.EndDate).GetUnix();
                startDateUnix = startDateUnix < minDate ? startDateUnix : minDate;
                endDateUnix = endDateUnix > maxDate ? endDateUnix : maxDate;
            }



            // Lấy thông tin phong ban
            var departmentCalendar = (await _organizationHelperService.GetListDepartmentCalendar(startDateUnix, endDateUnix, departmentIds.ToArray()));

            foreach (var group in otherAssignments.GroupBy(a => new { a.ProductionOrderId, a.ObjectId, a.ObjectTypeId, a.DepartmentId }))
            {
                var totalInventoryQuantity = group.Key.ObjectTypeId == (int)EnumProductionStepLinkDataObjectType.Product
                    ? 0
                    : inventoryRequirements[group.Key.ProductionOrderId]
                    .Where(ir => !ir.ProductionStepId.HasValue && ir.DepartmentId == group.Key.DepartmentId && ir.ProductId == group.Key.ObjectId)
                    .Sum(ir => ir.ActualQuantity);

                var calendar = departmentCalendar.FirstOrDefault(d => d.DepartmentId == group.Key.DepartmentId);

                foreach (var otherAssignment in group)
                {
                    var productionStepName = $"{otherAssignment.StepName}";
                    var completedQuantity = handovers
                    .Where(h => h.FromProductionStepId == otherAssignment.ProductionStepId
                    && h.FromDepartmentId == otherAssignment.DepartmentId
                    && h.ObjectId == otherAssignment.ObjectId
                    && h.ObjectTypeId == otherAssignment.ObjectTypeId)
                    .Sum(h => h.HandoverQuantity);

                    if (otherAssignment.ImportStockQuantity > 0)
                    {
                        var allocatedQuantity = inventoryRequirements[group.Key.ProductionOrderId]
                            .Where(ir => ir.ProductionStepId.HasValue
                            && ir.ProductionStepId.Value == otherAssignment.ProductionStepId
                            && ir.DepartmentId == group.Key.DepartmentId
                            && ir.ProductId == group.Key.ObjectId)
                            .Sum(ir => ir.ActualQuantity);

                        var departmentImportStockQuantity = otherAssignment.AssignmentQuantity * otherAssignment.ImportStockQuantity / otherAssignment.TotalQuantity;
                        var unallocatedQuantity = totalInventoryQuantity > departmentImportStockQuantity - allocatedQuantity ? departmentImportStockQuantity - allocatedQuantity : totalInventoryQuantity;
                        completedQuantity += (allocatedQuantity + unallocatedQuantity);
                        totalInventoryQuantity -= unallocatedQuantity;
                    }

                    var capacityDepartment = new CapacityModel
                    {
                        ProductionOrderCode = otherAssignment.ProductionOrderCode,
                        StepName = productionStepName,
                        Workload = otherAssignment.Workload,
                        AssingmentQuantity = otherAssignment.AssignmentQuantity,
                        LinkDataQuantity = otherAssignment.TotalQuantity,
                        OutputQuantity = otherAssignment.OutputQuantity,
                        StartDate = otherAssignment.StartDate.GetUnix(),
                        EndDate = otherAssignment.EndDate.GetUnix(),
                        CreatedDatetimeUtc = otherAssignment.CreatedDatetimeUtc.GetUnix(),
                        ObjectId = otherAssignment.ObjectId,
                        ObjectTypeId = otherAssignment.ObjectTypeId,
                        CompletedQuantity = completedQuantity
                    };


                    // Nếu có tồn tại năng suất
                    if (otherAssignment.ProductivityPerPerson > 0 && otherAssignment.NumberOfPerson > 0)
                    {
                        foreach (var productionAssignmentDetail in otherAssignment.ProductionAssignmentDetail)
                        {

                            // Tính năng suất tổ theo ngày
                            // Tìm tăng ca
                            var overHour = calendar.DepartmentOverHourInfo.FirstOrDefault(oh => oh.StartDate <= productionAssignmentDetail.WorkDate.GetUnix() && oh.EndDate >= productionAssignmentDetail.WorkDate.GetUnix());
                            var productivity = (otherAssignment.NumberOfPerson + (overHour?.NumberOfPerson ?? 0)) * otherAssignment.ProductivityPerPerson;

                            var capacityPerDay = otherAssignment.TotalQuantity > 0 ? (otherAssignment.Workload
                                * productionAssignmentDetail.QuantityPerDay.Value)
                                / (otherAssignment.TotalQuantity
                                * productivity) : 0;
                            capacityDepartment.CapacityDetail.Add(new CapacityDetailModel
                            {
                                WorkDate = productionAssignmentDetail.WorkDate.GetUnix(),
                                StepName = productionStepName,
                                ProductionOrderCode = otherAssignment.ProductionOrderCode,
                                CapacityPerDay = capacityPerDay,
                                Productivity = productivity
                            });
                        }
                    }
                    else
                    {
                        foreach (var productionAssignmentDetail in otherAssignment.ProductionAssignmentDetail)
                        {
                            var workDateUnix = productionAssignmentDetail.WorkDate.GetUnix();
                            // Tính số giờ làm việc theo ngày của tổ
                            var workingHourInfo = calendar.DepartmentWorkingHourInfo.Where(wh => wh.StartDate <= productionAssignmentDetail.WorkDate.GetUnix()).OrderByDescending(wh => wh.StartDate).FirstOrDefault();
                            var overHour = calendar.DepartmentOverHourInfo.FirstOrDefault(oh => oh.StartDate <= productionAssignmentDetail.WorkDate.GetUnix() && oh.EndDate >= productionAssignmentDetail.WorkDate.GetUnix());

                            var workingHoursPerDay = (decimal)((workingHourInfo?.WorkingHourPerDay ?? 0) + (overHour?.OverHour ?? 0));

                            var totalHour = capacityDepartments[otherAssignment.DepartmentId]
                                .SelectMany(c => c.CapacityDetail)
                                .Where(c => c.WorkDate == workDateUnix)
                                .Sum(c => c.CapacityPerDay);

                            var capacityPerDay = workingHoursPerDay < totalHour ? 0 : workingHoursPerDay - totalHour;
                            if (otherAssignment.MinHour.HasValue && capacityPerDay < otherAssignment.MinHour) capacityPerDay = 0;
                            if (otherAssignment.MaxHour.HasValue && capacityPerDay > otherAssignment.MaxHour) capacityPerDay = otherAssignment.MaxHour;

                            capacityDepartment.CapacityDetail.Add(new CapacityDetailModel
                            {
                                WorkDate = workDateUnix,
                                StepName = productionStepName,
                                ProductionOrderCode = otherAssignment.ProductionOrderCode,
                                CapacityPerDay = capacityPerDay
                            });
                        }
                    }
                    capacityDepartments[otherAssignment.DepartmentId].Add(capacityDepartment);
                }

                if (totalInventoryQuantity > 0)
                {
                    capacityDepartments[group.Key.DepartmentId][capacityDepartments[group.Key.DepartmentId].Count - 1].CompletedQuantity += totalInventoryQuantity;
                }
            }

            return new CapacityOutputModel
            {
                CapacityData = capacityDepartments
            };
        }

        public async Task<IDictionary<int, Dictionary<int, ProductivityModel>>> GetGeneralProductivityDepartments()
        {
            return (await (from sd in _manufacturingDBContext.StepDetail
                           join s in _manufacturingDBContext.Step on sd.StepId equals s.StepId
                           select new
                           {
                               s.StepId,
                               sd.DepartmentId,
                               s.Productivity,
                               sd.NumberOfPerson,
                               s.UnitId
                           })
                    .ToListAsync()
                    )
                    .GroupBy(sd => sd.StepId)
                    .ToDictionary(g => g.Key, g => g.ToDictionary(sd => sd.DepartmentId, sd => new ProductivityModel
                    {
                        NumberOfPerson = sd.NumberOfPerson,
                        ProductivityPerPerson = sd.Productivity.GetValueOrDefault(),
                        UnitId = sd.UnitId
                    }));
        }

        public async Task<IList<ProductionStepWorkInfoOutputModel>> GetListProductionStepWorkInfo(long productionOrderId)
        {
            return await (from w in _manufacturingDBContext.ProductionStepWorkInfo
                          join ps in _manufacturingDBContext.ProductionStep on w.ProductionStepId equals ps.ProductionStepId
                          where ps.ContainerTypeId == (int)EnumContainerType.ProductionOrder && ps.ContainerId == productionOrderId
                          select w)
                          .ProjectTo<ProductionStepWorkInfoOutputModel>(_mapper.ConfigurationProvider)
                          .ToListAsync();
        }

        //public async Task<IList<DepartmentTimeTableModel>> GetDepartmentTimeTable(int[] departmentIds, long startDate, long endDate)
        //{
        //    DateTime startDateTime = startDate.UnixToDateTime().GetValueOrDefault();
        //    DateTime endDateTime = endDate.UnixToDateTime().GetValueOrDefault();

        //    return await _manufacturingDBContext.DepartmentTimeTable
        //        .Where(t => departmentIds.Contains(t.DepartmentId) && t.WorkDate >= startDateTime && t.WorkDate <= endDateTime)
        //        .ProjectTo<DepartmentTimeTableModel>(_mapper.ConfigurationProvider)
        //        .ToListAsync();
        //}

        public async Task<bool> ChangeAssignedProgressStatus(long productionOrderId, long productionStepId, int departmentId, EnumAssignedProgressStatus status)
        {
            var assignment = _manufacturingDBContext.ProductionAssignment
                .FirstOrDefault(a => a.ProductionOrderId == productionOrderId && a.ProductionStepId == productionStepId && a.DepartmentId == departmentId);
            if (assignment == null) throw new BadRequestException(GeneralCode.InvalidParams, "Công việc không tồn tại");
            try
            {
                assignment.AssignedProgressStatus = (int)status;
                assignment.IsManualFinish = true;
                _manufacturingDBContext.SaveChanges();
                await _activityLogService.CreateLog(EnumObjectType.ProductionAssignment, productionOrderId, $"Cập nhật trạng thái phân công sản xuất cho lệnh sản xuất {productionOrderId}", assignment.JsonSerialize());
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateProductAssignment");
                throw;
            }
        }
        private class AssignmentCapacityDetail
        {
            public DateTime WorkDate { get; set; }
            public decimal? QuantityPerDay { get; set; }
            //public decimal? HourPerDay { get; set; }
        }
        private class AssignmentCapacityInfo
        {
            public int DepartmentId { get; set; }
            public long ProductionStepId { get; set; }
            public decimal AssignmentQuantity { get; set; }
            public decimal TotalQuantity { get; set; }
            public decimal Workload { get; set; }
            public int ObjectTypeId { get; set; }
            public long ObjectId { get; set; }
            public string StepName { get; set; }
            public int NumberOfPerson { get; set; }
            public decimal ProductivityPerPerson { get; set; }
            public string ProductionOrderCode { get; set; }
            public long ProductionOrderId { get; set; }
            public decimal OutputQuantity { get; set; }
            public decimal ImportStockQuantity { get; set; }
            public decimal? MinHour { get; set; }
            public decimal? MaxHour { get; set; }

            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public DateTime CreatedDatetimeUtc { get; set; }

            public List<AssignmentCapacityDetail> ProductionAssignmentDetail { get; set; }
        }
    }
}
