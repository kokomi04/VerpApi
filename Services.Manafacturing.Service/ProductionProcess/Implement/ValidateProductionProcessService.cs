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
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.ManufacturingDB;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Manafacturing.Model.Outsource.RequestPart;
using VErp.Services.Manafacturing.Model.Outsource.RequestStep;
using VErp.Services.Manafacturing.Model.ProductionOrder;
using VErp.Services.Manafacturing.Model.ProductionProcess;
using VErp.Services.Manafacturing.Model.ProductionStep;
using VErp.Services.Manafacturing.Service.Outsource;
using static VErp.Commons.Enums.Manafacturing.EnumProductionProcess;

namespace VErp.Services.Manafacturing.Service.ProductionProcess.Implement
{
    public class ValidateProductionProcessService : IValidateProductionProcessService
    {

        private readonly ManufacturingDBContext _manufacturingDBContext;
        private readonly IActivityLogService _activityLogService;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        public ValidateProductionProcessService(ManufacturingDBContext manufacturingDB
            , IActivityLogService activityLogService
            , ILogger<ProductionProcessService> logger
            , IMapper mapper)
        {
            _manufacturingDBContext = manufacturingDB;
            _activityLogService = activityLogService;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<IList<ProductionProcessWarningMessage>> ValidateProductionProcess(EnumContainerType containerTypeId, long containerId, ProductionProcessModel productionProcess)
        {
            if (productionProcess != null && productionProcess.ProductionStepLinkDataRoles.Count == 0)
            {
                var warningCode = containerTypeId == EnumContainerType.ProductionOrder ? EnumProductionProcessWarningCode.WarningProductionStep : EnumProductionProcessWarningCode.WarningProduct;
                return new[] { new ProductionProcessWarningMessage {
                    Message = "Chưa thiết lập quy trình sản xuất",
                    WarningCode = warningCode,
                    GroupName = warningCode.GetEnumDescription()
                } };
            }

            var lsWarning = new List<ProductionProcessWarningMessage>();

            var warningProductionStep = await ValidateProductionStep(productionProcess);
            var warningProductionLinkData = await ValidateProductionStepLinkData(productionProcess);

            lsWarning.AddRange(warningProductionStep);
            lsWarning.AddRange(warningProductionLinkData);

            if (containerTypeId == EnumContainerType.ProductionOrder)
            {
                var warningOutsourcePartRequest = await ValidateOutsourcePartRequest(productionProcess);
                var warningOutsourceStepRequest = await ValidateOutsourceStepRequest(productionProcess);
                var warningProductionOrder = await ValidateProductionOrder(productionProcess);
                lsWarning.AddRange(warningProductionOrder);
                lsWarning.AddRange(warningOutsourcePartRequest);
                lsWarning.AddRange(warningOutsourceStepRequest);
            }

            return lsWarning;
        }

        public async Task<IList<ProductionProcessWarningMessage>> ValidateProductionOrder(ProductionProcessModel productionProcess)
        {
            IList<ProductionProcessWarningMessage> lsWarning = new List<ProductionProcessWarningMessage>();

            var sql = $"SELECT * FROM vProductionOrderDetail WHERE ProductionOrderId = @ProductionOrderId";
            var parammeters = new SqlParameter[]
            {
                    new SqlParameter("@ProductionOrderId", productionProcess.ContainerId)
            };
            var resultData = await _manufacturingDBContext.QueryDataTable(sql, parammeters);

            var productionOrderDetail = resultData.ConvertData<ProductionOrderDetailOutputModel>();

            var stepFinal = productionProcess.ProductionSteps.FirstOrDefault(x => x.IsFinish);

            if (stepFinal != null)
            {
                var linkData = from role in productionProcess.ProductionStepLinkDataRoles
                               join ld in productionProcess.ProductionStepLinkDatas on role.ProductionStepLinkDataCode equals ld.ProductionStepLinkDataCode
                               where role.ProductionStepCode == stepFinal.ProductionStepCode && role.ProductionStepLinkDataRoleTypeId == EnumProductionStepLinkDataRoleType.Input
                               select ld;
                foreach(var ld in linkData)
                {
                    var p = productionOrderDetail.FirstOrDefault(x => x.ProductId == ld.ObjectId);
                    if(p == null)
                    {
                        lsWarning.Add(new ProductionProcessWarningMessage
                        {
                            Message = $"Sản phẩm \"{ld.ObjectTitle}\" đã bị xóa, cần xem xét thay đổi quy trình sản xuất.",
                            ObjectId = ld.ProductionStepLinkDataId,
                            ObjectCode = ld.ProductionStepLinkDataCode,
                            GroupName = EnumProductionProcessWarningCode.WarningProductionOrder.GetEnumDescription(),
                            WarningCode = EnumProductionProcessWarningCode.WarningProductionOrder
                        });
                    }
                }
                

            }

            return lsWarning;
        }

        public async Task<IList<ProductionProcessWarningMessage>> ValidateOutsourceStepRequest(ProductionProcessModel productionProcess)
        {
            IList<ProductionProcessWarningMessage> lsWarning = new List<ProductionProcessWarningMessage>();

            var outsourceStepRequest = (await _manufacturingDBContext.OutsourceStepRequest.AsNoTracking()
                .Where(x => x.ProductionOrderId == productionProcess.ContainerId)
                .ProjectTo<OutsourceStepRequestModel>(_mapper.ConfigurationProvider)
                .ToListAsync())
                .ToDictionary(x => x.OutsourceStepRequestId, x => x);

            var dicOutsourceStep = new Dictionary<long, IList<string>>();
            foreach (var stepRequest in outsourceStepRequest)
                dicOutsourceStep.Add(stepRequest.Key, productionProcess.ProductionSteps.Where(x=>x.OutsourceStepRequestId == stepRequest.Key).Select(x=>x.ProductionStepCode).ToList());

            var productionStepIds = dicOutsourceStep.SelectMany(x => x.Value);
            var productionStepLinkDataIds = productionProcess.ProductionStepLinkDataRoles
                                            .Where(x => productionStepIds.Contains(x.ProductionStepCode))
                                            .GroupBy(x => x.ProductionStepLinkDataCode)
                                            .ToDictionary(x => x.Key, x => new
                                            {
                                                x.First().ProductionStepLinkDataCode,
                                                x.First().ProductionStepCode,
                                                x.First().ProductionStepLinkDataId,
                                            });

            foreach (var linkData in productionProcess.ProductionStepLinkDatas)
            {
                var quantity = linkData.QuantityOrigin - (linkData.OutsourceQuantity + linkData.OutsourcePartQuantity);

                if (productionStepLinkDataIds.ContainsKey(linkData.ProductionStepLinkDataCode)
                    && (linkData.OutsourceQuantity > (linkData.QuantityOrigin - linkData.OutsourcePartQuantity) || linkData.ExportOutsourceQuantity > quantity))
                {
                    var stepInfo = productionStepLinkDataIds[linkData.ProductionStepLinkDataCode];
                    var stepRequest = outsourceStepRequest[dicOutsourceStep.FirstOrDefault(x => x.Value.Contains(stepInfo.ProductionStepCode)).Key];
                    lsWarning.Add(new ProductionProcessWarningMessage
                    {
                        Message = $"YCGC {stepRequest.OutsourceStepRequestCode} - Chi tiết \"{linkData.ObjectTitle}\" có số lượng gia công vượt quá so với QTSX.",
                        ObjectId = stepRequest.OutsourceStepRequestId,
                        ObjectCode = stepRequest.OutsourceStepRequestCode,
                        GroupName = EnumProductionProcessWarningCode.WarningOutsourceStepRequest.GetEnumDescription(),
                        WarningCode = EnumProductionProcessWarningCode.WarningOutsourceStepRequest,
                    });
                }
            }

            return lsWarning;
        }

        public async Task<IList<ProductionProcessWarningMessage>> ValidateOutsourcePartRequest(ProductionProcessModel productionProcess)
        {
            IList<ProductionProcessWarningMessage> lsWarning = new List<ProductionProcessWarningMessage>();

            var outsourceLinkData = productionProcess.ProductionStepLinkDatas
                                    .Where(x => x.ProductionStepLinkDataTypeId == EnumProductionStepLinkDataType.StepLinkDataOutsourcePart).ToList();

            var sumQuantityUsage = outsourceLinkData.GroupBy(x => x.OutsourceRequestDetailId)
                                    .Select(x => new
                                    {
                                        OutsourcePartRequestDetailId = x.Key,
                                        QuantityUsage = x.Sum(x => x.Quantity)
                                    });

            var outsourcePartRequestDetails = await GetOutsourcePartRequestDetailInfo(productionProcess.ContainerId);

            if (outsourcePartRequestDetails.Count() > 0)
            {
                foreach (var outsourcePartRequestDetail in outsourcePartRequestDetails)
                {

                    var usage = sumQuantityUsage.FirstOrDefault(x => x.OutsourcePartRequestDetailId == outsourcePartRequestDetail.OutsourcePartRequestDetailId);

                    if (usage == null)
                    {
                        lsWarning.Add(new ProductionProcessWarningMessage
                        {
                            Message = $"YCGC {outsourcePartRequestDetail.OutsourcePartRequestCode} - Chi tiết \"{outsourcePartRequestDetail.ProductPartTitle}\" của SP \"{outsourcePartRequestDetail.ProductTitle}\" chưa được thiết lập trong QTSX.",
                            ObjectId = outsourcePartRequestDetail.OutsourcePartRequestId,
                            ObjectCode = outsourcePartRequestDetail.OutsourcePartRequestCode,
                            GroupName = EnumProductionProcessWarningCode.WarningOutsourcePartRequest.GetEnumDescription(),
                            WarningCode = EnumProductionProcessWarningCode.WarningOutsourcePartRequest,
                        });

                    }
                    else if (usage.QuantityUsage != outsourcePartRequestDetail.Quantity)
                    {
                        lsWarning.Add(new ProductionProcessWarningMessage
                        {
                            Message = $"YCGC {outsourcePartRequestDetail.OutsourcePartRequestCode} - Số lượng của chi tiết \"{outsourcePartRequestDetail.ProductPartTitle}\" của SP \"{outsourcePartRequestDetail.ProductTitle}\" thiết lập chưa chính xác(thừa/thiếu) trong QTSX.",
                            ObjectId = outsourcePartRequestDetail.OutsourcePartRequestId,
                            ObjectCode = outsourcePartRequestDetail.OutsourcePartRequestCode,
                            GroupName = EnumProductionProcessWarningCode.WarningOutsourcePartRequest.GetEnumDescription(),
                            WarningCode = EnumProductionProcessWarningCode.WarningOutsourcePartRequest,
                        });
                    }
                }
            }

            var requestDetailIds = outsourcePartRequestDetails.Select(x => x.OutsourcePartRequestDetailId);
            foreach (var linkData in outsourceLinkData)
            {
                if (!requestDetailIds.Contains(linkData.OutsourceRequestDetailId.GetValueOrDefault()))
                {
                    lsWarning.Add(new ProductionProcessWarningMessage
                    {
                        Message = $"Chi tiết gia công \"{linkData.ObjectTitle}\" không thuộc bất kỳ đơn YCGC nào. Cần phải xóa chi tiết này đi.",
                        ObjectId = linkData.ProductionStepLinkDataId,
                        ObjectCode = linkData.ProductionStepLinkDataCode,
                        GroupName = EnumProductionProcessWarningCode.WarningProductionLinkData.GetEnumDescription(),
                        WarningCode = EnumProductionProcessWarningCode.WarningProductionLinkData,
                    });
                }
            }

            return lsWarning;
        }

        private Task<IList<ProductionProcessWarningMessage>> ValidateProductionStep(ProductionProcessModel productionProcess)
        {
            IList<ProductionProcessWarningMessage> lsWarning = new List<ProductionProcessWarningMessage>();

            var productionSteps = productionProcess.ProductionSteps.Where(x => x.IsGroup == true && x.IsFinish == false).ToList();
            var productionStepInOuts = productionProcess.ProductionSteps.Where(x => x.IsGroup == false).ToList();

            if (productionSteps.Count() > 0 && productionSteps.Any(x => !x.StepId.HasValue))
            {

                lsWarning.Add(new ProductionProcessWarningMessage
                {
                    Message = $"Trong QTSX đang có công đoạn trắng. Cần thiết lập nó là công đoạn gì.",
                    GroupName = EnumProductionProcessWarningCode.WarningProductionStep.GetEnumDescription(),
                    WarningCode = EnumProductionProcessWarningCode.WarningProductionStep,
                });
            }

            productionSteps.Where(x => x.StepId.HasValue).Join(productionStepInOuts, x => x.ProductionStepId, y => y.ParentId, (x, y) => new {
                ProductionStep = x,
                ProductionStepInOutId = y?.ProductionStepId
            }).ToList().ForEach(x => {
                if (!x.ProductionStepInOutId.HasValue) {
                    lsWarning.Add(new ProductionProcessWarningMessage {
                        Message = $"Công đoạn {x.ProductionStep.Title} chưa thiết lập nhóm (đầu ra đầu vào)",
                        GroupName = EnumProductionProcessWarningCode.WarningProductionStep.GetEnumDescription(),
                        WarningCode = EnumProductionProcessWarningCode.WarningProductionStep,
                    });
                }
            });


            var groupRole = productionProcess.ProductionStepLinkDataRoles.GroupBy(x => x.ProductionStepCode);

            var productionStepInOutNoRoles =  productionStepInOuts.Where(x => !groupRole.Any(g => g.Key.Equals(x.ProductionStepCode))).ToList();

            foreach(var p in productionStepInOutNoRoles) {
                var step = productionSteps.FirstOrDefault(x => x.ProductionStepId == p.ParentId);
                if (step == null || step.IsFinish) {
                    continue;
                }

                lsWarning.Add(new ProductionProcessWarningMessage {
                    Message = $"Công đoạn {step.Title} có nhóm (đầu ra đầu vào) không có đầu ra đầu vào",
                    GroupName = EnumProductionProcessWarningCode.WarningProductionStep.GetEnumDescription(),
                    WarningCode = EnumProductionProcessWarningCode.WarningProductionStep,
                });
            }

            foreach (var group in groupRole)
            {
                var inOutOfStep = productionStepInOuts.FirstOrDefault(x => x.ProductionStepCode == group.Key);
                if (inOutOfStep == null) {
                    continue;
                }

                var step = productionSteps.FirstOrDefault(x => x.ProductionStepId == inOutOfStep.ParentId);
                if (step == null || step.IsFinish)
                {
                    continue;
                }
                var inputs = group.Where(x => x.ProductionStepLinkDataRoleTypeId == EnumProductionStepLinkDataRoleType.Input);
                var outputs = group.Where(x => x.ProductionStepLinkDataRoleTypeId == EnumProductionStepLinkDataRoleType.Output);

                if (inputs.Count() == 0 && outputs.Count() == 0)
                {
                    lsWarning.Add(new ProductionProcessWarningMessage
                    {
                        Message = $"Công đoạn \"{step.Title}\" có nhóm (đầu ra đầu vào) không có đầu vào và đầu ra",
                        ObjectCode = step.ProductionStepCode,
                        ObjectId = step.ProductionStepId,
                        GroupName = EnumProductionProcessWarningCode.WarningProductionStep.GetEnumDescription(),
                        WarningCode = EnumProductionProcessWarningCode.WarningProductionStep,
                    });
                    continue;
                }

                if (inputs.Count() == 0)
                {
                    lsWarning.Add(new ProductionProcessWarningMessage
                    {
                        Message = $"Công đoạn \"{step.Title}\" có nhóm (đầu ra đầu vào) không có đầu vào",
                        ObjectCode = step.ProductionStepCode,
                        ObjectId = step.ProductionStepId,
                        GroupName = EnumProductionProcessWarningCode.WarningProductionStep.GetEnumDescription(),
                        WarningCode = EnumProductionProcessWarningCode.WarningProductionStep,
                    });
                }
                if (outputs.Count() == 0)
                {
                    lsWarning.Add(new ProductionProcessWarningMessage
                    {
                        Message = $"Công đoạn \"{step.Title}\" có nhóm (đầu ra đầu vào) không có đầu ra",
                        ObjectCode = step.ProductionStepCode,
                        ObjectId = step.ProductionStepId,
                        GroupName = EnumProductionProcessWarningCode.WarningProductionStep.GetEnumDescription(),
                        WarningCode = EnumProductionProcessWarningCode.WarningProductionStep,
                    });
                }

                if (inputs.Count() > 0 && outputs.Count() > 0)
                {
                    var inputLinkDataInfos = productionProcess.ProductionStepLinkDatas
                        .Where(l => inputs.Select(x => x.ProductionStepLinkDataCode).Contains(l.ProductionStepLinkDataCode)
                            && l.ObjectTypeId == EnumProductionStepLinkDataObjectType.Product);
                    var outputLinkDataInfos = productionProcess.ProductionStepLinkDatas
                        .Where(l => outputs.Select(x => x.ProductionStepLinkDataCode).Contains(l.ProductionStepLinkDataCode)
                         && l.ObjectTypeId == EnumProductionStepLinkDataObjectType.Product);

                    var duplicates = from i in inputLinkDataInfos
                                     join o in outputLinkDataInfos
                                        on new { i.ObjectId, i.ObjectTypeId } equals new { o.ObjectId, o.ObjectTypeId }
                                     select i;
                    foreach (var d in duplicates)
                        lsWarning.Add(new ProductionProcessWarningMessage
                        {
                            Message = $"Công đoạn \"{step.Title}\" có nhóm (đầu ra đầu vào) có chi tiết \"{d.ObjectTitle}\" xuất hiện ở đầu vào và đầu ra.",
                            ObjectCode = step.ProductionStepCode,
                            ObjectId = step.ProductionStepId,
                            GroupName = EnumProductionProcessWarningCode.WarningProductionStep.GetEnumDescription(),
                            WarningCode = EnumProductionProcessWarningCode.WarningProductionStep,
                        });
                }
            }

            return Task.FromResult(lsWarning);
        }

        private Task<IList<ProductionProcessWarningMessage>> ValidateProductionStepLinkData(ProductionProcessModel productionProcess)
        {
            IList<ProductionProcessWarningMessage> lsWarning = new List<ProductionProcessWarningMessage>();

            var lsInputStep = productionProcess.ProductionStepLinkDataRoles.Where(x => x.ProductionStepLinkDataRoleTypeId == EnumProductionStepLinkDataRoleType.Input);
            var lsOutputStep = productionProcess.ProductionStepLinkDataRoles.Where(x => x.ProductionStepLinkDataRoleTypeId == EnumProductionStepLinkDataRoleType.Output);

            var stepLinkDatas = productionProcess.ProductionStepLinkDatas.GroupBy(x => x.ProductionStepLinkDataCode);
            foreach (var group in stepLinkDatas)
            {
                if (group.Count() > 1)
                {
                    lsWarning.Add(new ProductionProcessWarningMessage
                    {
                        Message = $"Tồn tại 2 chi tiết có mã \"{group.Key}\"",
                        GroupName = EnumProductionProcessWarningCode.WarningProductionLinkData.GetEnumDescription(),
                        WarningCode = EnumProductionProcessWarningCode.WarningProductionLinkData,
                    });
                    continue;
                }
                var inStep = lsInputStep.Where(x => x.ProductionStepLinkDataCode.Equals(group.Key)).ToList();
                var outStep = lsOutputStep.Where(x => x.ProductionStepLinkDataCode.Equals(group.Key)).ToList();

                var linkData = group.First();
                if (inStep.Count == 0 && outStep.Count == 0)
                {
                    lsWarning.Add(new ProductionProcessWarningMessage
                    {
                        Message = $"Chi tiết \"{linkData.ObjectTitle}\" không thuộc bất kỳ công đoạn nào.",
                        ObjectCode = linkData.ProductionStepLinkDataCode,
                        ObjectId = linkData.ProductionStepLinkDataId,
                        GroupName = EnumProductionProcessWarningCode.WarningProductionLinkData.GetEnumDescription(),
                        WarningCode = EnumProductionProcessWarningCode.WarningProductionLinkData,
                    });
                }
                if (inStep.Count > 1)
                {
                    lsWarning.Add(new ProductionProcessWarningMessage
                    {
                        Message = $"Chi tiết \"{linkData.ObjectTitle}\" không thể là đầu vào của 2 công đoạn.",
                        ObjectCode = linkData.ProductionStepLinkDataCode,
                        ObjectId = linkData.ProductionStepLinkDataId,
                        GroupName = EnumProductionProcessWarningCode.WarningProductionLinkData.GetEnumDescription(),
                        WarningCode = EnumProductionProcessWarningCode.WarningProductionLinkData,
                    });
                }
                if (outStep.Count > 1)
                {
                    lsWarning.Add(new ProductionProcessWarningMessage
                    {
                        Message = $"Chi tiết \"{linkData.ObjectTitle}\" không thể là đầu ra của 2 công đoạn.",
                        ObjectCode = linkData.ProductionStepLinkDataCode,
                        ObjectId = linkData.ProductionStepLinkDataId,
                        GroupName = EnumProductionProcessWarningCode.WarningProductionLinkData.GetEnumDescription(),
                        WarningCode = EnumProductionProcessWarningCode.WarningProductionLinkData,
                    });
                }
            }

            /*
             * Kiểm tra sản phẩm đầu ra cuối cùng đã về công đoạn kết thúc hay chưa
             */

            var ldFinishInvalid = productionProcess.ProductionStepLinkDataRoles.GroupBy(x => x.ProductionStepLinkDataCode)
                .Where(x => x.Count() == 1)
                .Select(x => x.FirstOrDefault())
                .Where(x => x.ProductionStepLinkDataRoleTypeId == EnumProductionStepLinkDataRoleType.Output);
            var productIds = _manufacturingDBContext.ProductionOrderDetail.AsNoTracking()
                .Where(x => x.ProductionOrderId == productionProcess.ContainerId)
                .Select(x=> (long)x.ProductId);
            foreach (var l in ldFinishInvalid)
            {
                var ld = productionProcess.ProductionStepLinkDatas.FirstOrDefault(x => x.ProductionStepLinkDataCode == l.ProductionStepLinkDataCode);
                if (ld != null && productionProcess.ContainerTypeId == EnumContainerType.ProductionOrder && productIds.Contains(ld.ObjectId))
                    lsWarning.Add(new ProductionProcessWarningMessage
                    {
                        Message = $"Chi tiết đầu ra \"{ld.ObjectTitle}\" chưa được kết nối công đoạn \"Kết thúc\"",
                        ObjectCode = ld.ProductionStepLinkDataCode,
                        ObjectId = ld.ProductionStepLinkDataId,
                        GroupName = EnumProductionProcessWarningCode.WarningProductionLinkData.GetEnumDescription(),
                        WarningCode = EnumProductionProcessWarningCode.WarningProductionLinkData,

                    });
            }

            /*
             * Kiểm tra bán thành phẩm
             */
            var productionStepLinkDataCodes = productionProcess.ProductionStepLinkDataRoles.GroupBy(x => x.ProductionStepLinkDataCode)
                .Where(x => x.Count() == 1 && x.First().ProductionStepLinkDataRoleTypeId == EnumProductionStepLinkDataRoleType.Output)
                .Select(x => x.Key)
                .ToArray();

            var productionStepLinkDatas = productionProcess.ProductionStepLinkDatas
                .Where(x => productionStepLinkDataCodes.Contains(x.ProductionStepLinkDataCode)
                    && x.ObjectTypeId == EnumProductionStepLinkDataObjectType.ProductSemi)
                .ToList();
            foreach (var linkData in productionStepLinkDatas)
            {
                lsWarning.Add(new ProductionProcessWarningMessage
                {
                    Message = $"Bán thành phẩm \"{linkData.ObjectTitle}\" không thể nhập về kho.",
                    ObjectCode = linkData.ProductionStepLinkDataCode,
                    ObjectId = linkData.ProductionStepLinkDataId,
                    GroupName = EnumProductionProcessWarningCode.WarningProductionLinkData.GetEnumDescription(),
                    WarningCode = EnumProductionProcessWarningCode.WarningProductionLinkData,
                });
            }

            /*
             * Kiểm tra và báo sự xuất hiện của 2 mặt hàng trên cùng 1 nhánh
             */
            var groupbyLinkDataRole = productionProcess.ProductionStepLinkDataRoles
                .GroupBy(r => r.ProductionStepLinkDataCode)
                .Where(g => g.Count() == 2)
                .ToList();

            var groupbyLinkDataRoleScanned = new List<IGrouping<string, ProductionStepLinkDataRoleInput>>();
            for (int i = 0; i < groupbyLinkDataRole.Count; i++)
            {
                var role = groupbyLinkDataRole[i];
                if (groupbyLinkDataRoleScanned.Contains(role))
                    continue;

                groupbyLinkDataRoleScanned.Add(role);
                var lsProductionStepIdInGroup = new List<string>();
                foreach (var linkData in role)
                {
                    if (lsProductionStepIdInGroup.Contains(linkData.ProductionStepCode))
                        continue;
                    lsProductionStepIdInGroup.Add(linkData.ProductionStepCode);
                    var temp = groupbyLinkDataRole.Where(x => x.Key != role.Key && x.Where(y => y.ProductionStepCode == linkData.ProductionStepCode).Count() > 0).ToList();
                    TraceProductionStepRelationship(temp, groupbyLinkDataRoleScanned, groupbyLinkDataRole, lsProductionStepIdInGroup);
                }

                var productionStepLinkData = from l in productionProcess.ProductionStepLinkDatas
                                             join r in productionProcess.ProductionStepLinkDataRoles
                                                on l.ProductionStepLinkDataCode equals r.ProductionStepLinkDataCode
                                             where lsProductionStepIdInGroup.Contains(r.ProductionStepCode) && l.ObjectTypeId == EnumProductionStepLinkDataObjectType.Product && r.ProductionStepLinkDataRoleTypeId == EnumProductionStepLinkDataRoleType.Output
                                             select l;
                var productionLinkDataDuplicate = productionStepLinkData
                                                .GroupBy(x => x.ObjectId)
                                                .Where(x => x.Count() > 1)
                                                .SelectMany(x => x)
                                                .ToList();
                if (productionLinkDataDuplicate.Count > 0)
                {
                    foreach (var linkData in productionLinkDataDuplicate)
                    {
                        var currentRole = productionProcess.ProductionStepLinkDataRoles
                                       .Where(x => x.ProductionStepLinkDataCode == linkData.ProductionStepLinkDataCode
                                        && x.ProductionStepLinkDataRoleTypeId == EnumProductionStepLinkDataRoleType.Input)
                                       .ToList();
                        var warning = SeekingLinkDataInRelationship(productionProcess, currentRole, linkData/*, linkDataInProcess*/);
                        if (warning != null)
                            lsWarning.Add(warning);
                    }
                }
            }

            return Task.FromResult(lsWarning);
        }

        private ProductionProcessWarningMessage SeekingLinkDataInRelationship(ProductionProcessModel req, IList<ProductionStepLinkDataRoleInput> currentRole, ProductionStepLinkDataInput linkData/*, Dictionary<string, ProductionProcess> linkDataInProcess*/)
        {
            foreach (var c in currentRole)
            {
                var productionStepCode = c.ProductionStepCode;
                var OutputInStep = from l in req.ProductionStepLinkDatas
                                   join r in req.ProductionStepLinkDataRoles
                                      on l.ProductionStepLinkDataCode equals r.ProductionStepLinkDataCode
                                   where r.ProductionStepCode == productionStepCode && l.ObjectTypeId == EnumProductionStepLinkDataObjectType.Product && r.ProductionStepLinkDataRoleTypeId == EnumProductionStepLinkDataRoleType.Output
                                   select l;
                if (OutputInStep.Select(x => x.ObjectId).Contains(linkData.ObjectId))
                    return new ProductionProcessWarningMessage
                    {
                        Message = $"Xuất hiện nhiều chi tiết \"{linkData.ObjectTitle}\" là đầu ra của các công đoạn có quan hệ với nhau.",
                        ObjectCode = linkData.ProductionStepLinkDataCode,
                        ObjectId = linkData.ProductionStepLinkDataId,
                        GroupName = EnumProductionProcessWarningCode.WarningProductionLinkData.GetEnumDescription(),
                        WarningCode = EnumProductionProcessWarningCode.WarningProductionLinkData,

                    };

                foreach (var output in OutputInStep)
                {
                    var nextRole = req.ProductionStepLinkDataRoles
                                       .Where(x => x.ProductionStepLinkDataCode == output.ProductionStepLinkDataCode
                                        && x.ProductionStepLinkDataRoleTypeId == EnumProductionStepLinkDataRoleType.Input)
                                       .ToList();
                    return SeekingLinkDataInRelationship(req, nextRole, linkData);
                }
            }
            return null;
        }

        private void TraceProductionStepRelationship(List<IGrouping<string, ProductionStepLinkDataRoleInput>> groupbyLinkDataRole
            , List<IGrouping<string, ProductionStepLinkDataRoleInput>> groupbyLinkDataRoleScanned
            , List<IGrouping<string, ProductionStepLinkDataRoleInput>> groupbyLinkDataRoleOrigin
            , List<string> lsProductionStepIdInGroup)
        {
            foreach (var role in groupbyLinkDataRole)
            {
                if (groupbyLinkDataRoleScanned.Contains(role))
                    continue;
                groupbyLinkDataRoleScanned.Add(role);
                foreach (var linkData in role)
                {
                    if (lsProductionStepIdInGroup.Contains(linkData.ProductionStepCode))
                        continue;
                    lsProductionStepIdInGroup.Add(linkData.ProductionStepCode);

                    var temp = groupbyLinkDataRoleOrigin.Where(x => x.Where(y => y.ProductionStepId == linkData.ProductionStepId).Count() > 0).ToList();
                    TraceProductionStepRelationship(temp, groupbyLinkDataRoleScanned, groupbyLinkDataRoleOrigin, lsProductionStepIdInGroup);
                }
                groupbyLinkDataRoleOrigin.Remove(role);
            }
        }

        private IList<string> FoundProductionStepInOutsourceStepRequest(IList<OutsourceStepRequestDataModel> outsourceStepRequestDatas, List<ProductionStepLinkDataRoleInput> roles)
        {
            var outputData = outsourceStepRequestDatas
                .Where(x => x.ProductionStepLinkDataRoleTypeId == EnumProductionStepLinkDataRoleType.Output)
                .Select(x => x.ProductionStepLinkDataId)
                .ToList();

            var inputData = outsourceStepRequestDatas
                .Where(x => x.ProductionStepLinkDataRoleTypeId == EnumProductionStepLinkDataRoleType.Input)
                .Select(x => x.ProductionStepLinkDataId)
                .ToList();

            var productionStepStartCode = roles.Where(x => inputData.Contains(x.ProductionStepLinkDataId)
                   && x.ProductionStepLinkDataRoleTypeId == EnumProductionStepLinkDataRoleType.Input)
                .Select(x => x.ProductionStepCode)
                .Distinct()
                .ToList();
            var productionStepEndCode = roles.Where(x => outputData.Contains(x.ProductionStepLinkDataId)
                     && x.ProductionStepLinkDataRoleTypeId == EnumProductionStepLinkDataRoleType.Output)
                .Select(x => x.ProductionStepCode)
                .Distinct()
                .ToList();

            var lsProductionStepCCode = new List<string>();
            foreach (var id in productionStepEndCode)
                FindTraceProductionStep(inputData, roles, productionStepStartCode, lsProductionStepCCode, id);

            return lsProductionStepCCode
                    .Union(productionStepEndCode)
                    .Union(productionStepStartCode)
                    .Distinct()
                    .ToList();
        }

        private void FindTraceProductionStep(List<long> inputLinkData, List<ProductionStepLinkDataRoleInput> roles, List<string> productionStepStartId, List<string> result, string productionStepCode)
        {
            var roleInput = roles.Where(x => x.ProductionStepCode == productionStepCode
                    && !inputLinkData.Contains(x.ProductionStepLinkDataId)
                    && x.ProductionStepLinkDataRoleTypeId == EnumProductionStepLinkDataRoleType.Input)
                .ToList();
            foreach (var input in roleInput)
            {
                var roleOutput = roles.Where(x => x.ProductionStepLinkDataId == input.ProductionStepLinkDataId
                        && x.ProductionStepLinkDataRoleTypeId == EnumProductionStepLinkDataRoleType.Output)
                    .FirstOrDefault();

                if (roleOutput == null) continue;

                result.Add(roleOutput.ProductionStepCode);
                FindTraceProductionStep(inputLinkData, roles, productionStepStartId, result, roleOutput.ProductionStepCode);
            }
        }

        private async Task<IList<OutsourcePartRequestDetailInfo>> GetOutsourcePartRequestDetailInfo(long productionOrderId)
        {
            var parammeters = new List<SqlParameter>();
            var sql = new StringBuilder($"SELECT * FROM vOutsourcePartRequestExtractInfo v Where v.ProductionOrderId = {productionOrderId}");
            var resultData = await _manufacturingDBContext.QueryDataTable(sql.ToString(), parammeters.Select(p => p.CloneSqlParam()).ToArray());
            var lst = resultData.ConvertData<OutsourcePartRequestDetailExtractInfo>()
               .AsQueryable()
               .ProjectTo<OutsourcePartRequestDetailInfo>(_mapper.ConfigurationProvider)
               .ToList();

            return lst;
        }
    }

}
