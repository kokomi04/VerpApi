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
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.ManufacturingDB;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Manafacturing.Model.ProductionOrder;
using VErp.Services.Manafacturing.Model.ProductionProcess;
using VErp.Services.Manafacturing.Model.ProductionStep;
using static VErp.Commons.Enums.Manafacturing.EnumProductionProcess;
using ProductSemiEnity = VErp.Infrastructure.EF.ManufacturingDB.ProductSemi;

namespace VErp.Services.Manafacturing.Service.ProductionProcess.Implement
{
    public class ProductionProcessService : IProductionProcessService
    {
        private readonly ManufacturingDBContext _manufacturingDBContext;
        private readonly IActivityLogService _activityLogService;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly IProductHelperService _productHelperService;
        private readonly IValidateProductionProcessService _validateProductionProcessService;

        public ProductionProcessService(ManufacturingDBContext manufacturingDB
            , IActivityLogService activityLogService
            , ILogger<ProductionProcessService> logger
            , IMapper mapper
            , IProductHelperService productHelperService
            , IValidateProductionProcessService validateProductionProcessService)
        {
            _manufacturingDBContext = manufacturingDB;
            _activityLogService = activityLogService;
            _logger = logger;
            _mapper = mapper;
            _productHelperService = productHelperService;
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
            var productionSteps = await _manufacturingDBContext.ProductionStep.AsNoTracking()
                .Include(s => s.Step)
                .Include(s => s.OutsourceStepRequest)
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
                ProductionStepLinkTypeId = (int)d.ProductionStepLinkTypeId,
                ProductionStepLinkDataGroup = d.ProductionStepLinkDataGroup
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

            var productionProcessInfo = new ProductionProcessInfo
            {
                ProductionSteps = SortProductionProcess(productionSteps, productionStepLinks),
                ProductionStepLinks = productionStepLinks
            };

            return productionProcessInfo;
        }

        private List<ProductionStepInfo> SortProductionProcess(List<ProductionStepInfo> productionSteps, List<ProductionStepLinkModel> productionStepLinks)
        {
            var result = new List<ProductionStepInfo>();
            var lstProductionStepGroups = productionSteps
                .Where(ps => ps.StepId.HasValue && ps.IsGroup.GetValueOrDefault() && !ps.IsFinish)
                .ToList();

            var lstProductionStepGroupIds = lstProductionStepGroups.Select(ps => ps.ProductionStepId).ToList();

            var lstProductionStepGroupLinks = productionStepLinks
                .Where(l => lstProductionStepGroupIds.Contains(l.FromStepId) && lstProductionStepGroupIds.Contains(l.ToStepId))
                .ToList();

            var sortGroups = SortProductionSteps(lstProductionStepGroups, lstProductionStepGroupLinks);
            var sortGroupIds = sortGroups.Select(g => g.ProductionStepId).ToList();
            result.AddRange(sortGroups);

            foreach(var group in sortGroups)
            {
                var childProductionSteps = productionSteps
                    .Where(ps => ps.StepId.HasValue && !ps.IsGroup.GetValueOrDefault() && ps.ParentId == group.ProductionStepId && !ps.IsFinish)
                    .ToList();
                result.AddRange(childProductionSteps);
            }

            result.AddRange(productionSteps
               .Where(ps => ps.StepId.HasValue && !ps.IsGroup.GetValueOrDefault() && (!ps.ParentId.HasValue || !sortGroupIds.Contains(ps.ParentId.Value)) && !ps.IsFinish)
               .ToList());

            result.AddRange(productionSteps
               .Where(ps => !ps.StepId.HasValue || ps.IsFinish)
               .ToList());

            return result;
        }

        private List<ProductionStepInfo> SortProductionSteps(List<ProductionStepInfo> lstProductionSteps, List<ProductionStepLinkModel> lstProductionStepLinks)
        {
            // Lấy danh sách cần sắp xếp
            var sortedProductionSteps = new List<ProductionStepInfo>();
            // Lấy danh sách step kết thúc
            var endProductionSteps = lstProductionSteps
                .Where(ps => !lstProductionStepLinks.Any(l => l.FromStepId == ps.ProductionStepId))
                .OrderBy(ps => ps.ProductionStepId)
                .ToList();

            // Duyệt tất cả step kết thúc
            foreach (var endProductionStep in endProductionSteps)
            {
                sortedProductionSteps.Add(endProductionStep);

                // Lấy danh sách node trước đó và không ra nhiều nhánh
                IncludePrevProductionStep(endProductionStep.ProductionStepId, ref lstProductionSteps, ref lstProductionStepLinks, ref sortedProductionSteps);

                lstProductionSteps.Remove(endProductionStep);
                lstProductionStepLinks.RemoveAll(l => l.ToStepId == endProductionStep.ProductionStepId);
            }
            sortedProductionSteps.Reverse();
            return sortedProductionSteps;
        }




        private void IncludePrevProductionStep(long productionStepId, ref List<ProductionStepInfo> lstProductionSteps, ref List<ProductionStepLinkModel> lstProductionStepLinks, ref List<ProductionStepInfo> sortedProductionSteps)
        {
            var lstTempProductionStepLinks = new List<ProductionStepLinkModel>();
            lstTempProductionStepLinks.AddRange(lstProductionStepLinks);
            var lstTempProductionSteps = new List<ProductionStepInfo>();
            lstTempProductionSteps.AddRange(lstProductionSteps);

            // Kiểm tra có tồn tại node trước đó ra nhiều nhánh
            var isMultiple = lstProductionStepLinks
                .Any(l => l.ToStepId == productionStepId
                && lstTempProductionStepLinks.Any(ol => ol.ToStepId != l.ToStepId && ol.FromStepId == l.FromStepId)
                && lstTempProductionSteps.Any(ps => ps.ProductionStepId == l.FromStepId));

            // Lấy danh sách node trước đó và không có nhiều nhánh đầu ra
            var prevProductionStepIds = lstProductionStepLinks
                .Where(l => l.ToStepId == productionStepId
                && !lstTempProductionStepLinks.Any(ol => ol.ToStepId != l.ToStepId && ol.FromStepId == l.FromStepId)
                && lstTempProductionSteps.Any(ps => ps.ProductionStepId == l.FromStepId))
                .Select(l => l.FromStepId)
                .OrderBy(ps => ps)
                .ToList();

            foreach (var prevProductionStepId in prevProductionStepIds)
            {
                var prevProductionStep = lstProductionSteps.First(ps => ps.ProductionStepId == prevProductionStepId);
                sortedProductionSteps.Add(prevProductionStep);

                // Tiếp tục lấy danh sách node tiếp theo và không có nhiều nhánh đầu ra
                IncludePrevProductionStep(prevProductionStep.ProductionStepId, ref lstProductionSteps, ref lstProductionStepLinks, ref sortedProductionSteps);

                lstProductionSteps.Remove(prevProductionStep);
                lstProductionStepLinks.RemoveAll(l => l.ToStepId == prevProductionStepId);
            }

            if (isMultiple && prevProductionStepIds.Count > 0)
            {
                IncludePrevProductionStep(productionStepId, ref lstProductionSteps, ref lstProductionStepLinks, ref sortedProductionSteps);
            }
        }

        public async Task<ProductionProcessModel> GetProductionProcessByContainerId(EnumContainerType containerTypeId, long containerId)
        {

            var productionSteps = await _manufacturingDBContext.ProductionStep.AsNoTracking()
                .Where(s => s.ContainerId == containerId && s.ContainerTypeId == (int)containerTypeId)
                .Include(s => s.Step)
                .Include(s => s.OutsourceStepRequest)
                .Include(s => s.ProductionStepLinkDataRole)
                .ThenInclude(r => r.ProductionStepLinkData)
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
                ProductionStepLinkTypeId = d.ProductionStepLinkData.ProductionStepLinkTypeId,
                ProductionStepLinkDataGroup = d.ProductionStepLinkDataGroup
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

            return new ProductionProcessModel
            {
                ContainerId = containerId,
                ContainerTypeId = containerTypeId,
                ProductionSteps = stepInfos,
                ProductionStepLinkDataRoles = roles,
                ProductionStepLinkDatas = stepLinkDatas,
                ProductionStepLinks = productionStepLinks,
                ProductionStepGroupLinkDataRoles = productionStepGroupLinkDataRoles,
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
                        ProductionStepLinkTypeId = r.ProductionStepLinkTypeId,
                        ProductionStepLinkDataGroup = r.ProductionStepLinkDataGroup
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

            return CalcProductonStepLink(roleUnions);
        }

        private static List<ProductionStepLinkModel> CalcProductonStepLink(List<ProductionStepLinkDataRoleInput> roles)
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
                                ProductionStepLinkTypeId = to.ProductionStepLinkTypeId
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
                            Workload = step.Workload * (productionOrderDetail.Quantity + productionOrderDetail.ReserveQuantity).GetValueOrDefault() / product.Coefficient,
                            IsFinish = step.IsFinish
                        };
                        if (newStep.CoordinateY.GetValueOrDefault() > newMaxY) newMaxY = newStep.CoordinateY.GetValueOrDefault();
                        if (step.ParentId.HasValue)
                        {
                            parentIdUpdater.Add(step);
                        }

                        _manufacturingDBContext.ProductionStep.Add(newStep);
                        stepMap.Add(step.ProductionStepId, newStep);
                    }
                    _manufacturingDBContext.SaveChanges();

                    // update parentId
                    foreach (var step in parentIdUpdater)
                    {
                        if (!step.ParentId.HasValue) continue;
                        stepMap[step.ProductionStepId].ParentId = stepMap[step.ParentId.Value].ProductionStepId;
                        stepMap[step.ProductionStepId].ParentCode = stepMap[step.ParentId.Value].ProductionStepCode;
                    }

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
                            Quantity = item.Quantity * (productionOrderDetail.Quantity + productionOrderDetail.ReserveQuantity).GetValueOrDefault() / product.Coefficient,
                            QuantityOrigin = item.QuantityOrigin * (productionOrderDetail.Quantity + productionOrderDetail.ReserveQuantity).GetValueOrDefault() / product.Coefficient,
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
                            ProductionStepLinkDataRoleTypeId = role.ProductionStepLinkDataRoleTypeId,
                            ProductionStepLinkDataGroup = role.ProductionStepLinkDataGroup
                        };
                        _manufacturingDBContext.ProductionStepLinkDataRole.Add(newRole);
                    }
                    _manufacturingDBContext.SaveChanges();

                    maxY = newMaxY;
                }


                // Copy roleClient
                var dataClient = await _manufacturingDBContext.ProductionStepRoleClient.FirstOrDefaultAsync(x => x.ContainerId == productionOrderId && x.ContainerTypeId == (int)EnumContainerType.ProductionOrder);
                var roleClients = (await _manufacturingDBContext.ProductionStepRoleClient.AsNoTracking()
                         .Where(x => productionOrderDetails.Select(p => (long)p.ProductId).Contains(x.ContainerId) && x.ContainerTypeId == (int)EnumContainerType.Product)
                         .ToListAsync()).SelectMany(x => (x.ClientData.JsonDeserialize<IList<RoleClientData>>()));

                if (dataClient != null)
                {
                    var roleClientModelOrigin = dataClient.ClientData.JsonDeserialize<List<RoleClientData>>();
                    roleClientModelOrigin.AddRange(roleClients);

                    dataClient.ClientData = roleClientModelOrigin.JsonSerialize();
                }
                else
                {
                    _manufacturingDBContext.ProductionStepRoleClient.Add(new ProductionStepRoleClient
                    {
                        ClientData = roleClients.JsonSerialize(),
                        ContainerId = productionOrderId,
                        ContainerTypeId = (int)EnumContainerType.ProductionOrder
                    });
                }

                await _manufacturingDBContext.SaveChangesAsync();

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

        //public async Task<bool> MergeProductionProcess(int productionOrderId, IList<long> productionStepIds)
        //{
        //    var productionSteps = _manufacturingDBContext.ProductionStep
        //        .Where(s => s.ContainerId == productionOrderId
        //        && productionStepIds.Contains(s.ProductionStepId)
        //        && s.ContainerTypeId == (int)EnumContainerType.ProductionOrder
        //        && !s.ParentId.HasValue
        //        && !s.StepId.HasValue)
        //        .ToList();

        //    if (productionSteps.Count != productionStepIds.Count()) throw new BadRequestException(GeneralCode.InvalidParams, "Không tìm thấy quy trình trong lệnh sản xuất");

        //    var productionStepOrder = _manufacturingDBContext.ProductionStepOrder.Where(so => productionStepIds.Contains(so.ProductionStepId)).ToList();

        //    using (var trans = _manufacturingDBContext.Database.BeginTransaction())
        //    {
        //        try
        //        {
        //            // Tạo mới
        //            var productionStep = new ProductionStep
        //            {
        //                Title = $"Quy trình chung ({string.Join(",", productionSteps.Select(s => s.Title).ToList())}",
        //                ContainerTypeId = (int)EnumContainerType.ProductionOrder,
        //                ContainerId = productionOrderId,
        //                IsGroup = true
        //            };
        //            _manufacturingDBContext.ProductionStep.Add(productionStep);

        //            // Xóa danh sách cũ
        //            foreach (var item in productionSteps)
        //            {
        //                item.IsDeleted = true;
        //            }
        //            _manufacturingDBContext.SaveChanges();

        //            // Maping quy trình với chi tiết lệnh SX
        //            foreach (var item in productionStepOrder)
        //            {
        //                _manufacturingDBContext.ProductionStepOrder.Add(new ProductionStepOrder
        //                {
        //                    ProductionStepId = productionStep.ProductionStepId,
        //                    ProductionOrderDetailId = item.ProductionOrderDetailId
        //                });
        //            }
        //            _manufacturingDBContext.ProductionStepOrder.RemoveRange(productionStepOrder);

        //            var productionStepIdsType = productionStepIds.Cast<long?>().ToList();
        //            var childProductionSteps = _manufacturingDBContext.ProductionStep.Where(s => productionStepIdsType.Contains(s.ParentId)).ToList();
        //            // Cập nhật parentStep
        //            foreach (var child in childProductionSteps)
        //            {
        //                child.ParentId = productionStep.ProductionStepId;
        //            }

        //            _manufacturingDBContext.SaveChanges();

        //            await trans.CommitAsync();

        //            return true;
        //        }
        //        catch (Exception ex)
        //        {
        //            await trans.RollbackAsync();
        //            _logger.LogError(ex, "MergeProductionProcess");
        //            throw;
        //        }
        //    }
        //}

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
                    //var productionStepOrder = _manufacturingDBContext.ProductionStepOrder
                    //    .Where(so => productionStepIds.Contains(so.ProductionStepId)).ToList();
                    //var orderDetailIds = productionStepOrder.Select(so => so.ProductionOrderDetailId).Distinct();
                    // Xóa mapping cũ 
                    //_manufacturingDBContext.ProductionStepOrder.RemoveRange(productionStepOrder);
                    // Tạo lại mapping mới
                    //foreach (var orderDetailId in orderDetailIds)
                    //{
                    //    _manufacturingDBContext.ProductionStepOrder.Add(new ProductionStepOrder
                    //    {
                    //        ProductionStepId = productionStepIds[0],
                    //        ProductionOrderDetailId = orderDetailId
                    //    });
                    //}

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

            return info == null ? "" : info.ClientData;
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
            if (req.ProductionSteps.Count() > 0 && req.ProductionSteps.Any(x => x.IsGroup == true && x.IsFinish == false && !x.StepId.HasValue))
                throw new BadRequestException(ProductionProcessErrorCode.ValidateProductionStep, "Trong QTSX đang có công đoạn trắng. Cần thiết lập nó là công đoạn gì.");

            if (req.ProductionStepLinkDataRoles.GroupBy(x => new { x.ProductionStepCode, x.ProductionStepLinkDataCode, x.ProductionStepLinkDataRoleTypeId })
                .Any(x => x.Count() > 1))
                throw new BadRequestException(ProductionProcessErrorCode.ValidateProductionStepLinkDataRole, "Xuất hiện role trùng nhau");

            if (req.ProductionSteps.GroupBy(x => x.ProductionStepCode)
                .Any(x => x.Count() > 1))
                throw new BadRequestException(ProductionProcessErrorCode.ValidateProductionStep, "Xuất hiện công đoạn trùng nhau mã code");

            if (req.ProductionStepLinkDatas.GroupBy(x => x.ProductionStepLinkDataCode)
                .Any(x => x.Count() > 1))
                throw new BadRequestException(ProductionProcessErrorCode.ValidateProductionStepLinkData, "Xuất hiện chi tiết trùng nhau mã code");

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
                        throw new BadRequestException(ProductionProcessErrorCode.ValidateProductionStepLinkData,
                            $"Không thể xóa chi tiết liên quan đến YCGC công đoạn");
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
                    if (containerTypeId == EnumContainerType.ProductionOrder && dest.OutsourceStepRequestId.GetValueOrDefault() > 0)
                        throw new BadRequestException(ProductionProcessErrorCode.ValidateProductionStep, $"Không thể xóa công đoạn có liên quan đến YCGC công đoạn");
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
                            ProductionStepLinkDataRoleTypeId = (int)r.ProductionStepLinkDataRoleTypeId,
                            ProductionStepLinkDataGroup = r.ProductionStepLinkDataGroup
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


            if (containerTypeId == EnumContainerType.ProductionOrder)
            {
                await UpdateStatusValidForProductionOrder(containerTypeId, containerId, req);
                await ValidOutsourcePartRequest(containerId);
                await ValidOutsourceStepRequest(containerId);
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

        public async Task<IList<GroupProductionStepToOutsource>> GroupProductionStepToOutsource(EnumContainerType containerType, long containerId, long[] arrProductionStepId)
        {
            int indexGroup = 1;
            var data = new List<GroupProductionStepToOutsource>();
            var groupRelationship = new NonCamelCaseDictionary();

            var lsProductionStepInfo = await _manufacturingDBContext.ProductionStep.AsNoTracking()
                .Where(x => arrProductionStepId.Contains(x.ParentId.GetValueOrDefault()) || arrProductionStepId.Contains(x.ProductionStepId))
                .ProjectTo<ProductionStepModel>(_mapper.ConfigurationProvider)
                .ToListAsync();

            var productionStepChildIds = lsProductionStepInfo.Where(x => arrProductionStepId.Contains(x.ParentId.GetValueOrDefault())).Select(x => x.ProductionStepId).ToArray();
            var roles = await _manufacturingDBContext.ProductionStepLinkDataRole.AsNoTracking()
                .Include(x => x.ProductionStep)
                .Where(x => x.ProductionStep.ContainerId == containerId && x.ProductionStep.ContainerTypeId == (int)containerType)
                .ProjectTo<ProductionStepLinkDataRoleInput>(_mapper.ConfigurationProvider)
                .ToListAsync();


            var lsRoleProductionStepParent = CalcInOutDataForGroup(lsProductionStepInfo, roles);

            var groupbyLinkDataRole = lsRoleProductionStepParent
                .GroupBy(r => r.ProductionStepLinkDataId)
                .Where(g => g.Count() == 2)
                .ToList();
            /*
             * 1. Lấy ra các công đoạn mà không có cặp linkData InOut
             * 2. Mỗi công đoạn này sẽ tạo 1 nhóm riêng biệt
             */
            var productionStepNotCoupleRole = arrProductionStepId
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
            var groupbyLinkDataRoleScanned = new List<IGrouping<long, ProductionStepLinkDataRoleInput>>();
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

            //foreach (var (key, value) in groupRelationship)
            //{
            //    var stepIds = value as IList<long>;
            //    var calcTotalOutputMap = roles.Where(x => stepIds.Contains(x.ProductionStepId) && x.ProductionStepLinkDataRoleTypeId == EnumProductionStepLinkDataRoleType.Output)
            //        .GroupBy(r => r.ProductionStepId)
            //        .ToDictionary(k => k.Key, v => v.Count());
            //    var roleOutside = roles.Where(x => stepIds.Contains(x.ProductionStepId) )
            //        .GroupBy(r => r.ProductionStepLinkDataId)
            //        .Where(g => g.Count() == 1 && g.First().ProductionStepLinkDataRoleTypeId == EnumProductionStepLinkDataRoleType.Output)
            //        .SelectMany(r => r)
            //        .GroupBy(r => r.ProductionStepId)
            //        .Where(g => g.Count() < calcTotalOutputMap[g.Key])
            //        .SelectMany(r => r)
            //        .ToArray();


            //    if (roleOutside.Length > 0 && TraceProductionStepInsideGroupProductionStepToOutsource(roles, stepIds, roleOutside))
            //    {
            //        groupRelationship.Remove(key);
            //    }

            //}

            //if (groupRelationship.Count() == 0)
            //    throw new BadRequestException(GeneralCode.InternalError, "Nhóm công đoạn không thể đi gia công do tồn tại công đoạn kết nối trung gian");

            foreach (var (key, value) in groupRelationship)
            {
                var stepIds = value as IList<long>;

                var item = GetGroupProductionStepToOutsource(roles, stepIds, key);
                data.Add(item);
            }
            return data;
        }

        private GroupProductionStepToOutsource GetGroupProductionStepToOutsource(List<ProductionStepLinkDataRoleInput> roles, IList<long> stepIds, string title = "")
        {
            var calcTotalOutputMap = roles.Where(x => stepIds.Contains(x.ProductionStepId) && x.ProductionStepLinkDataRoleTypeId == EnumProductionStepLinkDataRoleType.Output)
                                .GroupBy(r => r.ProductionStepId)
                                .ToDictionary(k => k.Key, v => v.Count());
            var lsProductionStepLinkDataOutput = roles.Where(x => stepIds.Contains(x.ProductionStepId))
                .GroupBy(r => r.ProductionStepLinkDataId)
                .Where(g => g.Count() == 1 && g.First().ProductionStepLinkDataRoleTypeId == EnumProductionStepLinkDataRoleType.Output)
                .SelectMany(r => r);

            var item = new GroupProductionStepToOutsource
            {
                Title = title,
                ProdictionStepId = stepIds.ToArray(),
                ProductionStepLinkDataOutput = lsProductionStepLinkDataOutput.GroupBy(r => r.ProductionStepId)
                    .Where(g => g.Count() == calcTotalOutputMap[g.Key])
                    .SelectMany(r => r).Select(x => x.ProductionStepLinkDataId).ToArray(),
                ProductionStepLinkDataOutputInterpolation = lsProductionStepLinkDataOutput.GroupBy(r => r.ProductionStepId)
                    .Where(g => g.Count() < calcTotalOutputMap[g.Key])
                    .SelectMany(r => r).Select(x => x.ProductionStepLinkDataId).ToArray()
            };
            return item;
        }

        private bool TraceProductionStepInsideGroupProductionStepToOutsource(List<ProductionStepLinkDataRoleModel> roles, IList<long> stepIds, ProductionStepLinkDataRoleModel[] roleOutside)
        {
            foreach (var role in roleOutside)
            {
                var roleInput = roles.FirstOrDefault(x => x.ProductionStepLinkDataRoleTypeId == EnumProductionStepLinkDataRoleType.Input && x.ProductionStepLinkDataId == role.ProductionStepLinkDataId);

                if (roleInput == null) continue;

                if (stepIds.Contains(roleInput.ProductionStepId))
                    return true;

                var roleOutput = roles.Where(x => x.ProductionStepId == roleInput.ProductionStepId && x.ProductionStepLinkDataRoleTypeId == EnumProductionStepLinkDataRoleType.Output).ToArray();

                if (TraceProductionStepInsideGroupProductionStepToOutsource(roles, stepIds, roleOutput)) return true;
            }

            return false;
        }

        private static void TraceProductionStepRelationShip(List<IGrouping<long, ProductionStepLinkDataRoleInput>> groupbyLinkDataRole
            , List<IGrouping<long, ProductionStepLinkDataRoleInput>> groupbyLinkDataRoleScanned
            , List<IGrouping<long, ProductionStepLinkDataRoleInput>> groupbyLinkDataRoleOrigin
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

        public async Task<ProductionProcessOutsourceStep> GetProductionProcessOutsourceStep(EnumContainerType containerType, long containerId, long[] productionStepIds)
        {
            var roles = await _manufacturingDBContext.ProductionStepLinkDataRole.AsNoTracking()
                .Include(x => x.ProductionStep)
                .Where(x => x.ProductionStep.ContainerId == containerId && x.ProductionStep.ContainerTypeId == (int)containerType)
                .ProjectTo<ProductionStepLinkDataRoleInput>(_mapper.ConfigurationProvider)
                .ToListAsync();

            var productionSteps = await _manufacturingDBContext.ProductionStep.AsNoTracking()
                .Where(x => x.ContainerId == containerId && x.ContainerTypeId == (int)containerType)
                .ProjectTo<ProductionStepModel>(_mapper.ConfigurationProvider)
                .ToListAsync();


            var productionStepChilds = productionSteps.Where(x => productionStepIds.Contains(x.ProductionStepId)).ToList();
            var productionStepParents = await _manufacturingDBContext.ProductionStep.AsNoTracking()
                .Where(x => x.ContainerId == containerId && x.ContainerTypeId == (int)containerType && productionStepChilds.Select(x=>x.ParentId).Contains(x.ProductionStepId))
                .ProjectTo<ProductionStepModel>(_mapper.ConfigurationProvider)
                .ToListAsync();

            productionStepChilds.AddRange(productionStepParents);

            var lsProductionStepLinkDataId = roles.Where(x=>productionStepIds.Contains(x.ProductionStepId)).Select(x => x.ProductionStepLinkDataId).Distinct().ToList();
            var stepLinkDatas = new List<ProductionStepLinkDataOutsourceStep>();
            if (lsProductionStepLinkDataId.Count > 0)
            {
                var sql = new StringBuilder("Select * from ProductionStepLinkDataExtractInfo v ");
                var parammeters = new List<SqlParameter>();
                var whereCondition = new StringBuilder();

                whereCondition.Append("v.ProductionStepLinkDataTypeId = 0 AND v.ProductionStepLinkDataId IN ( ");
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
                        .ConvertData<ProductionStepLinkDataOutsourceStep>();

                foreach (var ld in stepLinkDatas)
                {
                    var roleInput = roles.FirstOrDefault(x => x.ProductionStepLinkDataId == ld.ProductionStepLinkDataId && x.ProductionStepLinkDataRoleTypeId == EnumProductionStepLinkDataRoleType.Input);
                    var roleOutput = roles.FirstOrDefault(x => x.ProductionStepLinkDataId == ld.ProductionStepLinkDataId && x.ProductionStepLinkDataRoleTypeId == EnumProductionStepLinkDataRoleType.Output);

                    ld.ProductionStepReceiveTitle = "Kho";
                    ld.ProductionStepSourceTitle = "Kho";
                    ld.IsImportant = false;

                    if (roleInput != null)
                    {
                        var step = productionSteps.FirstOrDefault(x => x.ProductionStepId == roleInput.ProductionStepId);
                        var parent = productionSteps.FirstOrDefault(x => x.ProductionStepId == step.ParentId);
                        ld.ProductionStepReceiveTitle = parent.Title;
                        ld.IsImportant = true;
                        ld.ProductionStepReceiveId = parent.ProductionStepId;
                    }

                    if (roleOutput != null)
                    {
                        var step = productionSteps.FirstOrDefault(x => x.ProductionStepId == roleOutput.ProductionStepId);
                        var parent = productionSteps.FirstOrDefault(x => x.ProductionStepId == step.ParentId);

                        ld.ProductionStepSourceTitle = parent.Title;
                        ld.ProductionStepSourceId = parent.ProductionStepId;
                    }
                }
            }

            //Tính toán mối quan hệ giữa (stepLink) các công đoạn
            var productionStepGroupLinkDataRoles = CalcInOutDataForGroup(productionStepChilds, roles);
            var productionStepLinks = CalcProductonStepLink(productionStepGroupLinkDataRoles);
            var rolesChilds = roles.Where(x => productionStepIds.Contains(x.ProductionStepId)).ToList();

            productionStepGroupLinkDataRoles.AddRange(rolesChilds);

            return new ProductionProcessOutsourceStep
            {
                ProductionSteps = productionStepChilds,
                ProductionStepLinkDataRoles = productionStepGroupLinkDataRoles,
                ProductionStepLinkDatas = stepLinkDatas,
                ProductionStepLinks = productionStepLinks,
                ProductionStepLinkDataOutput = roles.Where(x => productionStepIds.Contains(x.ProductionStepId)).GroupBy(r => r.ProductionStepLinkDataId)
                    .Where(g => g.Count() == 1 && g.First().ProductionStepLinkDataRoleTypeId == EnumProductionStepLinkDataRoleType.Output)
                    .SelectMany(r => r)
                    .Select(x => x.ProductionStepLinkDataId)
                    .ToArray(),
                ProductionStepLinkDataIntput = roles.Where(x => productionStepIds.Contains(x.ProductionStepId)).GroupBy(r => r.ProductionStepLinkDataId)
                    .Where(g => g.Count() == 1 && g.First().ProductionStepLinkDataRoleTypeId == EnumProductionStepLinkDataRoleType.Input)
                    .SelectMany(r => r)
                    .Select(x => x.ProductionStepLinkDataId)
                    .ToArray()
            };
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

        private async Task ValidOutsourcePartRequest(long productionOrderId)
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

        }

        public async Task ValidOutsourceStepRequest(long productionOrderId)
        {
            var lsRequest = await _manufacturingDBContext.OutsourceStepRequest.Where(x => x.ProductionOrderId == productionOrderId).ToListAsync();

            var productionStepInfos = await _manufacturingDBContext.ProductionStep.AsNoTracking()
            .Include(s => s.Step)
            .Include(s => s.ProductionStepLinkDataRole)
            .ThenInclude(r => r.ProductionStepLinkData)
            .Where(s => s.ContainerTypeId == (int)EnumContainerType.ProductionOrder && s.ContainerId == productionOrderId)
            .ProjectTo<ProductionStepInfo>(_mapper.ConfigurationProvider)
            .ToListAsync();

            foreach (var rq in lsRequest)
            {
                rq.IsInvalid = false;

                var stepInfoInRequests = productionStepInfos.Where(p => p.OutsourceStepRequestId == rq.OutsourceStepRequestId);
                foreach (var s in stepInfoInRequests)
                {
                    foreach (var l in s.ProductionStepLinkDatas)
                    {
                        if (l.ExportOutsourceQuantity > (l.QuantityOrigin - (l.OutsourcePartQuantity + l.OutsourceQuantity))
                            || l.OutsourceQuantity > (l.QuantityOrigin - l.OutsourcePartQuantity))
                        {
                            rq.IsInvalid = true;
                            break;
                        }
                    }
                }
            }

            await _manufacturingDBContext.SaveChangesAsync();
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

                var lsProductionSemiFinal = await _manufacturingDBContext.ProductSemi.AsNoTracking()
                    .Where(x => x.ContainerId == toContainerId && (int)containerTypeId == x.ContainerTypeId)
                    .ToListAsync();

                process.ProductionSteps.ForEach(x => { x.ProductionStepId = 0; x.ContainerId = toContainerId; });
                process.ProductionStepLinkDatas.ForEach(x =>
                {
                    x.ProductionStepLinkDataId = 0;
                    if (x.ObjectTypeId == EnumProductionStepLinkDataObjectType.ProductSemi)
                    {
                        var p = lsProductSemi.FirstOrDefault(s => s.ProductSemiId == x.ObjectId);
                        if (p == null)
                            throw new BadRequestException(ProductSemiErrorCode.NotFoundProductSemi);
                        var pf = lsProductionSemiFinal.FirstOrDefault(x => x.Title.ToLower().Equals(p.Title.ToLower()));
                        if (pf != null)
                        {
                            x.ObjectId = pf.ProductSemiId;
                        }
                        else
                        {
                            var entityProductSemi = new ProductSemiEnity
                            {
                                ContainerId = toContainerId,
                                ContainerTypeId = p.ContainerTypeId,
                                Title = p.Title,
                                UnitId = p.UnitId,
                                Specification = p.Specification,
                                Note = p.Note
                            };

                            _manufacturingDBContext.ProductSemi.Add(entityProductSemi);
                            _manufacturingDBContext.SaveChanges();

                            x.ObjectId = entityProductSemi.ProductSemiId;
                            lsProductionSemiFinal.Add(entityProductSemi);
                        }

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

                var d1 = await _manufacturingDBContext.ProductionStepRoleClient.AsNoTracking().FirstOrDefaultAsync(x => x.ContainerTypeId == (int)containerTypeId && x.ContainerId == fromContainerId);
                var d2 = await _manufacturingDBContext.ProductionStepRoleClient.FirstOrDefaultAsync(x => x.ContainerTypeId == (int)containerTypeId && x.ContainerId == toContainerId);

                if (d1 != null)
                {
                    if (d2 != null)
                        d2.ClientData = d1.ClientData;
                    else
                    {
                        d1.ContainerId = toContainerId;
                        await _manufacturingDBContext.ProductionStepRoleClient.AddAsync(d1);
                    }
                    await _manufacturingDBContext.SaveChangesAsync();
                }

                // Sync cơ số sản phẩm của SPA->SPB
                if (containerTypeId == EnumContainerType.Product)
                {
                    var p = await _productHelperService.GetProduct((int)fromContainerId);

                    await _productHelperService.UpdateProductCoefficientManual((int)toContainerId, p.Coefficient);
                }

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

        public async Task<IList<ProductionStepSimpleModel>> GetAllProductionStep(EnumContainerType containerTypeId, long containerId)
        {
            var productionSteps = await _manufacturingDBContext.ProductionStep.AsNoTracking()
                .Where(s => s.ContainerId == containerId && s.ContainerTypeId == (int)containerTypeId && s.IsGroup == false && s.IsFinish == false && s.StepId.HasValue)
                .Include(s => s.Step)
                .Include(x => x.ProductionStepLinkDataRole)
                .ThenInclude(r => r.ProductionStepLinkData)
                .ToListAsync();

            var roleOutput = productionSteps.SelectMany(x => x.ProductionStepLinkDataRole)
                .Where(x => x.ProductionStepLinkDataRoleTypeId == (int)EnumProductionStepLinkDataRoleType.Output);

            var productIds = roleOutput
                .Where(x => x.ProductionStepLinkData.ObjectTypeId == (int)EnumProductionStepLinkDataObjectType.Product)
                .Select(x => (int)x.ProductionStepLinkData.ObjectId)
                .Distinct()
                .ToList();
            var productSemiIds = roleOutput
                .Where(x => x.ProductionStepLinkData.ObjectTypeId == (int)EnumProductionStepLinkDataObjectType.ProductSemi)
                .Select(x => x.ProductionStepLinkData.ObjectId)
                .Distinct()
                .ToList();

            var productInfoMap = (await _productHelperService.GetListProducts(productIds)).ToDictionary(k => k.ProductId, v => string.Concat(v.ProductCode, "/ ", v.ProductName));
            var productSemiInfoMap = (await _manufacturingDBContext.ProductSemi.AsNoTracking().Where(x => productSemiIds.Contains(x.ProductSemiId)).ToListAsync())
                .ToDictionary(k => k.ProductSemiId, v => v.Title);


            var data = productionSteps.Select(s =>
            {
                var output = s.ProductionStepLinkDataRole.Where(x => x.ProductionStepLinkDataRoleTypeId == (int)EnumProductionStepLinkDataRoleType.Output)
                .Select(x =>
                {
                    var objectId = x.ProductionStepLinkData.ObjectId;
                    var objectTypeId = x.ProductionStepLinkData.ObjectTypeId;
                    if (objectTypeId == (int)EnumProductionStepLinkDataObjectType.Product)
                        return productInfoMap.ContainsKey((int)objectId) ? productInfoMap[(int)objectId] : "";
                    else return productSemiInfoMap.ContainsKey(objectId) ? productSemiInfoMap[objectId] : "";
                });
                var title = string.IsNullOrEmpty(s.Title) ? s.Step?.StepName : s.Title;
                return new ProductionStepSimpleModel
                {
                    ProductionStepId = s.ProductionStepId,
                    ProductionStepCode = s.ProductionStepCode,
                    Title = title,
                    OutputString = $"{string.Join(", ", output)}",
                    StepId = s.StepId
                };
            }).ToList();

            return data;
        }

        /*
         * Lấy thông tin nhóm đầu ra đầu vào gia công
         */
        public async Task<IList<GroupProductionStepToOutsource>> GroupProductionStepInOutToOutsource(EnumContainerType containerType, long containerId, long[] arrProductionStepId)
        {
            int indexGroup = 1;
            var data = new List<GroupProductionStepToOutsource>();
            var groupRelationship = new NonCamelCaseDictionary();

            var lsProductionStepChildIds = await _manufacturingDBContext.ProductionStep.AsNoTracking()
                .Where(x => arrProductionStepId.Contains(x.ParentId.GetValueOrDefault()))
                .Select(x => x.ProductionStepId).ToListAsync();

            var roles = await _manufacturingDBContext.ProductionStepLinkDataRole.AsNoTracking()
                .Include(x => x.ProductionStep)
                .Where(x => x.ProductionStep.ContainerId == containerId && x.ProductionStep.ContainerTypeId == (int)containerType)
                .ProjectTo<ProductionStepLinkDataRoleInput>(_mapper.ConfigurationProvider)
                .ToListAsync();

            var groupbyLinkDataRole = roles
                .Where(x => lsProductionStepChildIds.Contains(x.ProductionStepId))
                .GroupBy(r => r.ProductionStepLinkDataId)
                .Where(g => g.Count() == 2)
                .ToList();
            /*
             * 1. Lấy ra các công đoạn mà không có cặp linkData InOut
             * 2. Mỗi công đoạn này sẽ tạo 1 nhóm riêng biệt
             */
            var productionStepNotCoupleRole = lsProductionStepChildIds
                                                .Where(value => !groupbyLinkDataRole
                                                                .SelectMany(x => x.Select(y => y.ProductionStepId))
                                                                .Distinct().Contains(value));
            foreach (var productionStepid in productionStepNotCoupleRole)
            {
                var ls = new List<long>();
                ls.Add(productionStepid);
                groupRelationship.Add($"Nhóm chi tiết gia công #{indexGroup}", ls);
                indexGroup++;
            }
            /*
             * Đệ quy để tìm các công đoạn trong cùng 1 nhóm.
             */
            var groupbyLinkDataRoleScanned = new List<IGrouping<long, ProductionStepLinkDataRoleInput>>();
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
                groupRelationship.Add($"Nhóm chi tiết gia công #{indexGroup}", lsProductionStepIdInGroup);
                indexGroup++;
            }

            foreach (var (key, value) in groupRelationship) {
                var stepIds = value as IList<long>;

                var item = GetGroupProductionStepToOutsource(roles, stepIds, key);
                data.Add(item);
            }

            return data;
        }

    }
}
