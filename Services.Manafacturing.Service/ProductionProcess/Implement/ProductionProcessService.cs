using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.ErrorCodes;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.ManufacturingDB;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Manafacturing.Model.Outsource.RequestPart;
using VErp.Services.Manafacturing.Model.Outsource.RequestStep;
using VErp.Services.Manafacturing.Model.ProductionProcess;
using VErp.Services.Manafacturing.Model.ProductionStep;
using VErp.Services.Manafacturing.Service.Outsource;
using static VErp.Commons.Enums.Manafacturing.EnumProductionProcess;

namespace VErp.Services.Manafacturing.Service.ProductionProcess.Implement
{
    public class ProductionProcessService : IProductionProcessService
    {
        private readonly ManufacturingDBContext _manufacturingDBContext;
        private readonly IActivityLogService _activityLogService;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly IProductHelperService _productHelperService;
        private readonly IOutsourceStepRequestService _outsourceStepRequestService;
        private readonly IValidateProductionProcessService _validateProductionProcessService;

        public ProductionProcessService(ManufacturingDBContext manufacturingDB
            , IActivityLogService activityLogService
            , ILogger<ProductionProcessService> logger
            , IMapper mapper
            , IProductHelperService productHelperService
            , IOutsourceStepRequestService outsourceStepRequestService
            , IValidateProductionProcessService validateProductionProcessService)
        {
            _manufacturingDBContext = manufacturingDB;
            _activityLogService = activityLogService;
            _logger = logger;
            _mapper = mapper;
            _productHelperService = productHelperService;
            _outsourceStepRequestService = outsourceStepRequestService;
            _validateProductionProcessService = validateProductionProcessService;
        }

        public async Task<long> CreateProductionStep(ProductionStepInfo req)
        {
            using (var trans = _manufacturingDBContext.Database.BeginTransaction())
            {
                try
                {
                    var step = _mapper.Map<ProductionStep>((ProductionStepModel)req);
                    await _manufacturingDBContext.ProductionStep.AddAsync(step);
                    await _manufacturingDBContext.SaveChangesAsync();

                    var rInOutStepLinks = await InsertAndUpdateProductionStepLinkData(step.ProductionStepId, req.ProductionStepLinkDatas);
                    await _manufacturingDBContext.ProductionStepLinkDataRole.AddRangeAsync(rInOutStepLinks);
                    await _manufacturingDBContext.SaveChangesAsync();

                    await trans.CommitAsync();

                    await _activityLogService.CreateLog(EnumObjectType.ProductionStep, step.ProductionStepId,
                        $"Tạo mới công đoạn {req.ProductionStepId} của {req.ContainerTypeId.GetEnumDescription()} {req.ContainerId}", req.JsonSerialize());
                    return step.ProductionStepId;
                }
                catch (Exception ex)
                {
                    await trans.RollbackAsync();
                    _logger.LogError(ex, "CreateProductionStep");
                    throw;
                }
            }
        }

        public async Task<bool> DeleteProductionStepById(long productionStepId)
        {
            var productionStep = await _manufacturingDBContext.ProductionStep
                                   .Where(s => s.ProductionStepId == productionStepId)
                                   .Include(x => x.ProductionStepLinkDataRole)
                                   .FirstOrDefaultAsync();
            if (productionStep == null)
                throw new BadRequestException(ProductionProcessErrorCode.NotFoundProductionStep);

            var productStepLinks = await _manufacturingDBContext.ProductionStepLinkData.Include(x => x.ProductionStepLinkDataRole)
                .Where(x => productionStep.ProductionStepLinkDataRole.Select(r => r.ProductionStepLinkDataId)
                .Contains(x.ProductionStepLinkDataId)).ToListAsync();

            using (var trans = _manufacturingDBContext.Database.BeginTransaction())
            {
                try
                {
                    productionStep.IsDeleted = true;
                    foreach (var p in productStepLinks)
                    {
                        if (p.ProductionStepLinkDataRole.Count > 1)
                            throw new BadRequestException(ProductionProcessErrorCode.InvalidDeleteProductionStep,
                                    "Không thể xóa công đoạn!. Đang tồn tại mối quan hệ với công đoạn khác");
                        p.IsDeleted = true;
                    }

                    await _manufacturingDBContext.SaveChangesAsync();
                    await trans.CommitAsync();

                    await _activityLogService.CreateLog(EnumObjectType.ProductionStep, productionStep.ProductionStepId,
                        $"Xóa công đoạn {productionStep.ProductionStepId} của {((EnumContainerType)productionStep.ContainerTypeId).GetEnumDescription()} {productionStep.ContainerId}", productionStep.JsonSerialize());
                    return true;
                }
                catch (Exception ex)
                {
                    await trans.RollbackAsync();
                    _logger.LogError(ex, "DeleteProductionStepById");
                    throw;
                }
            }
        }

        public async Task<ProductionProcessInfo> GetProductionProcessByProductionOrder(long productionOrderId)
        {
            //var productOrderDetailIds = _manufacturingDBContext.ProductionOrderDetail
            //    .Where(s => s.ProductionOrderId == productionOrderId)
            //    .Select(s => s.ProductionOrderDetailId)
            //    .ToList();

            //var productionStepIds = _manufacturingDBContext.ProductionStepOrder
            //    .Where(so => productOrderDetailIds.Contains(so.ProductionOrderDetailId))
            //    .Select(so => so.ProductionStepId)
            //    .Distinct()
            //    .ToList();

            var productionSteps = await _manufacturingDBContext.ProductionStep.AsNoTracking()
                .Include(s => s.Step)
                .Include(s => s.ProductionStepLinkDataRole)
                .ThenInclude(r => r.ProductionStepLinkData)
                .Where(s => s.ContainerId == productionOrderId && s.ContainerTypeId == (int)EnumContainerType.ProductionOrder)
                .ProjectTo<ProductionStepInfo>(_mapper.ConfigurationProvider)
                .ToListAsync();

            // Lấy role chi tiết trong công đoạn
            var roles = productionSteps.SelectMany(s => s.ProductionStepLinkDatas, (s, d) => new ProductionStepLinkDataRoleInput
            {
                ProductionStepId = s.ProductionStepId,
                ProductionStepLinkDataId = d.ProductionStepLinkDataId,
                ProductionStepCode = s.ProductionStepCode,
                ProductionStepLinkDataCode = d.ProductionStepLinkDataCode,
                ProductionStepLinkDataRoleTypeId = d.ProductionStepLinkDataRoleTypeId,
                ProductionStepLinkTypeId = (int)d.ProductionStepLinkTypeId
            }).ToList();

            var stepInfos = productionSteps.Select(s => (ProductionStepModel)s).ToList();

            var dataLinks = productionSteps
                .SelectMany(s => s.ProductionStepLinkDatas)
                .GroupBy(s => s.ProductionStepLinkDataId)
                .ToDictionary(g => g.Key, g => g.First());

            // Tính toán quan hệ của quy trình con
            var productionStepGroupLinkDataRoles = CalcInOutDataForGroup(stepInfos, roles);

            foreach (var group in productionSteps.Where(s => s.IsGroup.Value))
            {
                group.ProductionStepLinkDatas = productionStepGroupLinkDataRoles
                    .Where(r => r.ProductionStepId == group.ProductionStepId)
                    .Select(r => new ProductionStepLinkDataInfo
                    {
                        ProductionStepLinkDataId = dataLinks[r.ProductionStepLinkDataId].ProductionStepLinkDataId,
                        ProductionStepLinkDataCode = dataLinks[r.ProductionStepLinkDataId].ProductionStepLinkDataCode,
                        ObjectId = dataLinks[r.ProductionStepLinkDataId].ObjectId,
                        ObjectTypeId = dataLinks[r.ProductionStepLinkDataId].ObjectTypeId,
                        Quantity = dataLinks[r.ProductionStepLinkDataId].Quantity,
                        OutsourceQuantity = dataLinks[r.ProductionStepLinkDataId].OutsourceQuantity,
                        SortOrder = dataLinks[r.ProductionStepLinkDataId].SortOrder,
                        ProductionStepId = r.ProductionStepId,
                        ProductionStepLinkDataRoleTypeId = r.ProductionStepLinkDataRoleTypeId
                    }).ToList();
            }

            //Tính toán mối quan hệ giữa (stepLink) các công đoạn
            var productionStepLinks = CalcProductionStepLink(roles, productionStepGroupLinkDataRoles);

            return new ProductionProcessInfo
            {
                ProductionSteps = productionSteps,
                ProductionStepLinks = productionStepLinks
            };
        }

        public async Task<ProductionProcessModel> GetProductionProcessByContainerId(EnumContainerType containerTypeId, long containerId)
        {

            var productionSteps = await _manufacturingDBContext.ProductionStep.AsNoTracking()
                .Include(s => s.Step)
                .Include(s => s.ProductionStepLinkDataRole)
                .ThenInclude(r => r.ProductionStepLinkData)
                .Where(s => s.ContainerId == containerId && s.ContainerTypeId == (int)containerTypeId)
                .ToListAsync();

            //Lấy thông tin công đoạn
            var stepInfos = _mapper.Map<List<ProductionStepModel>>(productionSteps);
            //Lấy role chi tiết trong công đoạn
            var roles = productionSteps.SelectMany(x => x.ProductionStepLinkDataRole, (s, d) => new ProductionStepLinkDataRoleInput
            {
                ProductionStepId = s.ProductionStepId,
                ProductionStepLinkDataId = d.ProductionStepLinkDataId,
                ProductionStepCode = s.ProductionStepCode,
                ProductionStepLinkDataCode = d.ProductionStepLinkData.ProductionStepLinkDataCode,
                ProductionStepLinkDataRoleTypeId = (EnumProductionStepLinkDataRoleType)d.ProductionStepLinkDataRoleTypeId,
                ProductionStepLinkTypeId = d.ProductionStepLinkData.ProductionStepLinkTypeId
            }).ToList();

            //Lấy thông tin dữ liệu của steplinkdata
            var lsProductionStepLinkDataId = roles.Select(x => x.ProductionStepLinkDataId).Distinct().ToList();
            var stepLinkDatas = new List<ProductionStepLinkDataInput>();
            if (lsProductionStepLinkDataId.Count > 0)
            {
                var sql = new StringBuilder("Select * from ProductionStepLinkDataExtractInfo v ");
                var parammeters = new List<SqlParameter>();
                var whereCondition = new StringBuilder();

                whereCondition.Append("v.ProductionStepLinkDataId IN ( ");
                for (int i = 0; i < lsProductionStepLinkDataId.Count; i++)
                {
                    var number = lsProductionStepLinkDataId[i];
                    string pName = $"@ProductionStepLinkDataId{i + 1}";

                    if (i == lsProductionStepLinkDataId.Count - 1)
                        whereCondition.Append($"{pName} )");
                    else
                        whereCondition.Append($"{pName}, ");

                    parammeters.Add(new SqlParameter(pName, number));
                }
                if (whereCondition.Length > 0)
                {
                    sql.Append(" WHERE ");
                    sql.Append(whereCondition);
                }

                stepLinkDatas = (await _manufacturingDBContext.QueryDataTable(sql.ToString(), parammeters.Select(p => p.CloneSqlParam()).ToArray()))
                        .ConvertData<ProductionStepLinkDataInput>();
            }

            // Tính toán quan hệ của quy trình con
            var productionStepGroupLinkDataRoles = CalcInOutDataForGroup(stepInfos, roles);

            //Tính toán mối quan hệ giữa (stepLink) các công đoạn
            var productionStepLinks = CalcProductionStepLink(roles, productionStepGroupLinkDataRoles);

            var productionStepOrders = new List<ProductionStepOrderModel>();
            if (containerTypeId == EnumContainerType.ProductionOrder)
            {
                var lsProductionOrderDetailId = await _manufacturingDBContext.ProductionOrderDetail
                    .Where(x => x.ProductionOrderId == containerId)
                    .Select(x => x.ProductionOrderDetailId)
                    .ToListAsync();

                productionStepOrders = await _manufacturingDBContext.ProductionStepOrder
                    .Include(x => x.ProductionStep)
                    .Where(x => lsProductionOrderDetailId.Contains(x.ProductionOrderDetailId))
                    .ProjectTo<ProductionStepOrderModel>(_mapper.ConfigurationProvider)
                    .ToListAsync();
            }

            return new ProductionProcessModel
            {
                ContainerId = containerId,
                ContainerTypeId = containerTypeId,
                ProductionSteps = stepInfos,
                ProductionStepLinkDataRoles = roles,
                ProductionStepLinkDatas = stepLinkDatas,
                ProductionStepLinks = productionStepLinks,
                ProductionStepGroupLinkDataRoles = productionStepGroupLinkDataRoles,
                ProductionStepOrders = productionStepOrders
            };
        }

        private List<ProductionStepLinkDataRoleInput> CalcInOutDataForGroup(List<ProductionStepModel> stepInfos, List<ProductionStepLinkDataRoleInput> roles)
        {
            // Lấy thông tin đầu vào, đầu ra cho quy trình con
            //
            // 1. Lấy danh sách các công đoạn thuộc quy trình con
            // 2. Lấy danh sách đầu vào, đầu ra của tất cả công đoạn trong quy trình con
            // 3. Loại bỏ các role đủ 1 cặp IN/OUT
            // 4. Thêm role cho quy trình con
            var productionStepGroupLinkDataRoles = new List<ProductionStepLinkDataRoleInput>();
            var groupSteps = stepInfos.Where(x => x.IsGroup.Value).ToList();
            foreach (var groupStep in groupSteps)
            {
                // 1. Lấy danh sách các công đoạn thuộc quy trình con
                var children = GetChildren(stepInfos, groupStep.ProductionStepId);
                var childIds = children.Select(s => s.ProductionStepId).ToList();
                // 2. Lấy danh sách đầu vào, đầu ra của tất cả công đoạn trong quy trình con
                var childRoles = roles.Where(r => childIds.Contains(r.ProductionStepId)).ToList();
                // 3. Loại bỏ các role đủ 1 cặp IN/OUT
                var inOutRoles = childRoles
                    .GroupBy(r => r.ProductionStepLinkDataId)
                    .Where(g => g.Count() == 1)
                    .Select(g => g.First())
                    .Select(r => new ProductionStepLinkDataRoleInput
                    {
                        ProductionStepId = groupStep.ProductionStepId,
                        ProductionStepLinkDataId = r.ProductionStepLinkDataId,
                        ProductionStepCode = groupStep.ProductionStepCode,
                        ProductionStepLinkDataCode = r.ProductionStepLinkDataCode,
                        ProductionStepLinkDataRoleTypeId = r.ProductionStepLinkDataRoleTypeId,
                        ProductionStepLinkTypeId = r.ProductionStepLinkTypeId
                    })
                    .ToList();

                productionStepGroupLinkDataRoles.AddRange(inOutRoles);
            }
            return productionStepGroupLinkDataRoles;
        }

        private List<ProductionStepModel> GetChildren(List<ProductionStepModel> stepInfos, long productionStepId)
        {
            var result = new List<ProductionStepModel>();
            var children = stepInfos.Where(s => s.ParentId == productionStepId).ToList();
            result.AddRange(children);
            foreach (var child in children)
            {
                result.AddRange(GetChildren(stepInfos, child.ProductionStepId));
            }
            return result;
        }

        private List<ProductionStepLinkModel> CalcProductionStepLink(List<ProductionStepLinkDataRoleInput> roles, List<ProductionStepLinkDataRoleInput> groupRoles)
        {
            var roleUnions = roles.Union(groupRoles).ToList();

            return CalcProdictonStepLink(roleUnions);
        }

        private static List<ProductionStepLinkModel> CalcProdictonStepLink(List<ProductionStepLinkDataRoleInput> roles)
        {
            var roleGroups = roles.GroupBy(r => r.ProductionStepLinkDataId);
            var productionStepLinks = new List<ProductionStepLinkModel>();

            foreach (var roleGroup in roleGroups)
            {
                var froms = roleGroup.Where(r => r.ProductionStepLinkDataRoleTypeId == EnumProductionProcess.EnumProductionStepLinkDataRoleType.Output).ToList();
                var tos = roleGroup.Where(r => r.ProductionStepLinkDataRoleTypeId == EnumProductionProcess.EnumProductionStepLinkDataRoleType.Input).ToList();
                foreach (var from in froms)
                {
                    bool bExisted = productionStepLinks.Any(r => r.FromStepId == from.ProductionStepId);
                    foreach (var to in tos)
                    {
                        if (!bExisted || !productionStepLinks.Any(r => r.FromStepId == from.ProductionStepId && r.ToStepId == to.ProductionStepId))
                        {
                            productionStepLinks.Add(new ProductionStepLinkModel
                            {
                                FromStepCode = from.ProductionStepCode,
                                FromStepId = from.ProductionStepId,
                                ToStepId = to.ProductionStepId,
                                ToStepCode = to.ProductionStepCode,
                                ProductionStepLinkTypeId = (int)to.ProductionStepLinkTypeId
                            });
                        }
                    }
                }
            }
            return productionStepLinks;
        }

        public async Task<bool> IncludeProductionProcess(int productionOrderId)
        {
            var productionOrder = _manufacturingDBContext.ProductionOrder.FirstOrDefault(o => o.ProductionOrderId == productionOrderId);
            if (productionOrder == null)
                throw new BadRequestException(GeneralCode.InvalidParams, "Không tìm thấy lệnh sản xuất.");

            if (productionOrder.IsDraft)
                throw new BadRequestException(GeneralCode.InvalidParams, "Không được tạo quy trình sản xuất cho lệnh nháp.");

            // Kiểm tra đã tồn tại quy trình sx gắn với lệnh sx 
            var productionOrderDetails = _manufacturingDBContext.ProductionOrderDetail
                .Where(o => o.ProductionOrderId == productionOrderId)
                .ToList();

            if (productionOrderDetails.Count == 0)
                throw new BadRequestException(GeneralCode.InvalidParams, "Không tồn tại sản phẩm trong lệnh sản xuất.");

            var productIds = productionOrderDetails.Select(od => (long)od.ProductId).Distinct().ToList();

            // Lấy ra thông tin đầu ra nhập kho trong quy trình
            var processProductIds = (
                    from ld in _manufacturingDBContext.ProductionStepLinkData
                    join r in _manufacturingDBContext.ProductionStepLinkDataRole on ld.ProductionStepLinkDataId equals r.ProductionStepLinkDataId
                    join ps in _manufacturingDBContext.ProductionStep on r.ProductionStepId equals ps.ProductionStepId
                    where ps.ContainerId == productionOrderId
                    && ps.ContainerTypeId == (int)EnumContainerType.ProductionOrder
                    && ld.ObjectTypeId == (int)EnumProductionStepLinkDataObjectType.Product
                    && productIds.Contains(ld.ObjectId)
                    && r.ProductionStepLinkDataRoleTypeId == (int)EnumProductionStepLinkDataRoleType.Output
                    select ld.ObjectId
                )
                .Distinct()
                .ToList();


            var includeProductIds = productIds.Where(p => !processProductIds.Any(d => d == p)).ToList();

            if (includeProductIds.Count == 0)
            {
                throw new BadRequestException(GeneralCode.InvalidParams, "Quy trình cho lệnh sản xuất đã hoàn thiện.");
            }

            productionOrderDetails.RemoveAll(od => processProductIds.Contains(od.ProductId));

            //var productionOrderDetailIds = productionOrderDetails.Select(od => od.ProductionOrderDetailId).ToList();

            //var hasProcessDetailIds = _manufacturingDBContext.ProductionStepOrder
            //    .Where(so => productionOrderDetailIds.Contains(so.ProductionOrderDetailId))
            //    .Select(so => so.ProductionOrderDetailId)
            //    .Distinct()
            //    .ToList();

            //// Nếu đã có đầy đủ quy trình => thông báo lỗi
            //if (productionOrderDetailIds.Count == hasProcessDetailIds.Count)
            //{
            //    throw new BadRequestException(GeneralCode.InvalidParams, "Quy trình cho lệnh sản xuất đã hoàn thiện.");
            //}

            //// Nếu đã có một phần quy trình => remove phần đã tồn tại trong danh sách detail cần tạo
            //else if (hasProcessDetailIds.Count > 0)
            //{
            //    productionOrderDetails.RemoveAll(od => hasProcessDetailIds.Contains(od.ProductionOrderDetailId));
            //}

            //var productIds = productionOrderDetails.Select(od => (long)od.ProductId).Distinct().ToList();

            var productOrderMap = productionOrderDetails.ToDictionary(p => (long)p.ProductId, p => p.ProductionOrderDetailId);

            var products = await _productHelperService.GetListProducts(productIds.Select(p => (int)p).ToList());
            if (productIds.Count > products.Count) throw new BadRequestException(GeneralCode.InvalidParams, "Xuất hiện mặt hàng không tồn tại.");

            var productionSteps = _manufacturingDBContext.ProductionStep
                .Where(s => s.ContainerTypeId == (int)EnumContainerType.Product && productIds.Contains(s.ContainerId))
                .ToList();

            var productionStepIds = productionSteps.Select(s => s.ProductionStepId).ToList();

            var linkDataRoles = _manufacturingDBContext.ProductionStepLinkDataRole
                .Where(r => productionStepIds.Contains(r.ProductionStepId))
                .ToList();
            var linkDataIds = linkDataRoles.Select(r => r.ProductionStepLinkDataId).ToList();

            var linkDatas = _manufacturingDBContext.ProductionStepLinkData
                .Where(d => linkDataIds.Contains(d.ProductionStepLinkDataId)).ToList();

            using var trans = _manufacturingDBContext.Database.BeginTransaction();
            try
            {
                // Update status cho chi tiết LSX
                productionOrder.ProductionOrderStatus = (int)EnumProductionStatus.Waiting;

                //// Update status cho chi tiết LSX
                //foreach (var item in productionOrderDetails)
                //{
                //    item.Status = (int)EnumProductionStatus.Waiting;
                //}

                //
                var bottomStep = _manufacturingDBContext.ProductionStep
                    .Where(ps => ps.ContainerId == productionOrderId && ps.ContainerTypeId == (int)EnumContainerType.ProductionOrder)
                    .OrderByDescending(ps => ps.CoordinateY)
                    .FirstOrDefault();

                var maxY = bottomStep?.CoordinateY.GetValueOrDefault() ?? 0;
                var newMaxY = maxY;
                foreach (var productionOrderDetail in productionOrderDetails)
                {
                    // Tạo step ứng với quy trình sản xuất
                    var product = products.First(p => p.ProductId == productionOrderDetail.ProductId);

                    //var processStep = new ProductionStep
                    //{
                    //    StepId = null,
                    //    Title = $"{product.ProductCode} / {product.ProductName}",
                    //    ContainerTypeId = (int)EnumContainerType.ProductionOrder,
                    //    ContainerId = productionOrderId,
                    //    IsGroup = true,
                    //    ProductionStepCode = Guid.NewGuid().ToString(),
                    //};
                    //_manufacturingDBContext.ProductionStep.Add(processStep);
                    //_manufacturingDBContext.SaveChanges();

                    // Map step quy trình với chi tiết lệnh sản xuất
                    //_manufacturingDBContext.ProductionStepOrder.Add(new ProductionStepOrder
                    //{
                    //    ProductionOrderDetailId = productionOrderDetail.ProductionOrderDetailId,
                    //    ProductionStepId = processStep.ProductionStepId
                    //});

                    // create productionStep
                    var stepMap = new Dictionary<long, ProductionStep>();
                    //var stepOrders = new List<ProductionStep>();

                    var parentIdUpdater = new List<ProductionStep>();
                    var steps = productionSteps.Where(s => s.ContainerId == product.ProductId).ToList();
                    foreach (var step in steps)
                    {
                        var newStep = new ProductionStep
                        {
                            StepId = step.StepId,
                            Title = step.Title,
                            ContainerTypeId = (int)EnumContainerType.ProductionOrder,
                            ProductionStepCode = Guid.NewGuid().ToString(),
                            ContainerId = productionOrderId,
                            IsGroup = steps.Any(s => s.ParentId == step.ProductionStepId),
                            CoordinateX = step.CoordinateX,
                            CoordinateY = maxY + step.CoordinateY,
                            SortOrder = step.SortOrder,
                            Workload = step.Workload * (productionOrderDetail.Quantity + productionOrderDetail.ReserveQuantity).GetValueOrDefault(),
                            IsFinish = step.IsFinish
                        };
                        if (newStep.CoordinateY.GetValueOrDefault() > newMaxY) newMaxY = newStep.CoordinateY.GetValueOrDefault();
                        if (step.ParentId.HasValue)
                        {
                            parentIdUpdater.Add(step);
                        }
                        //else
                        //{
                        //    newStep.ParentId = processStep.ProductionStepId;
                        //    newStep.ParentCode = processStep.ProductionStepCode;
                        //}
                        _manufacturingDBContext.ProductionStep.Add(newStep);
                        stepMap.Add(step.ProductionStepId, newStep);
                        //stepOrders.Add(newStep);
                    }
                    _manufacturingDBContext.SaveChanges();

                    // update parentId
                    foreach (var step in parentIdUpdater)
                    {
                        if (!step.ParentId.HasValue) continue;
                        stepMap[step.ProductionStepId].ParentId = stepMap[step.ParentId.Value].ProductionStepId;
                        stepMap[step.ProductionStepId].ParentCode = stepMap[step.ParentId.Value].ProductionStepCode;
                    }

                    //// Map step công đoạn, quy trình con với chi tiết lệnh sản xuất
                    //foreach (var item in stepOrders)
                    //{
                    //    _manufacturingDBContext.ProductionStepOrder.Add(new ProductionStepOrder
                    //    {
                    //        ProductionOrderDetailId = productionOrderDetail.ProductionOrderDetailId,
                    //        ProductionStepId = item.ProductionStepId
                    //    });
                    //}

                    var stepIds = steps.Select(s => s.ProductionStepId).ToList();
                    var roles = linkDataRoles.Where(r => stepIds.Contains(r.ProductionStepId)).ToList();
                    var dataIds = roles.Select(r => r.ProductionStepLinkDataId).ToList();
                    var datas = linkDatas.Where(d => dataIds.Contains(d.ProductionStepLinkDataId)).ToList();

                    // Create data
                    var linkDataMap = new Dictionary<long, ProductionStepLinkData>();
                    foreach (var item in datas)
                    {
                        // Tính số lượng vật tư cần dùng cho quy trình
                        var newLinkData = new ProductionStepLinkData
                        {
                            ObjectId = item.ObjectId,
                            ObjectTypeId = item.ObjectTypeId,
                            Quantity = item.Quantity * (productionOrderDetail.Quantity + productionOrderDetail.ReserveQuantity).GetValueOrDefault(),
                            QuantityOrigin = item.QuantityOrigin * (productionOrderDetail.Quantity + productionOrderDetail.ReserveQuantity).GetValueOrDefault(),
                            SortOrder = item.SortOrder,
                            ProductionStepLinkDataCode = Guid.NewGuid().ToString(),
                            ProductionStepLinkTypeId = item.ProductionStepLinkTypeId,
                            ProductionStepLinkDataTypeId = item.ProductionStepLinkDataTypeId,
                        };

                        _manufacturingDBContext.ProductionStepLinkData.Add(newLinkData);
                        linkDataMap.Add(item.ProductionStepLinkDataId, newLinkData);
                    }
                    _manufacturingDBContext.SaveChanges();

                    // Create role
                    foreach (var role in roles)
                    {
                        var newRole = new ProductionStepLinkDataRole
                        {
                            ProductionStepLinkDataId = linkDataMap[role.ProductionStepLinkDataId].ProductionStepLinkDataId,
                            ProductionStepId = stepMap[role.ProductionStepId].ProductionStepId,
                            ProductionStepLinkDataRoleTypeId = role.ProductionStepLinkDataRoleTypeId
                        };
                        _manufacturingDBContext.ProductionStepLinkDataRole.Add(newRole);
                    }
                    _manufacturingDBContext.SaveChanges();

                    maxY = newMaxY;
                }

                await UpdateStatusValidForProductionOrder(EnumContainerType.ProductionOrder, productionOrderId, (await GetProductionProcessByContainerId(EnumContainerType.ProductionOrder, productionOrderId)));

                await trans.CommitAsync();

                return true;
            }
            catch (Exception ex)
            {
                await trans.RollbackAsync();
                _logger.LogError(ex, "CreateProductionProcess");
                throw;
            }
        }

        public async Task<bool> MergeProductionProcess(int productionOrderId, IList<long> productionStepIds)
        {
            var productionSteps = _manufacturingDBContext.ProductionStep
                .Where(s => s.ContainerId == productionOrderId
                && productionStepIds.Contains(s.ProductionStepId)
                && s.ContainerTypeId == (int)EnumContainerType.ProductionOrder
                && !s.ParentId.HasValue
                && !s.StepId.HasValue)
                .ToList();

            if (productionSteps.Count != productionStepIds.Count()) throw new BadRequestException(GeneralCode.InvalidParams, "Không tìm thấy quy trình trong lệnh sản xuất");

            var productionStepOrder = _manufacturingDBContext.ProductionStepOrder.Where(so => productionStepIds.Contains(so.ProductionStepId)).ToList();

            using (var trans = _manufacturingDBContext.Database.BeginTransaction())
            {
                try
                {
                    // Tạo mới
                    var productionStep = new ProductionStep
                    {
                        Title = $"Quy trình chung ({string.Join(",", productionSteps.Select(s => s.Title).ToList())}",
                        ContainerTypeId = (int)EnumContainerType.ProductionOrder,
                        ContainerId = productionOrderId,
                        IsGroup = true
                    };
                    _manufacturingDBContext.ProductionStep.Add(productionStep);

                    // Xóa danh sách cũ
                    foreach (var item in productionSteps)
                    {
                        item.IsDeleted = true;
                    }
                    _manufacturingDBContext.SaveChanges();

                    // Maping quy trình với chi tiết lệnh SX
                    foreach (var item in productionStepOrder)
                    {
                        _manufacturingDBContext.ProductionStepOrder.Add(new ProductionStepOrder
                        {
                            ProductionStepId = productionStep.ProductionStepId,
                            ProductionOrderDetailId = item.ProductionOrderDetailId
                        });
                    }
                    _manufacturingDBContext.ProductionStepOrder.RemoveRange(productionStepOrder);

                    var productionStepIdsType = productionStepIds.Cast<long?>().ToList();
                    var childProductionSteps = _manufacturingDBContext.ProductionStep.Where(s => productionStepIdsType.Contains(s.ParentId)).ToList();
                    // Cập nhật parentStep
                    foreach (var child in childProductionSteps)
                    {
                        child.ParentId = productionStep.ProductionStepId;
                    }

                    _manufacturingDBContext.SaveChanges();

                    await trans.CommitAsync();

                    return true;
                }
                catch (Exception ex)
                {
                    await trans.RollbackAsync();
                    _logger.LogError(ex, "MergeProductionProcess");
                    throw;
                }
            }
        }

        public async Task<bool> MergeProductionStep(int productionOrderId, IList<long> productionStepIds)
        {
            var productionSteps = _manufacturingDBContext.ProductionStep
                .Where(s => s.ContainerId == productionOrderId && productionStepIds.Contains(s.ProductionStepId) && s.ContainerTypeId == (int)EnumProductionProcess.EnumContainerType.ProductionOrder)
                .ToList();
            if (productionSteps.Count <= 1)
                throw new BadRequestException(GeneralCode.InvalidParams, "Yêu cầu gộp 2 công đoạn trở lên");

            if (productionSteps.Count != productionStepIds.Count())
                throw new BadRequestException(GeneralCode.InvalidParams, "Không tìm thấy quy trình trong lệnh sản xuất");

            if (productionSteps.Any(s => !s.StepId.HasValue))
                throw new BadRequestException(GeneralCode.InvalidParams, "Không được phép gộp quy trình con");

            var roles = _manufacturingDBContext.ProductionStepLinkDataRole
                .Where(r => productionStepIds.Contains(r.ProductionStepId))
                .ToList();

            var linkDataIds = roles.Select(r => r.ProductionStepLinkDataId).Distinct().ToList();

            var linkDatas = _manufacturingDBContext.ProductionStepLinkData
                .Where(d => linkDataIds.Contains(d.ProductionStepLinkDataId))
                .ToDictionary(d => d.ProductionStepLinkDataId, d => d);

            var group = roles.GroupBy(r => r.ProductionStepId).ToDictionary(g => g.Key, g => g.Select(r => new
            {
                r.ProductionStepLinkDataRoleTypeId,
                linkDatas[r.ProductionStepLinkDataId].ObjectId,
                linkDatas[r.ProductionStepLinkDataId].ObjectTypeId,
            }).ToList());

            // Validate loại công đoạn
            if (productionSteps.Select(s => s.StepId).Distinct().Count() != 1)
                throw new BadRequestException(GeneralCode.InvalidParams, "Không được gộp các công đoạn không đồng nhất");

            // validate input, output
            var firstStepId = productionStepIds[0];
            for (int indx = 1; indx < productionStepIds.Count; indx++)
            {
                var stepId = productionStepIds[indx];
                var bOk = group[stepId].Count == group[firstStepId].Count
                    && group[stepId].All(r => group[firstStepId].Any(p => p.ObjectId == r.ObjectId && p.ObjectTypeId == r.ObjectTypeId && p.ProductionStepLinkDataRoleTypeId == r.ProductionStepLinkDataRoleTypeId));
                if (!bOk) throw new BadRequestException(GeneralCode.InvalidParams, "Không được gộp các công đoạn không đồng nhất");
            }

            using (var trans = _manufacturingDBContext.Database.BeginTransaction())
            {
                try
                {
                    // Gộp các đầu ra, đầu vào step vào step đầu tiên
                    foreach (var role in roles)
                    {
                        if (role.ProductionStepId == productionStepIds[0]) continue;
                        role.ProductionStepId = productionStepIds[0];
                    }
                    // Xóa các step còn lại
                    foreach (var step in productionSteps)
                    {
                        if (step.ProductionStepId == productionStepIds[0]) continue;
                        step.IsDeleted = true;
                    }

                    // Maping quy trình với chi tiết lệnh SX
                    var productionStepOrder = _manufacturingDBContext.ProductionStepOrder
                        .Where(so => productionStepIds.Contains(so.ProductionStepId)).ToList();
                    var orderDetailIds = productionStepOrder.Select(so => so.ProductionOrderDetailId).Distinct();
                    // Xóa mapping cũ 
                    _manufacturingDBContext.ProductionStepOrder.RemoveRange(productionStepOrder);
                    // Tạo lại mapping mới
                    foreach (var orderDetailId in orderDetailIds)
                    {
                        _manufacturingDBContext.ProductionStepOrder.Add(new ProductionStepOrder
                        {
                            ProductionStepId = productionStepIds[0],
                            ProductionOrderDetailId = orderDetailId
                        });
                    }

                    _manufacturingDBContext.SaveChanges();

                    await trans.CommitAsync();

                    return true;
                }
                catch (Exception ex)
                {
                    await trans.RollbackAsync();
                    _logger.LogError(ex, "MergeProductionStep");
                    throw;
                }
            }
        }

        public async Task<ProductionStepInfo> GetProductionStepById(long productionStepId)
        {
            var productionStep = await _manufacturingDBContext.ProductionStep.AsNoTracking()
                                    .Where(s => s.ProductionStepId == productionStepId)
                                    .ProjectTo<ProductionStepInfo>(_mapper.ConfigurationProvider)
                                    .FirstOrDefaultAsync();
            if (productionStep == null)
                throw new BadRequestException(ProductionProcessErrorCode.NotFoundProductionStep);

            productionStep.ProductionStepLinkDatas = await _manufacturingDBContext.ProductionStepLinkDataRole.AsNoTracking()
                                            .Where(d => d.ProductionStepId == productionStep.ProductionStepId)
                                            .Include(x => x.ProductionStep)
                                            .ProjectTo<ProductionStepLinkDataInfo>(_mapper.ConfigurationProvider)
                                            .ToListAsync();

            return productionStep;
        }

        public async Task<bool> UpdateProductionStepById(long productionStepId, ProductionStepInfo req)
        {
            var sProductionStep = await _manufacturingDBContext.ProductionStep
                                   .Where(s => s.ProductionStepId == productionStepId)
                                   .FirstOrDefaultAsync();
            if (sProductionStep == null)
                throw new BadRequestException(ProductionProcessErrorCode.NotFoundProductionStep);

            var dInOutStepLinks = await _manufacturingDBContext.ProductionStepLinkDataRole
                                    .Where(x => x.ProductionStepId == sProductionStep.ProductionStepId)
                                    .ToListAsync();

            using (var trans = _manufacturingDBContext.Database.BeginTransaction())
            {
                try
                {
                    _mapper.Map((ProductionStepModel)req, sProductionStep);

                    var rInOutStepLinks = await InsertAndUpdateProductionStepLinkData(sProductionStep.ProductionStepId, req.ProductionStepLinkDatas);
                    _manufacturingDBContext.ProductionStepLinkDataRole.RemoveRange(dInOutStepLinks);
                    await _manufacturingDBContext.SaveChangesAsync();

                    await _manufacturingDBContext.ProductionStepLinkDataRole.AddRangeAsync(rInOutStepLinks);
                    await _manufacturingDBContext.SaveChangesAsync();

                    await trans.CommitAsync();

                    await _activityLogService.CreateLog(EnumObjectType.ProductionStep, sProductionStep.ProductionStepId,
                        $"Cập nhật công đoạn {sProductionStep.ProductionStepId} của {((EnumProductionProcess.EnumContainerType)sProductionStep.ContainerTypeId).GetEnumDescription()} {sProductionStep.ContainerId}", req.JsonSerialize());
                    return true;
                }
                catch (Exception ex)
                {
                    await trans.RollbackAsync();
                    _logger.LogError(ex, "UpdateProductionStepById");
                    throw;
                }
            }

        }

        private async Task<IList<ProductionStepLinkDataRole>> InsertAndUpdateProductionStepLinkData(long productionStepId, List<ProductionStepLinkDataInfo> source)
        {
            var nProductionInSteps = source.Where(x => x.ProductionStepLinkDataId <= 0)
                                            .Select(x => _mapper.Map<ProductionStepLinkData>((ProductionStepLinkDataModel)x)).ToList();

            var uProductionInSteps = source.Where(x => x.ProductionStepLinkDataId > 0)
                                            .Select(x => (ProductionStepLinkDataModel)x).ToList();

            var destProductionInSteps = _manufacturingDBContext.ProductionStepLinkData
                .Where(x => uProductionInSteps.Select(x => x.ProductionStepLinkDataId).Contains(x.ProductionStepLinkDataId)).ToList();

            foreach (var d in destProductionInSteps)
            {
                var s = uProductionInSteps.FirstOrDefault(s => s.ProductionStepLinkDataId == d.ProductionStepLinkDataId);
                if (s != null)
                    _mapper.Map(s, d);
            }

            await _manufacturingDBContext.ProductionStepLinkData.AddRangeAsync(nProductionInSteps);
            await _manufacturingDBContext.SaveChangesAsync();

            var inOutStepLinks = source.Where(x => x.ProductionStepLinkDataId > 0).Select(x => new ProductionStepLinkDataRole
            {
                ProductionStepLinkDataRoleTypeId = (int)x.ProductionStepLinkDataRoleTypeId,
                ProductionStepLinkDataId = x.ProductionStepLinkDataId,
                ProductionStepId = productionStepId
            }).ToList();

            foreach (var p in nProductionInSteps)
            {
                var s = source.FirstOrDefault(x => x.ObjectId == p.ObjectId && (int)x.ObjectTypeId == p.ObjectTypeId);
                if (s == null)
                    throw new BadRequestException(GeneralCode.ItemNotFound);

                inOutStepLinks.Add(new ProductionStepLinkDataRole
                {
                    ProductionStepLinkDataRoleTypeId = (int)s.ProductionStepLinkDataRoleTypeId,
                    ProductionStepLinkDataId = p.ProductionStepLinkDataId,
                    ProductionStepId = productionStepId
                });
            }

            return inOutStepLinks;
        }

        public async Task<bool> InsertAndUpdatePorductionStepRoleClient(ProductionStepRoleClientModel model)
        {
            var info = _manufacturingDBContext.ProductionStepRoleClient
                .Where(x => x.ContainerId == model.ContainerId && x.ContainerTypeId == model.ContainerTypeId)
                .FirstOrDefault();

            if (info != null)
                info.ClientData = model.ClientData;
            else
                _manufacturingDBContext.Add(_mapper.Map<ProductionStepRoleClient>(model));

            await _manufacturingDBContext.SaveChangesAsync();
            return true;
        }

        public async Task<string> GetPorductionStepRoleClient(int containerTypeId, long containerId)
        {
            var info = await _manufacturingDBContext.ProductionStepRoleClient
                .Where(x => x.ContainerId == containerId && x.ContainerTypeId == containerTypeId)
                .FirstOrDefaultAsync();
            
            return info == null? "" : info.ClientData;
        }

        public async Task<long> CreateProductionStepGroup(ProductionStepGroupModel req)
        {
            var stepGroup = _mapper.Map<ProductionStep>(req);
            stepGroup.IsGroup = true;
            _manufacturingDBContext.ProductionStep.Add(stepGroup);
            await _manufacturingDBContext.SaveChangesAsync();

            var child = _manufacturingDBContext.ProductionStep.Where(s => req.ListProductionStepId.Contains(s.ProductionStepId));
            foreach (var c in child)
            {
                c.ParentId = stepGroup.ProductionStepId;
            }
            await _manufacturingDBContext.SaveChangesAsync();

            await _activityLogService.CreateLog(EnumObjectType.ProductionStep, stepGroup.ProductionStepId,
                        $"Tạo mới quy trình con {req.ProductionStepId} của {req.ContainerTypeId.GetEnumDescription()} {req.ContainerId}", req.JsonSerialize());
            return stepGroup.ProductionStepId;
        }

        public async Task<bool> UpdateProductionStepSortOrder(IList<PorductionStepSortOrderModel> req)
        {
            var trans = await _manufacturingDBContext.Database.BeginTransactionAsync();
            try
            {
                var lsProductionStep = await _manufacturingDBContext.ProductionStep.Where(x => req.Select(y => y.ProductionStepId).Contains(x.ProductionStepId)).ToListAsync();
                foreach (var p in lsProductionStep)
                {
                    p.SortOrder = req.SingleOrDefault(y => y.ProductionStepId == p.ProductionStepId).SortOrder;
                }
                await _manufacturingDBContext.SaveChangesAsync();
                await trans.CommitAsync();
                await _activityLogService.CreateLog(EnumObjectType.ProductionStep, req.First().ProductionStepId,
                        $"Cập nhật vị trí cho các công đoạn", req.JsonSerialize());
                return true;
            }
            catch (Exception ex)
            {
                await trans.RollbackAsync();
                _logger.LogError(ex, "UpdateProductionStepSortOrder");
                throw;
            }
        }

        #region Production process

        public async Task<bool> UpdateProductionProcess(EnumContainerType containerTypeId, long containerId, ProductionProcessModel req)
        {
            var trans = await _manufacturingDBContext.Database.BeginTransactionAsync();
            try
            {
                await UpdateProductionProcessManual(containerTypeId, containerId, req);

                await trans.CommitAsync();
                await _activityLogService.CreateLog(EnumObjectType.ProductionProcess, req.ContainerId, "Cập nhật quy trình sản xuất", req.JsonSerialize());

                return true;
            }
            catch (Exception ex)
            {
                await trans.RollbackAsync();
                _logger.LogError(ex, "UpdateProductionProcess");
                throw;
            }

        }

        private async Task UpdateProductionProcessManual(EnumContainerType containerTypeId, long containerId, ProductionProcessModel req)
        {
            var productionStepOutsourced = await _outsourceStepRequestService.GetProductionStepHadOutsourceStepRequest(containerTypeId == EnumContainerType.ProductionOrder ? containerId : 0);

            //Cập nhật, xóa và tạo mới steplinkdata
            var lsStepLinkDataId = (from s in _manufacturingDBContext.ProductionStep
                                    join r in _manufacturingDBContext.ProductionStepLinkDataRole on s.ProductionStepId equals r.ProductionStepId
                                    where s.ContainerId == containerId && s.ContainerTypeId == (int)containerTypeId
                                    select r.ProductionStepLinkDataId).Distinct();
            var sourceStepLinkData = await _manufacturingDBContext.ProductionStepLinkData.Where(p => lsStepLinkDataId.Contains(p.ProductionStepLinkDataId)).ToListAsync();
            foreach (var dest in sourceStepLinkData)
            {
                var source = req.ProductionStepLinkDatas.FirstOrDefault(x => x.ProductionStepLinkDataId == dest.ProductionStepLinkDataId);
                if (source != null)
                {
                    _mapper.Map(source, dest);
                }
                else
                {
                    if (containerTypeId == EnumContainerType.ProductionOrder && (dest.OutsourceQuantity.GetValueOrDefault() > 0 || dest.ExportOutsourceQuantity.GetValueOrDefault() > 0))
                    {
                        var outsourceStepRequest = await _manufacturingDBContext.OutsourceStepRequest.Include(x => x.OutsourceStepRequestData)
                            .Where(x => x.OutsourceStepRequestData.Any(x => x.ProductionStepLinkDataId == dest.ProductionStepLinkDataId))
                            .FirstOrDefaultAsync();
                        throw new BadRequestException(ProductionProcessErrorCode.ValidateProductionStepLinkData, $"Không thể xóa chi tiết nằm trong YCGC công đoạn \"{outsourceStepRequest.OutsourceStepRequestCode}\"");
                    }
                    dest.IsDeleted = true;
                }
            }

            var newStepLinkData = req.ProductionStepLinkDatas.AsQueryable().ProjectTo<ProductionStepLinkData>(_mapper.ConfigurationProvider)
                .Where(x => !sourceStepLinkData.Select(y => y.ProductionStepLinkDataId).Contains(x.ProductionStepLinkDataId))
                .ToList();

            await _manufacturingDBContext.ProductionStepLinkData.AddRangeAsync(newStepLinkData);
            await _manufacturingDBContext.SaveChangesAsync();

            //Cập nhật, xóa và tạo mới step
            var sourceStep = await _manufacturingDBContext.ProductionStep.Where(p => p.ContainerId == containerId && p.ContainerTypeId == (int)containerTypeId).ToListAsync();
            foreach (var dest in sourceStep)
            {
                var source = req.ProductionSteps.SingleOrDefault(x => x.ProductionStepId == dest.ProductionStepId);
                if (source != null)
                    _mapper.Map(source, dest);
                else
                {
                    var os = productionStepOutsourced.FirstOrDefault(x => x.ProductionStepId == dest.ProductionStepId);
                    if (containerTypeId == EnumContainerType.ProductionOrder && os != null)
                        throw new BadRequestException(ProductionProcessErrorCode.ValidateProductionStep, $"Không thể xóa công đoạn nằm trong YCGC công đoạn {os.OutsourceStepRequestCode}");
                    dest.IsDeleted = true;
                }
            }

            var newStep = req.ProductionSteps.AsQueryable().ProjectTo<ProductionStep>(_mapper.ConfigurationProvider)
                .Where(x => !sourceStep.Select(y => y.ProductionStepId).Contains(x.ProductionStepId))
                .ToList();

            await _manufacturingDBContext.ProductionStep.AddRangeAsync(newStep);
            await _manufacturingDBContext.SaveChangesAsync();

            //Cập nhật role steplinkdata trong step
            newStep.AddRange(sourceStep.Where(x => !x.IsDeleted).ToList());
            newStepLinkData.AddRange(sourceStepLinkData.Where(x => !x.IsDeleted).ToList());

            var roles = from r in req.ProductionStepLinkDataRoles
                        join s in newStep on r.ProductionStepCode equals s.ProductionStepCode
                        join d in newStepLinkData on r.ProductionStepLinkDataCode equals d.ProductionStepLinkDataCode
                        select new ProductionStepLinkDataRole
                        {
                            ProductionStepId = s.ProductionStepId,
                            ProductionStepLinkDataId = d.ProductionStepLinkDataId,
                            ProductionStepLinkDataRoleTypeId = (int)r.ProductionStepLinkDataRoleTypeId
                        };
            var oldRoles = _manufacturingDBContext.ProductionStepLinkDataRole.Where(x => newStep.Select(y => y.ProductionStepId).Contains(x.ProductionStepId)).ToList();

            _manufacturingDBContext.ProductionStepLinkDataRole.RemoveRange(oldRoles);
            await _manufacturingDBContext.SaveChangesAsync();
            await _manufacturingDBContext.ProductionStepLinkDataRole.AddRangeAsync(roles);

            //Gán parentId nếu trong nhóm công đoạn có QTSX con
            foreach (var s in newStep)
            {
                if (!string.IsNullOrWhiteSpace(s.ParentCode))
                {
                    var p = newStep.FirstOrDefault(x => x.ProductionStepCode.Equals(s.ParentCode));
                    if (p != null)
                        s.ParentId = p.ProductionStepId;
                }
            }
            await _manufacturingDBContext.SaveChangesAsync();

            //Lưu công đoạn thuộc QTSX trong LSX nào
            if (containerTypeId == EnumContainerType.ProductionOrder && req.ProductionStepOrders.Count > 0)
            {
                var lsProductionOrderDetailId = await _manufacturingDBContext.ProductionOrderDetail.AsNoTracking().Where(x => x.ProductionOrderId == containerId).Select(x => x.ProductionOrderDetailId).ToListAsync();
                var lsProductionStepOrderOld = await _manufacturingDBContext.ProductionStepOrder.AsNoTracking().Where(x => lsProductionOrderDetailId.Contains(x.ProductionOrderDetailId)).ToListAsync();
                _manufacturingDBContext.ProductionStepOrder.RemoveRange(lsProductionStepOrderOld);
                await _manufacturingDBContext.SaveChangesAsync();

                req.ProductionStepOrders.ForEach(x =>
                {
                    var step = newStep.FirstOrDefault(y => y.ProductionStepCode.Equals(x.ProductionStepCode));
                    x.ProductionStepId = step.ProductionStepId;
                });
                var lsProductionStepOrderNew = _mapper.Map<IList<ProductionStepOrder>>(req.ProductionStepOrders);
                await _manufacturingDBContext.ProductionStepOrder.AddRangeAsync(lsProductionStepOrderNew);
                await _manufacturingDBContext.SaveChangesAsync();


            }

            if (containerTypeId == EnumContainerType.ProductionOrder)
            {
                await UpdateStatusValidForProductionOrder(containerTypeId, containerId, req);
            }

            await _manufacturingDBContext.SaveChangesAsync();
        }

        private async Task UpdateStatusValidForProductionOrder(EnumContainerType containerTypeId, long containerId, ProductionProcessModel process)
        {
            var productionOrder = await _manufacturingDBContext.ProductionOrder.FirstOrDefaultAsync(x => x.ProductionOrderId == containerId);
            productionOrder.IsResetProductionProcess = true;
            productionOrder.IsInvalid = (await _validateProductionProcessService.ValidateProductionProcess(containerTypeId, containerId, process)).Count() > 0;

            await _manufacturingDBContext.SaveChangesAsync();
        }

        #endregion

        public async Task<IList<ProductionStepLinkDataInput>> GetProductionStepLinkDataByListId(List<long> lsProductionStepLinkDataId)
        {
            var stepLinkDatas = new List<ProductionStepLinkDataInput>();
            if (lsProductionStepLinkDataId.Count > 0)
            {
                var sql = new StringBuilder("Select * from ProductionStepLinkDataExtractInfo v ");
                var parammeters = new List<SqlParameter>();
                var whereCondition = new StringBuilder();

                whereCondition.Append("v.ProductionStepLinkDataId IN ( ");
                for (int i = 0; i < lsProductionStepLinkDataId.Count; i++)
                {
                    var number = lsProductionStepLinkDataId[i];
                    string pName = $"@ProductionStepLinkDataId{i + 1}";

                    if (i == lsProductionStepLinkDataId.Count - 1)
                        whereCondition.Append($"{pName} )");
                    else
                        whereCondition.Append($"{pName}, ");

                    parammeters.Add(new SqlParameter(pName, number));
                }
                if (whereCondition.Length > 0)
                {
                    sql.Append(" WHERE ");
                    sql.Append(whereCondition);
                }

                stepLinkDatas = (await _manufacturingDBContext.QueryDataTable(sql.ToString(), parammeters.Select(p => p.CloneSqlParam()).ToArray()))
                        .ConvertData<ProductionStepLinkDataInput>();
            }

            return stepLinkDatas;
        }

        public async Task<IList<ProductionStepLinkDataRoleModel>> GetListStepLinkDataForOutsourceStep(List<long> lsProductionStepId)
        {
            var lsProductionStep = await _manufacturingDBContext.ProductionStep.AsNoTracking()
                .Include(x => x.ProductionStepLinkDataRole)
                .ThenInclude(x => x.ProductionStepLinkData)
                .Where(x => lsProductionStepId.Contains(x.ProductionStepId))
                .ToListAsync();

            var groupByContainerId = lsProductionStep.GroupBy(x => x.ContainerId);
            if (groupByContainerId.Count() > 1)
                throw new BadRequestException(ProductionProcessErrorCode.ListProductionStepNotInContainerId);

            var roles = lsProductionStep.SelectMany(x => x.ProductionStepLinkDataRole.Where(x => x.ProductionStepLinkData.ProductionStepLinkDataTypeId == (int)EnumProductionStepLinkDataType.None), (s, d) => new ProductionStepLinkDataRoleModel
            {
                ProductionStepId = s.ProductionStepId,
                ProductionStepLinkDataId = d.ProductionStepLinkDataId,
                ProductionStepLinkDataRoleTypeId = (EnumProductionStepLinkDataRoleType)d.ProductionStepLinkDataRoleTypeId,
            }).ToList();

            // 3. Loại bỏ các role đủ 1 cặp IN/OUT
            var inOutRoles = roles
                .GroupBy(r => r.ProductionStepLinkDataId)
                .Where(g => g.Count() == 1)
                .Select(g => g.First())
                .ToList();

            return inOutRoles;
        }

        public async Task<bool> ValidateProductionStepRelationship(List<long> lsProductionStepId)
        {
            var lsProductionStep = await _manufacturingDBContext.ProductionStep.AsNoTracking()
                .Include(x => x.ProductionStepLinkDataRole)
                .Where(x => lsProductionStepId.Contains(x.ProductionStepId))
                .ToListAsync();

            var groupByContainerId = lsProductionStep.GroupBy(x => x.ContainerId);
            if (groupByContainerId.Count() > 1)
                throw new BadRequestException(ProductionProcessErrorCode.ListProductionStepNotInContainerId);

            var roles = lsProductionStep.SelectMany(x => x.ProductionStepLinkDataRole).ToList();

            var linkDataRoles = roles
                .GroupBy(r => r.ProductionStepLinkDataId)
                .Where(g => g.Count() == 2)
                .ToList();

            return linkDataRoles.Count() == (lsProductionStepId.Count - 1);
        }

        public async Task<NonCamelCaseDictionary> GroupProductionStepRelationShip(IList<long> productionStepIds)
        {
            int indexGroup = 1;
            var groupRelationship = new NonCamelCaseDictionary();

            var lsProductionStep = await _manufacturingDBContext.ProductionStep.AsNoTracking()
               .Include(x => x.ProductionStepLinkDataRole)
               .Where(x => productionStepIds.Contains(x.ProductionStepId))
               .ToListAsync();

            if (lsProductionStep.GroupBy(x => x.ContainerId).Count() > 1)
                throw new BadRequestException(ProductionProcessErrorCode.ListProductionStepNotInContainerId);

            var roles = lsProductionStep.SelectMany(x => x.ProductionStepLinkDataRole).ToList();
            var groupbyLinkDataRole = roles
                .GroupBy(r => r.ProductionStepLinkDataId)
                .Where(g => g.Count() == 2)
                .ToList();
            /*
             * 1. Lấy ra các công đoạn mà không có cặp linkData InOut
             * 2. Mỗi công đoạn này sẽ tạo 1 nhóm riêng biệt
             */
            var productionStepNotCoupleRole = productionStepIds
                                                .Where(value => !groupbyLinkDataRole
                                                                .SelectMany(x => x.Select(y => y.ProductionStepId))
                                                                .Distinct().Contains(value));
            foreach (var productionStepid in productionStepNotCoupleRole)
            {
                var ls = new List<long>();
                ls.Add(productionStepid);
                groupRelationship.Add($"gc#{indexGroup}", ls);
                indexGroup++;
            }
            /*
             * Đệ quy để tìm các công đoạn trong cùng 1 nhóm.
             */
            var groupbyLinkDataRoleScanned = new List<IGrouping<long, ProductionStepLinkDataRole>>();
            for (int i = 0; i < groupbyLinkDataRole.Count; i++)
            {
                var role = groupbyLinkDataRole[i];
                if (groupbyLinkDataRoleScanned.Contains(role))
                    continue;

                groupbyLinkDataRoleScanned.Add(role);
                var lsProductionStepIdInGroup = new List<long>();
                foreach (var linkData in role)
                {
                    if (lsProductionStepIdInGroup.Contains(linkData.ProductionStepId))
                        continue;
                    lsProductionStepIdInGroup.Add(linkData.ProductionStepId);
                    var temp = groupbyLinkDataRole.Where(x => x.Key != role.Key && x.Where(y => y.ProductionStepId == linkData.ProductionStepId).Count() > 0).ToList();
                    TraceProductionStepRelationShip(temp, groupbyLinkDataRoleScanned, groupbyLinkDataRole, lsProductionStepIdInGroup);
                }
                groupRelationship.Add($"gc#{indexGroup}", lsProductionStepIdInGroup);
                indexGroup++;
            }

            return groupRelationship;
        }

        private static void TraceProductionStepRelationShip(List<IGrouping<long, ProductionStepLinkDataRole>> groupbyLinkDataRole
            , List<IGrouping<long, ProductionStepLinkDataRole>> groupbyLinkDataRoleScanned
            , List<IGrouping<long, ProductionStepLinkDataRole>> groupbyLinkDataRoleOrigin
            , List<long> lsProductionStepIdInGroup)
        {
            foreach (var role in groupbyLinkDataRole)
            {
                if (groupbyLinkDataRoleScanned.Contains(role))
                    continue;
                groupbyLinkDataRoleScanned.Add(role);
                foreach (var linkData in role)
                {
                    if (lsProductionStepIdInGroup.Contains(linkData.ProductionStepId))
                        continue;
                    lsProductionStepIdInGroup.Add(linkData.ProductionStepId);

                    var temp = groupbyLinkDataRoleOrigin.Where(x => x.Where(y => y.ProductionStepId == linkData.ProductionStepId).Count() > 0).ToList();
                    TraceProductionStepRelationShip(temp, groupbyLinkDataRoleScanned, groupbyLinkDataRoleOrigin, lsProductionStepIdInGroup);
                }
                groupbyLinkDataRoleOrigin.Remove(role);
            }
        }

        public async Task<bool> SetProductionStepWorkload(IList<ProductionStepWorkload> productionStepWorkload)
        {
            var productionSteps = await _manufacturingDBContext.ProductionStep
                .Where(y => productionStepWorkload.Select(x => x.ProductionStepId).Contains(y.ProductionStepId))
                .ToListAsync();

            foreach (var productionStep in productionSteps)
            {
                var w = productionStepWorkload.FirstOrDefault(x => x.ProductionStepId == productionStep.ProductionStepId);
                if (w != null)
                    _mapper.Map(w, productionStep);
            }

            await _manufacturingDBContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateMarkInvalidOutsourcePartRequest(long productionOrderId)
        {
            var trans = await _manufacturingDBContext.Database.BeginTransactionAsync();
            try
            {
                var outsourcePartRequests = await _manufacturingDBContext.OutsourcePartRequest
                .Include(x => x.ProductionOrderDetail)
                .Include(x => x.OutsourcePartRequestDetail)
                .Where(x => x.ProductionOrderDetail.ProductionOrderId == productionOrderId)
                .ToListAsync();

                var outsourcePartRequestDetailIds = outsourcePartRequests.SelectMany(x => x.OutsourcePartRequestDetail).Select(x => x.OutsourcePartRequestDetailId);

                var totalQuantityAllocate = (await _manufacturingDBContext.ProductionStep.AsNoTracking()
                    .Include(s => s.ProductionStepLinkDataRole)
                    .ThenInclude(r => r.ProductionStepLinkData)
                    .Where(x => x.ContainerId == productionOrderId && x.ContainerTypeId == (int)EnumContainerType.ProductionOrder)
                    .ProjectTo<ProductionStepInfo>(_mapper.ConfigurationProvider).ToListAsync())
                    .SelectMany(x => x.ProductionStepLinkDatas)
                    .Where(x => outsourcePartRequestDetailIds.Contains(x.OutsourceRequestDetailId.GetValueOrDefault()))
                    .GroupBy(x => x.OutsourceRequestDetailId.GetValueOrDefault())
                    .ToDictionary(k => k.Key, v => v.Sum(x => x.Quantity));

                foreach (var rq in outsourcePartRequests)
                {
                    rq.MarkInvalid = false;
                    if (totalQuantityAllocate.Count() == 0)
                        rq.MarkInvalid = true;
                    else
                    {
                        foreach (var rqd in rq.OutsourcePartRequestDetail)
                        {
                            if (!totalQuantityAllocate.ContainsKey(rqd.OutsourcePartRequestDetailId)
                                    || (totalQuantityAllocate[rqd.OutsourcePartRequestDetailId] != rqd.Quantity))
                            {
                                rq.MarkInvalid = true;
                                break;
                            }
                        }
                    }
                }

                await _manufacturingDBContext.SaveChangesAsync();
                await trans.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await trans.TryRollbackTransactionAsync();
                _logger.LogError(ex, "UpdateMarkInvalidOutsourcePartRequest");
                throw;
            }

        }

        public async Task<bool> UpdateMarkInvalidOutsourceStepRequest(long productionOrderId)
        {
            var trans = await _manufacturingDBContext.Database.BeginTransactionAsync();
            try
            {
                var productionStepOutsourced = await _outsourceStepRequestService.GetProductionStepHadOutsourceStepRequest(productionOrderId);

                var outsourceStepRequests = await _manufacturingDBContext.OutsourceStepRequest.Where(x => x.ProductionOrderId == productionOrderId).ToListAsync();

                var productionStepInfos = await _manufacturingDBContext.ProductionStep.AsNoTracking()
                .Include(s => s.Step)
                .Include(s => s.ProductionStepLinkDataRole)
                .ThenInclude(r => r.ProductionStepLinkData)
                .Where(s => productionStepOutsourced.Select(s => s.ProductionStepId).Contains(s.ProductionStepId))
                .ProjectTo<ProductionStepInfo>(_mapper.ConfigurationProvider)
                .ToListAsync();

                foreach (var rq in outsourceStepRequests)
                {
                    rq.MarkInvalid = false;

                    var stepInRequests = productionStepOutsourced.Where(p => p.OutsourceStepRequestId == rq.OutsourceStepRequestId).Select(s => s.ProductionStepId);
                    var stepInfoInRequests = productionStepInfos.Where(p => stepInRequests.Contains(p.ProductionStepId));
                    foreach (var s in stepInfoInRequests)
                    {
                        foreach (var l in s.ProductionStepLinkDatas)
                        {
                            if (l.ExportOutsourceQuantity > l.Quantity || l.OutsourceQuantity > l.Quantity)
                            {
                                rq.MarkInvalid = true;
                                break;
                            }
                        }
                    }
                }

                await _manufacturingDBContext.SaveChangesAsync();
                await trans.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await trans.TryRollbackTransactionAsync();
                _logger.LogError(ex, "UpdateMarkInvalidOutsourceStepRequest");
                throw;
            }
        }

        public async Task<bool> CopyProductionProcess(EnumContainerType containerTypeId, long fromContainerId, long toContainerId)
        {
            var process = await GetProductionProcessByContainerId(containerTypeId, fromContainerId);

            var trans = await _manufacturingDBContext.Database.BeginTransactionAsync();

            try
            {
                var semiIds = process.ProductionStepLinkDatas
                                .Where(x => x.ObjectTypeId == EnumProductionStepLinkDataObjectType.ProductSemi)
                                .Select(x => x.ObjectId);
                var lsProductSemi = await _manufacturingDBContext.ProductSemi.AsNoTracking()
                    .Where(s => semiIds.Contains(s.ProductSemiId))
                    .ToListAsync();


                process.ProductionSteps.ForEach(x => { x.ProductionStepId = 0; x.ContainerId = toContainerId; });
                process.ProductionStepLinkDatas.ForEach(x => { 
                    x.ProductionStepLinkDataId = 0; 
                    if(x.ObjectTypeId == EnumProductionStepLinkDataObjectType.ProductSemi)
                    {
                        var p = lsProductSemi.FirstOrDefault(s => s.ProductSemiId == x.ObjectId);
                        if (p == null)
                            throw new BadRequestException(ProductSemiErrorCode.NotFoundProductSemi);
                        p.ProductSemiId = 0;
                        p.ContainerId = toContainerId;

                        _manufacturingDBContext.ProductSemi.Add(p);
                        _manufacturingDBContext.SaveChanges();

                        x.ObjectId = p.ProductSemiId;
                    }
                });
                process.ProductionStepLinkDataRoles.ForEach(r => { r.ProductionStepId = 0; r.ProductionStepLinkDataId = 0; });

                await UpdateProductionProcessManual(containerTypeId, toContainerId, new ProductionProcessModel
                {
                    ProductionStepLinkDataRoles = process.ProductionStepLinkDataRoles,
                    ContainerId = toContainerId,
                    ContainerTypeId = containerTypeId,
                    ProductionStepLinkDatas = process.ProductionStepLinkDatas,
                    ProductionSteps = process.ProductionSteps,
                });

                await trans.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await trans.TryRollbackTransactionAsync();
                _logger.LogError("CopyProductionProcess", ex);
                throw;
            }
            
        }

    }
}
