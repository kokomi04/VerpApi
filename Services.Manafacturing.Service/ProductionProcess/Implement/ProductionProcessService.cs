﻿using AutoMapper;
using AutoMapper.QueryableExtensions;
using DocumentFormat.OpenXml.EMMA;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Ocsp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verp.Resources.Manafacturing.Production.Process;
using Verp.Resources.Master.Config.ActionButton;
using VErp.Commons.Enums.ErrorCodes;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface.Manufacturing;
using VErp.Commons.GlobalObject.InternalDataInterface.Stock;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.ManufacturingDB;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper.Product;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper.QueueHelper;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Manafacturing.Model.ProductionOrder;
using VErp.Services.Manafacturing.Model.ProductionProcess;
using VErp.Services.Manafacturing.Model.ProductionStep;
using VErp.Services.Manafacturing.Service.ProductionAssignment;
using VErp.Services.Manafacturing.Service.ProductionAssignment.Implement;
using static VErp.Commons.Enums.Manafacturing.EnumProductionProcess;
using ProductSemiEnity = VErp.Infrastructure.EF.ManufacturingDB.ProductSemi;

namespace VErp.Services.Manafacturing.Service.ProductionProcess.Implement
{
    public class ProductionProcessService : IProductionProcessService
    {
        private readonly ManufacturingDBContext _manufacturingDBContext;
        private readonly ObjectActivityLogFacade _objActivityLogFacadeStep;
        private readonly ObjectActivityLogFacade _objActivityLogFacadeProcess;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly IProductHelperService _productHelperService;
        private readonly IValidateProductionProcessService _validateProductionProcessService;
        private readonly IProductionAssignmentService _productionAssignmentService;
        protected readonly IProductionOrderQueueHelperService _productionOrderQueueHelperService;

        public ProductionProcessService(ManufacturingDBContext manufacturingDB
            , IActivityLogService activityLogService
            , ILogger<ProductionProcessService> logger
            , IMapper mapper
            , IProductHelperService productHelperService
            , IValidateProductionProcessService validateProductionProcessService
            , IProductionAssignmentService productionAssignmentService
            , IProductionOrderQueueHelperService productionOrderQueueHelperService)
        {
            _manufacturingDBContext = manufacturingDB;
            _objActivityLogFacadeStep = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.ProductionStep);
            _objActivityLogFacadeProcess = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.ProductionProcess);
            _logger = logger;
            _mapper = mapper;
            _productHelperService = productHelperService;
            _validateProductionProcessService = validateProductionProcessService;
            _productionAssignmentService = productionAssignmentService;
            _productionOrderQueueHelperService = productionOrderQueueHelperService;
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

                    await _objActivityLogFacadeStep.LogBuilder(() => ProductionProcessActivityLogMessage.CreateStep)
                            .MessageResourceFormatDatas(req.ProductionStepCode,req.ContainerTypeId.GetEnumDescription(),req.ContainerId)
                            .ObjectId(step.ProductionStepId)
                            .JsonData(req)
                            .CreateLog();
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

                    await _objActivityLogFacadeStep.LogBuilder(() => ActionButtonActivityLogMessage.Delete)
                            .MessageResourceFormatDatas(productionStep.ProductionStepCode,((EnumContainerType)productionStep.ContainerTypeId).GetEnumDescription(),productionStep.ContainerId)
                            .ObjectId(productionStep.ProductionStepId)
                            .JsonData(productionStep)
                            .CreateLog();
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
                //ProductionStepLinkDataGroup = d.ProductionStepLinkDataGroup,
                WorkloadConvertRate = d.WorkloadConvertRate
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
                        LinkDataObjectId = dataLinks[r.ProductionStepLinkDataId].LinkDataObjectId,
                        LinkDataObjectTypeId = dataLinks[r.ProductionStepLinkDataId].LinkDataObjectTypeId,
                        Quantity = dataLinks[r.ProductionStepLinkDataId].Quantity,
                        OutsourceQuantity = dataLinks[r.ProductionStepLinkDataId].OutsourceQuantity,
                        SortOrder = dataLinks[r.ProductionStepLinkDataId].SortOrder,
                        ProductionStepId = r.ProductionStepId,
                        ProductionStepLinkDataRoleTypeId = r.ProductionStepLinkDataRoleTypeId,
                        WorkloadConvertRate = r.WorkloadConvertRate
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

            foreach (var group in sortGroups)
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


        public async Task<IList<ProductionProcessModel>> GetProductionProcessByContainerIds(EnumContainerType containerTypeId, IList<long> containerIds)
        {

            var infos = await _manufacturingDBContext.ProductionContainer.Where(c => c.ContainerTypeId == (int)containerTypeId
            && containerIds.Contains(c.ContainerId)
            ).ToListAsync();

            var productionSteps = await _manufacturingDBContext.ProductionStep.AsNoTracking()
                .Where(s => containerIds.Contains(s.ContainerId) && s.ContainerTypeId == (int)containerTypeId)
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
                //ProductionStepLinkDataGroup = d.ProductionStepLinkDataGroup
            }).ToList();

            //Lấy thông tin dữ liệu của steplinkdata
            var lsProductionStepLinkDataId = roles.Select(x => x.ProductionStepLinkDataId).Distinct().ToList();
            IList<ProductionStepLinkDataInput> stepLinkDatas = new List<ProductionStepLinkDataInput>();
            if (lsProductionStepLinkDataId.Count > 0)
            {
                var sql = new StringBuilder(@$"
                    SELECT * FROM dbo.ProductionStepLinkDataExtractInfo v 
                    WHERE v.ProductionStepLinkDataId IN (SELECT [Value] FROM @ProductionStepLinkDataIds)
                ");
                var parammeters = new List<SqlParameter>()
                {
                    lsProductionStepLinkDataId.ToSqlParameter("@ProductionStepLinkDataIds"),
                };

                stepLinkDatas = await _manufacturingDBContext.QueryListRaw<ProductionStepLinkDataInput>(sql.ToString(), parammeters);
            }

            var productionOutsourcePartMappings = await _manufacturingDBContext.ProductionOutsourcePartMapping.Where(x => containerIds.Contains(x.ContainerId))
            .ProjectTo<ProductionOutsourcePartMappingInput>(_mapper.ConfigurationProvider)
            .ToListAsync();

            foreach (var item in productionOutsourcePartMappings)
            {
                var linkDataCodes = stepLinkDatas.Where(x => x.ProductionOutsourcePartMappingId == item.ProductionOutsourcePartMappingId)
                                            .Select(x => x.ProductionStepLinkDataCode)
                                            .ToList();
                if (linkDataCodes != null)
                    item.ProductionStepLinkDataCodes.AddRange(linkDataCodes);
            }

            // Tính toán quan hệ của quy trình con
            //var productionStepGroupLinkDataRoles = CalcInOutDataForGroup(stepInfos, roles);

            //Tính toán mối quan hệ giữa (stepLink) các công đoạn
            var productionStepLinks = CalcProductionStepLink(roles);

            return containerIds.Select(id =>
            {
                var steps = stepInfos.Where(s => s.ContainerId == id).ToList();
                var productionStepIds = steps.Select(s => s.ProductionStepId);
                var roleData = roles.Where(r => productionStepIds.Contains(r.ProductionStepId)).ToList();
                var linkDataIds = roleData.Select(r => r.ProductionStepLinkDataId);
                var stepLinks = productionStepLinks.Where(s => productionStepIds.Contains(s.FromStepId) || productionStepIds.Contains(s.ToStepId)).ToList();

                return new ProductionProcessModel
                {
                    ContainerId = id,
                    ContainerTypeId = containerTypeId,
                    ProductionSteps = steps.OrderBy(s => s.SortOrder).ToList(),
                    ProductionStepLinkDataRoles = roleData,
                    ProductionStepLinkDatas = stepLinkDatas.Where(l => linkDataIds.Contains(l.ProductionStepLinkDataId)).ToList(),
                    ProductionStepLinks = stepLinks,
                    ProductionOutsourcePartMappings = productionOutsourcePartMappings.Where(o => o.ContainerId == id).ToList(),
                    UpdatedDatetimeUtc = (infos.FirstOrDefault(c => c.ContainerId == id)?.UpdatedDatetimeUtc)?.GetUnix()
                    //ProductionStepGroupLinkDataRoles = productionStepGroupLinkDataRoles,
                };

            }).ToList();
        }

        public async Task<ProductionProcessModel> GetProductionProcessByContainerId(EnumContainerType containerTypeId, long containerId)
        {
            return (await GetProductionProcessByContainerIds(containerTypeId, new[] { containerId })).FirstOrDefault();
            /*
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
                //ProductionStepLinkDataGroup = d.ProductionStepLinkDataGroup
            }).ToList();

            //Lấy thông tin dữ liệu của steplinkdata
            var lsProductionStepLinkDataId = roles.Select(x => x.ProductionStepLinkDataId).Distinct().ToList();
            IList<ProductionStepLinkDataInput> stepLinkDatas = new List<ProductionStepLinkDataInput>();
            if (lsProductionStepLinkDataId.Count > 0)
            {
                var sql = new StringBuilder(@$"
                    SELECT * FROM dbo.ProductionStepLinkDataExtractInfo v 
                    WHERE v.ProductionStepLinkDataId IN (SELECT [Value] FROM @ProductionStepLinkDataIds)
                ");
                var parammeters = new List<SqlParameter>()
                {
                    lsProductionStepLinkDataId.ToSqlParameter("@ProductionStepLinkDataIds"),
                };

                stepLinkDatas = await _manufacturingDBContext.QueryList<ProductionStepLinkDataInput>(sql.ToString(), parammeters);
            }

            var productionOutsourcePartMappings = await _manufacturingDBContext.ProductionOutsourcePartMapping.Where(x => x.ContainerId == containerId)
            .ProjectTo<ProductionOutsourcePartMappingInput>(_mapper.ConfigurationProvider)
            .ToListAsync();

            foreach (var item in productionOutsourcePartMappings)
            {
                var linkDataCodes = stepLinkDatas.Where(x => x.ProductionOutsourcePartMappingId == item.ProductionOutsourcePartMappingId)
                                            .Select(x => x.ProductionStepLinkDataCode)
                                            .ToList();
                if (linkDataCodes != null)
                    item.ProductionStepLinkDataCodes.AddRange(linkDataCodes);
            }

            // Tính toán quan hệ của quy trình con
            //var productionStepGroupLinkDataRoles = CalcInOutDataForGroup(stepInfos, roles);

            //Tính toán mối quan hệ giữa (stepLink) các công đoạn
            var productionStepLinks = CalcProductionStepLink(roles);

            return new ProductionProcessModel
            {
                ContainerId = containerId,
                ContainerTypeId = containerTypeId,
                ProductionSteps = stepInfos,
                ProductionStepLinkDataRoles = roles,
                ProductionStepLinkDatas = stepLinkDatas.ToList(),
                ProductionStepLinks = productionStepLinks,
                ProductionOutsourcePartMappings = productionOutsourcePartMappings
                //ProductionStepGroupLinkDataRoles = productionStepGroupLinkDataRoles,
            };*/
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
                        //ProductionStepLinkDataGroup = r.ProductionStepLinkDataGroup,
                        WorkloadConvertRate = r.WorkloadConvertRate
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

            return CalcProductionStepLink(roleUnions);
        }

        private static List<ProductionStepLinkModel> CalcProductionStepLink(List<ProductionStepLinkDataRoleInput> roles)
        {
            var roleGroups = roles.GroupBy(r => r.ProductionStepLinkDataId);
            var productionStepLinks = new List<ProductionStepLinkModel>();

            foreach (var roleGroup in roleGroups)
            {
                var froms = roleGroup.Where(r => r.ProductionStepLinkDataRoleTypeId == EnumProductionStepLinkDataRoleType.Output).ToList();
                var tos = roleGroup.Where(r => r.ProductionStepLinkDataRoleTypeId == EnumProductionStepLinkDataRoleType.Input).ToList();
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
                    && ld.LinkDataObjectTypeId == (int)EnumProductionStepLinkDataObjectType.Product
                    && productIds.Contains(ld.LinkDataObjectId)
                    && r.ProductionStepLinkDataRoleTypeId == (int)EnumProductionStepLinkDataRoleType.Output
                    select ld.LinkDataObjectId
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
                //productionOrder.ProductionOrderStatus = (int)EnumProductionStatus.Waiting;

                var bottomStep = _manufacturingDBContext.ProductionStep
                    .Where(ps => ps.ContainerId == productionOrderId && ps.ContainerTypeId == (int)EnumContainerType.ProductionOrder)
                    .OrderByDescending(ps => ps.CoordinateY)
                    .FirstOrDefault();

                var maxY = bottomStep?.CoordinateY.GetValueOrDefault() ?? 0;
                var newMaxY = maxY;
                foreach (var productionOrderDetail in productionOrderDetails.GroupBy(x => x.ProductId))
                {
                    // Tạo step ứng với quy trình sản xuất
                    var product = products.First(p => p.ProductId == productionOrderDetail.Key);
                    var totalQuantity = productionOrderDetail.Sum(x => x.Quantity + x.ReserveQuantity);

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
                            IsFinish = step.IsFinish,
                            Comment =step.Comment
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
                            LinkDataObjectId = item.LinkDataObjectId,
                            LinkDataObjectTypeId = item.LinkDataObjectTypeId,
                            Quantity = item.Quantity * totalQuantity / product.Coefficient,
                            QuantityOrigin = item.QuantityOrigin * totalQuantity / product.Coefficient,
                            SortOrder = item.SortOrder,
                            ProductionStepLinkDataCode = Guid.NewGuid().ToString(),
                            ProductionStepLinkTypeId = item.ProductionStepLinkTypeId,
                            ProductionStepLinkDataTypeId = item.ProductionStepLinkDataTypeId,
                            //ConverterId = item.ConverterId,
                            //ExportOutsourceQuantity = item.ExportOutsourceQuantity,
                            //OutsourcePartQuantity = item.OutsourcePartQuantity,
                            //OutsourceQuantity = item.OutsourceQuantity,
                            //OutsourceRequestDetailId = item.OutsourceRequestDetailId,
                            //ProductionOutsourcePartMappingId = item.ProductionOutsourcePartMappingId,
                            WorkloadConvertRate = item.WorkloadConvertRate
                        };

                        _manufacturingDBContext.ProductionStepLinkData.Add(newLinkData);
                        linkDataMap.Add(item.ProductionStepLinkDataId, newLinkData);
                    }

                    //Gán version mới nhất của MH vào chi tiết MH trong LSX
                    productionOrderDetail.ToList().ForEach(x => x.ProductionProcessVersion = product.ProductionProcessVersion);

                    _manufacturingDBContext.SaveChanges();

                    // Create role
                    foreach (var role in roles)
                    {
                        var newRole = new ProductionStepLinkDataRole
                        {
                            ProductionStepLinkDataId = linkDataMap[role.ProductionStepLinkDataId].ProductionStepLinkDataId,
                            ProductionStepId = stepMap[role.ProductionStepId].ProductionStepId,
                            ProductionStepLinkDataRoleTypeId = role.ProductionStepLinkDataRoleTypeId,
                            //ProductionStepLinkDataGroup = role.ProductionStepLinkDataGroup
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
                         .ToListAsync())
                         .SelectMany(x => (x.ClientData.JsonDeserialize<IList<RoleClientData>>()))
                         .ToList();

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

                var info = await _manufacturingDBContext.ProductionContainer.FirstOrDefaultAsync(c => c.ContainerTypeId == (int)EnumContainerType.ProductionOrder && c.ContainerId == productionOrderId);
                if (info == null)
                {
                    _manufacturingDBContext.ProductionContainer.Add(new ProductionContainer()
                    {
                        ContainerId = productionOrderId,
                        ContainerTypeId = (int)EnumContainerType.ProductionOrder
                    });
                }
                else
                {
                    info.UpdatedDatetimeUtc = DateTime.UtcNow;
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
                linkDatas[r.ProductionStepLinkDataId].LinkDataObjectId,
                linkDatas[r.ProductionStepLinkDataId].LinkDataObjectTypeId,
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
                    && group[stepId].All(r => group[firstStepId].Any(p => p.LinkDataObjectId == r.LinkDataObjectId && p.LinkDataObjectTypeId == r.LinkDataObjectTypeId && p.ProductionStepLinkDataRoleTypeId == r.ProductionStepLinkDataRoleTypeId));
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

                    await _objActivityLogFacadeStep.LogBuilder(() => ProductionProcessActivityLogMessage.UpdateDetail)
                            .MessageResourceFormatDatas(sProductionStep.ProductionStepCode,((EnumProductionProcess.EnumContainerType)sProductionStep.ContainerTypeId).GetEnumDescription(),sProductionStep.ContainerId)
                            .ObjectId(sProductionStep.ProductionStepId)
                            .JsonData(req)
                            .CreateLog();
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
                var s = source.FirstOrDefault(x => x.LinkDataObjectId == p.LinkDataObjectId && (int)x.LinkDataObjectTypeId == p.LinkDataObjectTypeId);
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

            await _objActivityLogFacadeStep.LogBuilder(() => ProductionProcessActivityLogMessage.Create)
                            .MessageResourceFormatDatas(req.ProductionStepCode,req.ContainerTypeId.GetEnumDescription(),req.ContainerId)
                            .ObjectId(stepGroup.ProductionStepId)
                            .JsonData(req)
                            .CreateLog();
            return stepGroup.ProductionStepId;
        }

        public async Task<bool> UpdateProductionStepSortOrder(IList<ProductionStepSortOrderModel> req)
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

                await _objActivityLogFacadeStep.LogBuilder(() => ProductionProcessActivityLogMessage.UpdateStep)
                           .ObjectId(req.First().ProductionStepId)
                           .JsonData(req)
                           .CreateLog();
                return true;
            }
            catch (Exception ex)
            {
                await trans.RollbackAsync();
                _logger.LogError(ex, "UpdateProductionStepSortOrder");
                throw;
            }
        }


        public async Task<bool> UpdateProductionProcess(EnumContainerType containerTypeId, long containerId, ProductionProcessModel req)
        {
            var trans = await _manufacturingDBContext.Database.BeginTransactionAsync();
            try
            {
                if (containerTypeId == EnumContainerType.Product)
                {
                    await _productHelperService.UpdateProductionProcessStatus(new InternalProductProcessStatus()
                    {
                        ProductId = containerId,
                        ProcessStatus = EnumProductionProcessStatus.CreateButNotYet
                    }, false);
                }
               
                var info = await _manufacturingDBContext.ProductionContainer.FirstOrDefaultAsync(c => c.ContainerTypeId == (int)containerTypeId && c.ContainerId == containerId);
                if (info == null)
                {
                    _manufacturingDBContext.ProductionContainer.Add(new ProductionContainer()
                    {
                        ContainerId = containerId,
                        ContainerTypeId = (int)containerTypeId
                    });
                }
                else
                {

                    if (req.UpdatedDatetimeUtc != info.UpdatedDatetimeUtc.GetUnix())
                    {
                        throw GeneralCode.DataIsOld.BadRequest();
                    }

                    info.UpdatedDatetimeUtc = DateTime.UtcNow;
                }

                if (containerTypeId == EnumContainerType.Product)
                {
                    var product = await _productHelperService.GetProduct((int)containerId);

                    var arrOutputProductionStepLinkDataCode = req.ProductionStepLinkDataRoles.GroupBy(x => x.ProductionStepLinkDataCode)
                                                                                           .Where(x => x.Count() == 1)
                                                                                           .SelectMany(x => x)
                                                                                           .Where(x => x.ProductionStepLinkDataRoleTypeId == EnumProductionStepLinkDataRoleType.Output)
                                                                                           .Select(x => x.ProductionStepLinkDataCode);
                    var finalProductionLinkData = req.ProductionStepLinkDatas.Where(x => arrOutputProductionStepLinkDataCode.Contains(x.ProductionStepLinkDataCode) && x.LinkDataObjectId == product.ProductId && x.LinkDataObjectTypeId == EnumProductionStepLinkDataObjectType.Product)
                                                                             .FirstOrDefault();
                    if (finalProductionLinkData != null && product.Coefficient != finalProductionLinkData.QuantityOrigin)
                    {
                        throw new BadRequestException(ProductionProcessErrorCode.ValidateProductionStepLinkData, "Số lượng đầu ra của mặt hàng trong quy trình không bằng sơ số sản phẩm");
                    }
                }

                await UpdateProductionProcessManual(containerTypeId, containerId, req, false);

                if (containerTypeId == EnumContainerType.Product)
                    await _productHelperService.UpdateProductionProcessVersion(containerId);

                await trans.CommitAsync();

                await _objActivityLogFacadeProcess.LogBuilder(() => ProductionProcessActivityLogMessage.UpdateProcess)
                           .ObjectId(req.ContainerId)
                           .JsonData(req)
                           .CreateLog();

                if(containerTypeId== EnumContainerType.ProductionOrder)
                {
                    var productionOrderInfo = await _manufacturingDBContext.ProductionOrder.FirstOrDefaultAsync(p => p.ProductionOrderId == containerId);
                    await _productionOrderQueueHelperService.ProductionOrderStatiticChanges(productionOrderInfo?.ProductionOrderCode, $"Cập nhật quy trình sản xuất");
                }
                return true;
            }
            catch (Exception ex)
            {
                await trans.RollbackAsync();
                _logger.LogError(ex, "UpdateProductionProcess");
                throw;
            }
        }

        public async Task<bool> DismissUpdateQuantity(long productionOrderId)
        {
            try
            {
                // Cập nhật lại trạng thái thay đổi số lượng LSX
                var productionOrder = _manufacturingDBContext.ProductionOrder.FirstOrDefault(po => po.ProductionOrderId == productionOrderId);
                if (productionOrder == null) throw new BadRequestException(GeneralCode.InvalidParams, "Lệnh sản xuất không tồn tại");
                if (productionOrder.IsUpdateQuantity == true)
                {
                    productionOrder.IsUpdateQuantity = false;
                }
                await _manufacturingDBContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DismissUpdateQuantity");
                throw;
            }
        }

        public async Task<bool> CheckHasAssignment(long productionOrderId)
        {
            return await _manufacturingDBContext.ProductionAssignment.AnyAsync(a => a.ProductionOrderId == productionOrderId);
        }

        
        private async Task UpdateProductionProcessManual(EnumContainerType containerTypeId, long containerId, ProductionProcessModel req, bool isFromCopy)
        {
            foreach (var s in req.ProductionSteps)
            {
                s.ContainerTypeId = containerTypeId;
                s.ContainerId = containerId;
            }

            var toRemoveLinkDatas = new List<ProductionStepLinkDataInput>();
            foreach (var d in req.ProductionStepLinkDatas)
            {
                if (!req.ProductionStepLinkDataRoles.Any(r => r.ProductionStepLinkDataId == d.ProductionStepLinkDataId || string.Compare(r.ProductionStepLinkDataCode, d.ProductionStepLinkDataCode, true) == 0))
                {
                    toRemoveLinkDatas.Add(d);
                }
                else
                {
                    var remaingQuantity = d.QuantityOrigin - d.OutsourceQuantity - (d.OutsourcePartQuantity ?? 0);// - d.ExportOutsourceQuantity;
                    if (d.Quantity.SubProductionDecimal(remaingQuantity) != 0 && !isFromCopy)
                    {
                        throw GeneralCode.InvalidParams.BadRequest("Lỗi xử lý quy trình sản xuất, Số lượng sản xuất phải bằng số lượng ban đầu trừ các số lượng đi gia công!");
                    }

                    if (d.Quantity.SubProductionDecimal(d.ExportOutsourceQuantity) < 0 && !isFromCopy)
                    {
                        throw GeneralCode.InvalidParams.BadRequest("Lỗi xử lý quy trình sản xuất, Số lượng sản xuất phải lớn hơn số lượng xuất đi gia công!");
                    }
                }
            }
            foreach (var d in toRemoveLinkDatas)
            {
                req.ProductionStepLinkDatas.Remove(d);
            }

            var productionStepGroups = req.ProductionSteps.Where(x => x.IsGroup == true && !x.IsFinish).ToList();
            var productionStepsInGroup = req.ProductionSteps.Where(x => x.IsGroup != true && !x.IsFinish).ToList();

            productionStepGroups.Where(x => x.IsGroup == true).ToList().ForEach(x =>
            {
                if (!productionStepsInGroup.Any(t => t.ParentCode == x.ProductionStepCode) && !isFromCopy)
                    throw new BadRequestException(ProductionProcessErrorCode.ValidateProductionStep, $"Công đoạn \"{x.Title}\" chưa được thiết lập chi tiết công đoạn");
            });

            var groupRolesByStepCode = req.ProductionStepLinkDataRoles.GroupBy(r => r.ProductionStepCode).ToDictionary(r=>r.Key,r=>r.ToList());

            if (!isFromCopy)
            {
                foreach (var p in productionStepsInGroup)
                {
                    var step = productionStepGroups.FirstOrDefault(x => x.ProductionStepCode == p.ParentCode);
                    if (step == null)
                    {
                        throw $"Công đoạn cha của công đoạn {p.Title} không tồn tại".BadRequest();
                    }

                    if (!groupRolesByStepCode.ContainsKey(p.ProductionStepCode))
                    {
                        throw new BadRequestException(ProductionProcessErrorCode.ValidateProductionStep, $"Công đoạn \"{p.Title}\" trong nhóm công đoạn \"{step.Title}\" không có đầu ra đầu vào");
                    }
                    if (groupRolesByStepCode.ContainsKey(p.ProductionStepCode))
                    {
                        if (!groupRolesByStepCode[p.ProductionStepCode].Any(r => r.ProductionStepLinkDataRoleTypeId == EnumProductionStepLinkDataRoleType.Input))
                        {
                            throw new BadRequestException(ProductionProcessErrorCode.ValidateProductionStep, $"Công đoạn \"{p.Title}\" trong nhóm công đoạn \"{step.Title}\" không có đầu vào");
                        }

                        if (!groupRolesByStepCode[p.ProductionStepCode].Any(r => r.ProductionStepLinkDataRoleTypeId == EnumProductionStepLinkDataRoleType.Output))
                        {
                            throw new BadRequestException(ProductionProcessErrorCode.ValidateProductionStep, $"Công đoạn \"{p.Title}\" trong nhóm công đoạn \"{step.Title}\" không có đầu ra");
                        }
                    }

                }
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


                foreach(var pStep in req.ProductionSteps)
                {
                    var outs = req.ProductionStepLinkDataRoles.Where(r =>
                    r.ProductionStepLinkDataRoleTypeId == EnumProductionStepLinkDataRoleType.Output
                    && r.ProductionStepCode == pStep.ProductionStepCode
                    ).Select(o=> new
                    {
                        o.ProductionStepLinkDataCode,
                        LinkData=req.ProductionStepLinkDatas.FirstOrDefault(d=>d.ProductionStepLinkDataCode==o.ProductionStepLinkDataCode),
                        ToProductionStepCode = req.ProductionStepLinkDataRoles
                        .FirstOrDefault(r=> r.ProductionStepLinkDataRoleTypeId== EnumProductionStepLinkDataRoleType.Input 
                                && r.ProductionStepLinkDataCode== o.ProductionStepLinkDataCode
                                )?.ProductionStepCode
                    }).ToList();


                    var duplicateLink = outs.GroupBy(o => new
                    {
                        o.LinkData.LinkDataObjectTypeId,
                        o.LinkData.LinkDataObjectId,
                        o.ToProductionStepCode
                    }).FirstOrDefault(o => o.Count() > 1);

                    if(duplicateLink != null)
                    {
                        var toProductionStep = req.ProductionSteps.FirstOrDefault(d => d.ProductionStepCode == duplicateLink.Key.ToProductionStepCode);

                        var toProductionStepTitle = "Kho";
                        if (toProductionStep != null)
                            toProductionStepTitle = toProductionStep.Title;

                        throw new BadRequestException(ProductionProcessErrorCode.ValidateProductionStepLinkData, $"Bàn giao giữa {pStep.Title} và {toProductionStepTitle} có mặt hàng trùng nhau");
                    }

                }

                if (req.ProductionOutsourcePartMappings == null)
                {
                    req.ProductionOutsourcePartMappings = new List<ProductionOutsourcePartMappingInput>();
                }
            }

            if (containerTypeId == EnumContainerType.ProductionOrder)
            {
                var sourceOutsourcePartMappings = await _manufacturingDBContext.ProductionOutsourcePartMapping.Where(x => x.ContainerId == containerId)
                                                                                                              .ToListAsync();

                foreach (var dest in sourceOutsourcePartMappings)
                {
                    var source = req.ProductionOutsourcePartMappings.FirstOrDefault(x => x.ProductionOutsourcePartMappingId == dest.ProductionOutsourcePartMappingId);
                    if (source != null)
                    {
                        _mapper.Map(source, dest);
                    }
                    else
                    {
                        dest.IsDeleted = true;
                    }

                }

                foreach (var item in req.ProductionOutsourcePartMappings)
                {
                    if (item.ProductionOutsourcePartMappingId <= 0)
                    {
                        var entity = _mapper.Map<ProductionOutsourcePartMapping>(item);
                        await _manufacturingDBContext.ProductionOutsourcePartMapping.AddAsync(entity);
                        await _manufacturingDBContext.SaveChangesAsync();

                        item.ProductionOutsourcePartMappingId = entity.ProductionOutsourcePartMappingId;
                    }

                }
            }

            //Cập nhật, xóa và tạo mới steplinkdata
            var lsStepLinkDataId = (from s in _manufacturingDBContext.ProductionStep
                                    join r in _manufacturingDBContext.ProductionStepLinkDataRole on s.ProductionStepId equals r.ProductionStepId
                                    where s.ContainerId == containerId && s.ContainerTypeId == (int)containerTypeId
                                    select r.ProductionStepLinkDataId).Distinct();
            var sourceStepLinkData = await _manufacturingDBContext.ProductionStepLinkData.Where(p => lsStepLinkDataId.Contains(p.ProductionStepLinkDataId)).ToListAsync();

            foreach (var item in req.ProductionOutsourcePartMappings)
            {
                var linkDatas = req.ProductionStepLinkDatas.Where(x => item.ProductionStepLinkDataCodes.Contains(x.ProductionStepLinkDataCode));

                foreach (var ld in linkDatas)
                {
                    ld.ProductionOutsourcePartMappingId = item.ProductionOutsourcePartMappingId;
                }
            }

            foreach (var dest in sourceStepLinkData)
            {
                var source = req.ProductionStepLinkDatas.FirstOrDefault(x => x.ProductionStepLinkDataId == dest.ProductionStepLinkDataId);
                if (source != null)
                {
                    _mapper.Map(source, dest);
                }
                else
                {
                    if (!isFromCopy && containerTypeId == EnumContainerType.ProductionOrder && (dest.OutsourceQuantity.GetValueOrDefault() > 0 || dest.ExportOutsourceQuantity.GetValueOrDefault() > 0))
                    {
                        throw new BadRequestException(ProductionProcessErrorCode.ValidateProductionStepLinkData,
                            $"Cần xóa liên kết gia công trong quy trình trước khi xóa dữ liệu của công đoạn liên quan");
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

                    if (!isFromCopy && containerTypeId == EnumContainerType.ProductionOrder && dest.OutsourceStepRequestId.GetValueOrDefault() > 0)
                        throw new BadRequestException(ProductionProcessErrorCode.ValidateProductionStep, $"Không thể xóa công đoạn có liên quan đến YCGC công đoạn. Cần xóa gia công công đoạn trước");

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
                        join s in newStep on r.ProductionStepCode.ToUpper() equals s.ProductionStepCode
                        join d in newStepLinkData on r.ProductionStepLinkDataCode.ToUpper() equals d.ProductionStepLinkDataCode
                        select new ProductionStepLinkDataRole
                        {
                            ProductionStepId = s.ProductionStepId,
                            ProductionStepLinkDataId = d.ProductionStepLinkDataId,
                            ProductionStepLinkDataRoleTypeId = (int)r.ProductionStepLinkDataRoleTypeId,
                            //ProductionStepLinkDataGroup = r.ProductionStepLinkDataGroup
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

            // Xóa thông tin phân công, bàn giao, khai báo nhân công khi thay đổi quy trình
            if (containerTypeId == EnumContainerType.ProductionOrder)
            {
                // Cập nhật lại trạng thái thay đổi số lượng LSX
                var productionOrder = _manufacturingDBContext.ProductionOrder.FirstOrDefault(po => po.ProductionOrderId == containerId);
                if (productionOrder == null) throw new BadRequestException(GeneralCode.InvalidParams, "Lệnh sản xuất không tồn tại");
                if (productionOrder.IsUpdateQuantity == true)
                {
                    productionOrder.IsUpdateQuantity = false;
                }

                // Kiểm tra nếu quy trình đã thực hiện phân công
                // Cập nhật trạng thái thay đổi quy trình LSX cho phân công
                if (await CheckHasAssignment(productionOrder.ProductionOrderId))
                {
                    productionOrder.IsUpdateProcessForAssignment = true;
                }

                // Lấy danh sách công đoạn hiện tại của lệnh
                var currentProductionStepIds = _manufacturingDBContext.ProductionStep
                    .Where(ps => ps.ContainerId == containerId && ps.ContainerTypeId == (int)containerTypeId)
                    .Select(ps => ps.ProductionStepId)
                    .ToList();


                lsStepLinkDataId = (from s in _manufacturingDBContext.ProductionStep
                                    join r in _manufacturingDBContext.ProductionStepLinkDataRole on s.ProductionStepId equals r.ProductionStepId
                                    where s.ContainerId == containerId && s.ContainerTypeId == (int)containerTypeId
                                    select r.ProductionStepLinkDataId).Distinct();

                // Xóa phân công cho các công đoạn bị xóa khỏi quy trình
                var deletedProductionStepAssignments = _manufacturingDBContext.ProductionAssignment
                    .Include(a => a.ProductionAssignmentDetail)
                    .Where(s => s.ProductionOrderId == containerId && (!currentProductionStepIds.Contains(s.ProductionStepId) || !lsStepLinkDataId.Contains(s.ProductionStepLinkDataId)))
                    .ToList();

                await _productionAssignmentService.DeleteAssignmentRef(containerId, deletedProductionStepAssignments);

                await _manufacturingDBContext.SaveChangesAsync();
            }
            if (containerTypeId == EnumContainerType.Product)
            {
                var productProcessModel = await GetProductionProcessByContainerId(containerTypeId, containerId);
                if ((await _validateProductionProcessService.ValidateProductionProcess(containerTypeId, containerId, productProcessModel)).Count() == 0)
                {
                    await _productHelperService.UpdateProductionProcessStatus(new InternalProductProcessStatus()
                    {
                        ProductId = containerId,
                        ProcessStatus = EnumProductionProcessStatus.Created
                    }, true);
                }
                else await _productHelperService.UpdateProductionProcessStatus(new InternalProductProcessStatus()
                {
                    ProductId = containerId,
                    ProcessStatus = EnumProductionProcessStatus.CreateButNotYet
                }, true);
            }                       

        }

        private async Task UpdateStatusValidForProductionOrder(EnumContainerType containerTypeId, long containerId, ProductionProcessModel process)
        {
            var productionOrder = await _manufacturingDBContext.ProductionOrder.FirstOrDefaultAsync(x => x.ProductionOrderId == containerId);
            productionOrder.IsResetProductionProcess = true;
            productionOrder.IsInvalid = (await _validateProductionProcessService.ValidateProductionProcess(containerTypeId, containerId, process)).Count() > 0;

            await _manufacturingDBContext.SaveChangesAsync();
        }

        public async Task UpdateProductionOrderProcessStatus(long productionOrderId)
        {
            var processModel = await GetProductionProcessByContainerId(EnumContainerType.ProductionOrder, productionOrderId);

            var productionOrder = await _manufacturingDBContext.ProductionOrder.FirstOrDefaultAsync(x => x.ProductionOrderId == productionOrderId);
            if (productionOrder != null)
            {
                productionOrder.IsInvalid = (await _validateProductionProcessService.ValidateProductionProcess(EnumContainerType.ProductionOrder, productionOrderId, processModel)).Count() > 0;

                await _manufacturingDBContext.SaveChangesAsync();
            }
        }

        public async Task<IList<ProductionStepLinkDataInput>> GetProductionStepLinkDataByListId(List<long> lsProductionStepLinkDataId)
        {
            IList<ProductionStepLinkDataInput> stepLinkDatas = new List<ProductionStepLinkDataInput>();
            if (lsProductionStepLinkDataId.Count > 0)
            {
                var sql = new StringBuilder(@$"
                    SELECT * FROM dbo.ProductionStepLinkDataExtractInfo v 
                    WHERE v.ProductionStepLinkDataId IN (SELECT [Value] FROM @ProductionStepLinkDataIds)
                ");
                var parammeters = new List<SqlParameter>()
                {
                    lsProductionStepLinkDataId.ToSqlParameter("@ProductionStepLinkDataIds"),
                };

                stepLinkDatas = await _manufacturingDBContext.QueryListRaw<ProductionStepLinkDataInput>(sql.ToString(), parammeters);
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
            var productionStepParents = productionSteps.Where(x => productionStepChilds.Select(x => x.ParentId).Contains(x.ProductionStepId)).ToList();

            var groupInOutToOutsource = await GroupProductionStepInOutToOutsource(containerType, containerId, productionStepParents.Select(x => x.ProductionStepId).Distinct().ToArray(), Ignore: true);

            productionStepChilds.AddRange(productionStepParents);

            var lsProductionStepLinkDataId = roles.Where(x => productionStepIds.Contains(x.ProductionStepId)).Select(x => x.ProductionStepLinkDataId).Distinct().ToList();

            IList<ProductionStepLinkDataOutsourceStep> stepLinkDatas = new List<ProductionStepLinkDataOutsourceStep>();
            if (lsProductionStepLinkDataId.Count > 0)
            {
                var sql = new StringBuilder(@$"
                    SELECT * FROM dbo.ProductionStepLinkDataExtractInfo v 
                    WHERE v.ProductionStepLinkDataId IN (SELECT [Value] FROM @ProductionStepLinkDataIds)
                        AND v.OutsourceRequestDetailId IS NULL
                ");
                var parammeters = new List<SqlParameter>()
                {
                    lsProductionStepLinkDataId.ToSqlParameter("@ProductionStepLinkDataIds"),
                };

                stepLinkDatas = await _manufacturingDBContext.QueryListRaw<ProductionStepLinkDataOutsourceStep>(sql.ToString(), parammeters);

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
            var productionStepLinks = CalcProductionStepLink(productionStepGroupLinkDataRoles);
            var rolesChilds = roles.Where(x => productionStepIds.Contains(x.ProductionStepId)).ToList();

            productionStepGroupLinkDataRoles.AddRange(rolesChilds);

            return new ProductionProcessOutsourceStep
            {
                ProductionSteps = productionStepChilds,
                ProductionStepLinkDataRoles = rolesChilds,
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
                    .ToArray(),
                groupProductionStepToOutsources = groupInOutToOutsource
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
                            || l.OutsourceQuantity > (l.QuantityOrigin - l.OutsourcePartQuantity) || l.LinkDataObjectTypeId == EnumProductionStepLinkDataObjectType.ProductSemi)
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
                                .Where(x => x.LinkDataObjectTypeId == EnumProductionStepLinkDataObjectType.ProductSemi)
                                .Select(x => x.LinkDataObjectId);
                var lsProductSemi = await _manufacturingDBContext.ProductSemi.AsNoTracking()
                    .Where(s => semiIds.Contains(s.ProductSemiId))
                    .ToListAsync();

                var lsProductionSemiFinal = await _manufacturingDBContext.ProductSemi.AsNoTracking()
                    .Where(x => x.ContainerId == toContainerId && (int)containerTypeId == x.ContainerTypeId)
                    .ToListAsync();

                //Filter not outsource data

                //remove outsource link
                process.ProductionStepLinkDatas = process.ProductionStepLinkDatas.Where(d => d.ProductionStepLinkDataTypeId == EnumProductionStepLinkDataType.None).ToList();

                // change id and unitId of last step
                if (process.ContainerTypeId == EnumContainerType.Product)
                {
                    var currentProductInfo = await _productHelperService.GetProduct((int)toContainerId);
                    process.ProductionStepLinkDatas.Where(p => p.LinkDataObjectId == fromContainerId && p.LinkDataObjectTypeId == EnumProductionStepLinkDataObjectType.Product).ToList().ForEach(x =>
                    {
                        x.LinkDataObjectId = toContainerId;
                        x.UnitId = currentProductInfo.UnitId;
                    });
                }

                //remove outsource step and outsource data from roles
                process.ProductionStepLinkDataRoles = process.ProductionStepLinkDataRoles
                    .Where(r => process.ProductionSteps.Select(s => s.ProductionStepId).Contains(r.ProductionStepId)
                    && process.ProductionStepLinkDatas.Select(l => l.ProductionStepLinkDataId).Contains(r.ProductionStepLinkDataId)
                    )
                    .ToList();


                //reset id and outsource data

                //reset id and outsource step
                process.ProductionSteps.ForEach(x =>
                {
                    x.ProductionStepId = 0;
                    x.ContainerId = toContainerId;
                    x.OutsourceStepRequestCode = null;
                    x.OutsourceStepRequestId = null;
                    x.ParentId = null;
                });


                //reset id and quantity, outsource data
                process.ProductionStepLinkDatas.ForEach(x =>
                {
                    x.Quantity = x.QuantityOrigin;
                    x.OutsourceQuantity = 0;
                    x.ExportOutsourceQuantity = 0;
                    x.OutsourcePartQuantity = null;
                    x.OutsourceRequestDetailId = null;
                    x.ProductionOutsourcePartMappingId = null;

                    x.ProductionStepLinkDataId = 0;
                    if (x.LinkDataObjectTypeId == EnumProductionStepLinkDataObjectType.ProductSemi)
                    {
                        var p = lsProductSemi.FirstOrDefault(s => s.ProductSemiId == x.LinkDataObjectId);
                        if (p == null)
                            throw new BadRequestException(ProductSemiErrorCode.NotFoundProductSemi);
                        var pf = lsProductionSemiFinal.FirstOrDefault(x => x.Title.ToLower().Equals(p.Title.ToLower()));
                        if (pf != null)
                        {
                            x.LinkDataObjectId = pf.ProductSemiId;
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

                            x.LinkDataObjectId = entityProductSemi.ProductSemiId;
                            lsProductionSemiFinal.Add(entityProductSemi);
                        }

                    }
                });



                //reset id
                process.ProductionStepLinkDataRoles.ForEach(r => { r.ProductionStepId = 0; r.ProductionStepLinkDataId = 0; });

                await UpdateProductionProcessManual(containerTypeId, toContainerId, new ProductionProcessModel
                {
                    ProductionStepLinkDataRoles = process.ProductionStepLinkDataRoles,
                    ContainerId = toContainerId,
                    ContainerTypeId = containerTypeId,
                    ProductionStepLinkDatas = process.ProductionStepLinkDatas,
                    ProductionSteps = process.ProductionSteps,
                    ProductionOutsourcePartMappings = new List<ProductionOutsourcePartMappingInput>()
                }, true);

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

        public async Task<IList<InternalProductionStepSimpleModel>> GetAllProductionStep(EnumContainerType containerTypeId, long containerId)
        {
            var productionSteps = await _manufacturingDBContext.ProductionStep.AsNoTracking()
                .Where(s => s.ContainerId == containerId && s.ContainerTypeId == (int)containerTypeId && s.IsGroup == true && s.IsFinish == false && s.StepId.HasValue)
                .Include(s => s.Step)
                .Include(x => x.ProductionStepLinkDataRole)
                .ThenInclude(r => r.ProductionStepLinkData)
                .ToListAsync();

            var roleOutput = productionSteps.SelectMany(x => x.ProductionStepLinkDataRole)
                .Where(x => x.ProductionStepLinkDataRoleTypeId == (int)EnumProductionStepLinkDataRoleType.Output);

            var productIds = roleOutput
                .Where(x => x.ProductionStepLinkData.LinkDataObjectTypeId == (int)EnumProductionStepLinkDataObjectType.Product)
                .Select(x => (int)x.ProductionStepLinkData.LinkDataObjectId)
                .Distinct()
                .ToList();
            var productSemiIds = roleOutput
                .Where(x => x.ProductionStepLinkData.LinkDataObjectTypeId == (int)EnumProductionStepLinkDataObjectType.ProductSemi)
                .Select(x => x.ProductionStepLinkData.LinkDataObjectId)
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
                    var objectId = x.ProductionStepLinkData.LinkDataObjectId;
                    var objectTypeId = x.ProductionStepLinkData.LinkDataObjectTypeId;
                    if (objectTypeId == (int)EnumProductionStepLinkDataObjectType.Product)
                        return productInfoMap.ContainsKey((int)objectId) ? productInfoMap[(int)objectId] : "";
                    else return productSemiInfoMap.ContainsKey(objectId) ? productSemiInfoMap[objectId] : "";
                });
                var title = string.IsNullOrEmpty(s.Title) ? s.Step?.StepName : s.Title;
                return new InternalProductionStepSimpleModel
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

        /// <summary>
        /// Lấy thông tin nhóm đầu ra đầu vào gia công
        /// </summary>
        /// <param name="containerType">Loại quy trình sản xuất</param>
        /// <param name="containerId">Mã quy trình sản xuất</param>
        /// <param name="arrProductionStepId">Danh sách công đoạn</param>
        /// <param name="Ignore">Loại bỏ các nhóm chi tiết đã gia công</param>
        /// <returns>Trả về nhóm chi tiết và danh sách đầu ra gia công (chi tiết chính và nội suy) </returns>
        public async Task<IList<GroupProductionStepToOutsource>> GroupProductionStepInOutToOutsource(EnumContainerType containerType, long containerId, long[] arrProductionStepId, bool Ignore = false)
        {
            int indexGroup = 1;
            var data = new List<GroupProductionStepToOutsource>();
            var groupRelationship = new NonCamelCaseDictionary();

            var lsProductionStepChildIds = await _manufacturingDBContext.ProductionStep.AsNoTracking()
                .Where(x => arrProductionStepId.Contains(x.ParentId.GetValueOrDefault()) && (Ignore ? true : x.OutsourceStepRequestId.GetValueOrDefault() == 0))
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

            foreach (var (key, value) in groupRelationship)
            {
                var stepIds = value as IList<long>;

                var item = GetGroupProductionStepToOutsource(roles, stepIds, key);
                data.Add(item);
            }

            return data;
        }

        // public async Task<IList<ProductionStepLinkDataInput>> GetAllProductInProductionProcess(EnumContainerType containerTypeId, long containerId)
        // {
        //     var productionSteps = await _manufacturingDBContext.ProductionStep.AsNoTracking()
        //         .Where(s => s.ContainerId == containerId && s.ContainerTypeId == (int)containerTypeId)
        //         .Include(s => s.ProductionStepLinkDataRole)
        //         .ToListAsync();

        //     var roles = productionSteps.SelectMany(x => x.ProductionStepLinkDataRole, (s, d) => new ProductionStepLinkDataRoleInput
        //     {
        //         ProductionStepId = s.ProductionStepId,
        //         ProductionStepLinkDataId = d.ProductionStepLinkDataId,
        //         ProductionStepCode = s.ProductionStepCode,
        //     }).ToList();

        //     //Lấy thông tin dữ liệu của steplinkdata
        //     var lsProductionStepLinkDataId = roles.Select(x => x.ProductionStepLinkDataId).Distinct().ToList();
        //     IList<ProductionStepLinkDataInput> stepLinkDatas = new List<ProductionStepLinkDataInput>();
        //     if (lsProductionStepLinkDataId.Count > 0)
        //     {
        //         var sql = new StringBuilder(@$"
        //             SELECT * FROM dbo.ProductionStepLinkDataExtractInfo v 
        //             WHERE v.LinkDataObjectTypeId = 1 AND v.ProductionStepLinkDataId IN (SELECT [Value] FROM @ProductionStepLinkDataIds)
        //         ");
        //         var parammeters = new List<SqlParameter>()
        //         {
        //             lsProductionStepLinkDataId.ToSqlParameter("@ProductionStepLinkDataIds"),
        //         };

        //         stepLinkDatas = await _manufacturingDBContext.QueryList<ProductionStepLinkDataInput>(sql.ToString(), parammeters);
        //     }

        //     return stepLinkDatas.GroupBy(x => x.LinkDataObjectId)
        //     .Select(x =>
        //     {
        //         var result = x.First();
        //         result.QuantityOrigin = x.Sum(s => s.QuantityOrigin);
        //         return result;
        //     }).ToList();
        // }


        public async Task<IList<ProductionStepLinkDataObjectModel>> GetAllProductInProductionProcessV2(EnumContainerType containerTypeId, long containerId)
        {
            var productionSteps = await _manufacturingDBContext.ProductionStep.AsNoTracking()
                .Where(s => s.ContainerId == containerId && s.ContainerTypeId == (int)containerTypeId && !s.IsFinish && (!s.IsGroup.HasValue || !s.IsGroup.Value))
                .Include(s => s.ProductionStepLinkDataRole)
                .ThenInclude(r => r.ProductionStepLinkData)
                .ToListAsync();

            var roles = productionSteps.SelectMany(x => x.ProductionStepLinkDataRole).ToList();

            var productionStepLinkDatas = await _manufacturingDBContext.ProductionStepLinkData
                .Where(x => roles.Select(x => x.ProductionStepLinkDataId).Contains(x.ProductionStepLinkDataId))
                .ToListAsync();

            var lastLinkDatas = roles.GroupBy(x => x.ProductionStepLinkDataId)
                         .Where(x => x.Count() == 1 && x.First().ProductionStepLinkDataRoleTypeId == (int)EnumProductionStepLinkDataRoleType.Output)
                        .Select(x => x.First().ProductionStepLinkData)
                        .Distinct()
                        .ToList();

            var branches = new List<List<ProductionStepLinkData>>();
            var travledLinkDataIds = new HashSet<long>();
            foreach (var brach in lastLinkDatas)
            {
                var stack = new Stack<ProductionStepLinkData>();
                stack.Push(brach);

                var brachItem = new List<ProductionStepLinkData>();
                while (stack.Count > 0)
                {
                    var currentLinkData = stack.Pop();
                    brachItem.Add(currentLinkData);
                    travledLinkDataIds.Add(currentLinkData.ProductionStepLinkDataId);

                    var step = roles.FirstOrDefault(r => r.ProductionStepLinkDataId == currentLinkData.ProductionStepLinkDataId && r.ProductionStepLinkDataRoleTypeId == (int)EnumProductionStepLinkDataRoleType.Output);
                    if (step != null)
                    {
                        var parents = roles.Where(x => x.ProductionStepId == step.ProductionStepId && x.ProductionStepLinkDataRoleTypeId == (int)EnumProductionStepLinkDataRoleType.Input)
                             .Select(x => x.ProductionStepLinkData)
                             .Distinct()
                             .ToList();

                        foreach (var p in parents)
                        {
                            if (!travledLinkDataIds.Contains(p.ProductionStepLinkDataId))
                                stack.Push(p);
                        }
                    }
                }
                branches.Add(brachItem);
            }

            return branches.SelectMany(b => b.GroupBy(p => new { p.LinkDataObjectId, p.LinkDataObjectTypeId }).Select(p => new
            {
                p.Key.LinkDataObjectTypeId,
                p.Key.LinkDataObjectId,
                Quantity = p.Max(l => l.QuantityOrigin),
                p.First().ProductionStepLinkDataId
            }))
                .GroupBy(p => new { p.LinkDataObjectId, p.LinkDataObjectTypeId })
                .Select(p => new ProductionStepLinkDataObjectModel
                {
                    LinkDataObjectTypeId = (EnumProductionStepLinkDataObjectType)p.Key.LinkDataObjectTypeId,
                    LinkDataObjectId = p.Key.LinkDataObjectId,
                    Quantity = p.Sum(l => l.Quantity)
                })
                .ToList();

        }


        public async Task<IList<ProductionStepLinkDataInput>> GetAllProductInProductionProcess(EnumContainerType containerTypeId, long containerId)
        {
            var productionSteps = await _manufacturingDBContext.ProductionStep.AsNoTracking()
                .Where(s => s.ContainerId == containerId && s.ContainerTypeId == (int)containerTypeId && !s.IsFinish && (!s.IsGroup.HasValue || !s.IsGroup.Value))
                .Include(s => s.ProductionStepLinkDataRole)
                .ToListAsync();

            var roles = productionSteps.SelectMany(x => x.ProductionStepLinkDataRole, (s, d) => new ProductionStepLinkDataRoleInput
            {
                ProductionStepId = s.ProductionStepId,
                ProductionStepLinkDataId = d.ProductionStepLinkDataId,
                ProductionStepCode = s.ProductionStepCode,
                ProductionStepLinkDataRoleTypeId = (EnumProductionStepLinkDataRoleType)d.ProductionStepLinkDataRoleTypeId,
            }).ToList();

            var productionStepLinkDatas = await _manufacturingDBContext.ProductionStepLinkData
                .Where(x => roles.Select(x => x.ProductionStepLinkDataId).Contains(x.ProductionStepLinkDataId))
                .ToListAsync();

            var arrayLastProductionStepId = roles.GroupBy(x => x.ProductionStepLinkDataId)
                         .Where(x => x.Count() == 1)
                         .SelectMany(x => x)
                         .Where(x => x.ProductionStepLinkDataRoleTypeId == EnumProductionStepLinkDataRoleType.Output)
                         .Select(x => x.ProductionStepId)
                         .Distinct()
                         .ToArray();

            var arrayNode = new List<AllProductInProductionProcessNode>();
            foreach (var (i, productionStepId) in arrayLastProductionStepId.Select((productionStepId, i) => (i, productionStepId)))
            {
                var localRoles = roles.Where(x => x.ProductionStepId == productionStepId);

                var node = new AllProductInProductionProcessNode(localRoles.Where(x => x.ProductionStepLinkDataRoleTypeId == EnumProductionStepLinkDataRoleType.Output)
                                                                           .Select(x => x.ProductionStepLinkDataId)
                                                                           .Distinct()
                                                                           .ToList());

                var arrayInputPart = localRoles.Where(x => x.ProductionStepLinkDataRoleTypeId == EnumProductionStepLinkDataRoleType.Input)
                                               .Select(x => x.ProductionStepLinkDataId)
                                               .Distinct();
                var arrayPrevProductionStepOfInputPart = roles.Where(x => arrayInputPart.Contains(x.ProductionStepLinkDataId) && x.ProductionStepLinkDataRoleTypeId == EnumProductionStepLinkDataRoleType.Output)
                                                              .Select(x => x.ProductionStepId)
                                                              .Distinct()
                                                              .ToArray();

                if (arrayPrevProductionStepOfInputPart.Length > 0)
                {
                    var resultForLoop = LoopAllProductInProductionProcess(roles, arrayPrevProductionStepOfInputPart);

                    if (resultForLoop.Count > 0)
                    {
                        var firstData = resultForLoop.First();
                        node.ArrayProductionStepLinkData.AddRange(firstData.ArrayProductionStepLinkData);

                        arrayNode.AddRange(resultForLoop.Skip(1));
                    }
                    else arrayNode.AddRange(resultForLoop);
                }
                else
                {
                    node.ArrayProductionStepLinkData.AddRange(localRoles.Where(x => x.ProductionStepLinkDataRoleTypeId == EnumProductionStepLinkDataRoleType.Input)
                                                                           .Select(x => x.ProductionStepLinkDataId)
                                                                           .Distinct()
                                                                           .ToList());
                }

                if (node.ArrayProductionStepLinkData.Count > 0)
                    arrayNode.Add(node);
            }

            var arrayProductionStepLinkDataPassed = new List<long>();
            var calcQuantityForNode = new List<AllProductInProductionProcessNodeResult>();
            foreach (var node in arrayNode)
            {
                var resultCalc = productionStepLinkDatas.Where(x => node.ArrayProductionStepLinkData.Where(x => !arrayProductionStepLinkDataPassed.Contains(x)).Contains(x.ProductionStepLinkDataId))
                .GroupBy(x => new { x.LinkDataObjectId, x.LinkDataObjectTypeId })
                .Select(x =>
                {
                    var value = x.First();
                    var quantity = x.Max(x => x.QuantityOrigin);
                    return new AllProductInProductionProcessNodeResult()
                    {
                        ProductionStepLinkDataId = value.ProductionStepLinkDataId,
                        LinkDataObjectId = value.LinkDataObjectId,
                        LinkDataObjectTypeId = value.LinkDataObjectTypeId,
                        Quantity = quantity
                    };
                });

                calcQuantityForNode.AddRange(resultCalc);
                arrayProductionStepLinkDataPassed.AddRange(node.ArrayProductionStepLinkData);
            }

            calcQuantityForNode = calcQuantityForNode.GroupBy(x => new { x.LinkDataObjectId, x.LinkDataObjectTypeId })
                .Select(x =>
                {
                    var value = x.First();
                    var quantity = x.Sum(x => x.Quantity);
                    return new AllProductInProductionProcessNodeResult()
                    {
                        ProductionStepLinkDataId = value.ProductionStepLinkDataId,
                        LinkDataObjectId = value.LinkDataObjectId,
                        LinkDataObjectTypeId = value.LinkDataObjectTypeId,
                        Quantity = quantity
                    };
                })
                .ToList();

            var lsProductionStepLinkDataId = calcQuantityForNode.Select(x => x.ProductionStepLinkDataId).ToArray();
            IList<ProductionStepLinkDataInput> stepLinkDatas = new List<ProductionStepLinkDataInput>();
            if (lsProductionStepLinkDataId.Length > 0)
            {
                var sql = new StringBuilder(@$"
                        SELECT * FROM dbo.ProductionStepLinkDataExtractInfo v 
                        WHERE v.LinkDataObjectTypeId = 1 AND v.ProductionStepLinkDataId IN (SELECT [Value] FROM @ProductionStepLinkDataIds)
                    ");
                var parammeters = new List<SqlParameter>()
                    {
                        lsProductionStepLinkDataId.ToSqlParameter("@ProductionStepLinkDataIds"),
                    };

                stepLinkDatas = await _manufacturingDBContext.QueryListRaw<ProductionStepLinkDataInput>(sql.ToString(), parammeters);
            }

            return stepLinkDatas.Select(x =>
            {
                var calc = calcQuantityForNode.FirstOrDefault(c => c.ProductionStepLinkDataId == x.ProductionStepLinkDataId);
                if (calc != null)
                    x.QuantityOrigin = calc.Quantity;
                return x;
            }).ToList();
        }

        private IList<AllProductInProductionProcessNode> LoopAllProductInProductionProcess(IEnumerable<ProductionStepLinkDataRoleInput> roles, IEnumerable<long> arrayLastProductionStepId)
        {
            var arrayNode = new List<AllProductInProductionProcessNode>();

            foreach (var (i, productionStepId) in arrayLastProductionStepId.Select((productionStepId, i) => (i, productionStepId)))
            {
                var localRoles = roles.Where(x => x.ProductionStepId == productionStepId);

                var node = new AllProductInProductionProcessNode(localRoles.Where(x => x.ProductionStepLinkDataRoleTypeId == EnumProductionStepLinkDataRoleType.Output)
                                                                           .Select(x => x.ProductionStepLinkDataId)
                                                                           .Distinct()
                                                                           .ToList());

                var arrayInputPart = localRoles.Where(x => x.ProductionStepLinkDataRoleTypeId == EnumProductionStepLinkDataRoleType.Input)
                                               .Select(x => x.ProductionStepLinkDataId)
                                               .Distinct();
                var arrayPrevProductionStepOfInputPart = roles.Where(x => arrayInputPart.Contains(x.ProductionStepLinkDataId) && x.ProductionStepLinkDataRoleTypeId == EnumProductionStepLinkDataRoleType.Output)
                                                              .Select(x => x.ProductionStepId)
                                                              .Distinct()
                                                              .ToArray();

                if (arrayPrevProductionStepOfInputPart.Length > 0)
                {
                    var resultForLoop = LoopAllProductInProductionProcess(roles, arrayPrevProductionStepOfInputPart);

                    if (resultForLoop.Count > 0)
                    {
                        var firstData = resultForLoop.First();
                        node.ArrayProductionStepLinkData.AddRange(firstData.ArrayProductionStepLinkData);

                        arrayNode.AddRange(resultForLoop.Skip(1));
                    }
                    else arrayNode.AddRange(resultForLoop);
                }
                else
                {
                    node.ArrayProductionStepLinkData.AddRange(localRoles.Where(x => x.ProductionStepLinkDataRoleTypeId == EnumProductionStepLinkDataRoleType.Input)
                                                                           .Select(x => x.ProductionStepLinkDataId)
                                                                           .Distinct()
                                                                           .ToList());
                }

                if (node.ArrayProductionStepLinkData.Count > 0)
                    arrayNode.Add(node);
            }

            return arrayNode;
        }

        public async Task<IList<ProductionProcessWarningMessage>> ValidateStatusProductionProcess(IList<int> productIds)
        {
            var lstWaring = new List<ProductionProcessWarningMessage>();
            var products = await _productHelperService.GetListProducts(productIds);
            foreach ( var product in products )
            {
                if (product.ProductionProcessStatusId != EnumProductionProcessStatus.Created)
                {
                    lstWaring.Add(new ProductionProcessWarningMessage()
                    {
                        WarningCode = EnumProductionProcessWarningCode.WarningProduct,
                        Message = $"Mặt hàng {product.ProductCode} {product.ProductionProcessStatusId.GetEnumDescription()} QTSX",
                        GroupName = EnumProductionProcessWarningCode.WarningProduct.GetEnumDescription()
                    });
                }
                
            }
            return lstWaring;
        }

        public class AllProductInProductionProcessNode
        {
            public List<long> ArrayProductionStepLinkData { get; set; }

            public AllProductInProductionProcessNode(IEnumerable<long> arrayProductionStepLinkData)
            {
                ArrayProductionStepLinkData = new List<long>(arrayProductionStepLinkData);
            }

            public AllProductInProductionProcessNode()
            {
                ArrayProductionStepLinkData = new List<long>();
            }
        }

        public class AllProductInProductionProcessNodeResult
        {
            public long ProductionStepLinkDataId { get; set; }
            public long LinkDataObjectId { get; set; }
            public int LinkDataObjectTypeId { get; set; }
            public decimal Quantity { get; set; }
        }

    }
}
