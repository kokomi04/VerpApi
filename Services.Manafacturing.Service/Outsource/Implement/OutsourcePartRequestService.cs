using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.SqlServer.Management.SqlParser.SqlCodeDom;
using OpenXmlPowerTools;
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
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Manafacturing.Model.Outsource.RequestPart;
using VErp.Services.Manafacturing.Model.ProductionStep;
using static VErp.Commons.Enums.Manafacturing.EnumProductionProcess;

namespace VErp.Services.Manafacturing.Service.Outsource.Implement
{
    public class OutsourcePartRequestService : IOutsourcePartRequestService
    {
        private readonly ManufacturingDBContext _manufacturingDBContext;
        private readonly IActivityLogService _activityLogService;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly ICustomGenCodeHelperService _customGenCodeHelperService;

        public OutsourcePartRequestService(ManufacturingDBContext manufacturingDB
            , IActivityLogService activityLogService
            , ILogger<OutsourcePartRequestService> logger
            , IMapper mapper
            , ICustomGenCodeHelperService customGenCodeHelperService)
        {
            _manufacturingDBContext = manufacturingDB;
            _activityLogService = activityLogService;
            _logger = logger;
            _mapper = mapper;
            _customGenCodeHelperService = customGenCodeHelperService;
        }

        public async Task<long> CreateOutsourcePartRequest(OutsourcePartRequestInfo req)
        {
            using var trans = await _manufacturingDBContext.Database.BeginTransactionAsync();
            try
            {
                // Get cấu hình sinh mã
                int customGenCodeId = 0;
                var currentConfig = await _customGenCodeHelperService.CurrentConfig(EnumObjectType.OutsourceRequest, EnumObjectType.OutsourceRequest, 0, null, req.OutsourcePartRequestCode, req.OutsourcePartRequestDate);

                if (currentConfig == null)
                {
                    throw new BadRequestException(GeneralCode.ItemNotFound, "Chưa thiết định cấu hình sinh mã");
                }
                var generated = await _customGenCodeHelperService.GenerateCode(currentConfig.CustomGenCodeId, currentConfig.CurrentLastValue.LastValue, null, req.OutsourcePartRequestCode, req.OutsourcePartRequestDate);
                if (generated == null)
                {
                    throw new BadRequestException(GeneralCode.InternalError, "Không thể sinh mã ");
                }
                customGenCodeId = currentConfig.CustomGenCodeId;

                // Create order
                var order = _mapper.Map<OutsourcePartRequest>(req as OutsourcePartRequestModel );
                order.OutsourcePartRequestCode = generated.CustomCode;

                _manufacturingDBContext.OutsourcePartRequest.Add(order);
                await _manufacturingDBContext.SaveChangesAsync();

                // Create order detail
                var orderDetails = new List<OutsourcePartRequestDetail>();
                foreach (var data in req.OutsourcePartRequestDetail)
                {
                    data.OutsourcePartRequestId = order.OutsourcePartRequestId;
                    var entity = _mapper.Map<OutsourcePartRequestDetail>(data as RequestOutsourcePartDetailModel);
                    orderDetails.Add(entity);
                    await UpdateProductionStepLinkDataRelative(order.ProductionOrderDetailId, entity.PathProductIdInBom, entity.ProductId, entity.Quantity);
                }

                await _manufacturingDBContext.OutsourcePartRequestDetail.AddRangeAsync(orderDetails);
                await _manufacturingDBContext.SaveChangesAsync();

                if (customGenCodeId > 0)
                {
                    await _customGenCodeHelperService.ConfirmCode(currentConfig.CurrentLastValue);
                }

                trans.Commit();
                await _activityLogService.CreateLog(EnumObjectType.ProductionOrder, order.OutsourcePartRequestId, $"Thêm mới yêu cầu gia công chi tiết {order.OutsourcePartRequestId}", order.JsonSerialize());

                return order.OutsourcePartRequestId;
            }
            catch (Exception ex)
            {
                trans.TryRollbackTransaction();
                _logger.LogError(ex, "CreateRequestOutsourcePart");
                throw;
            }
        }

        public async Task<OutsourcePartRequestInfo> GetOutsourcePartRequestExtraInfo(int OutsourcePartRequestId = 0)
        {
            var sql = new StringBuilder("SELECT * FROM vOutsourcePartRequestExtractInfo v WHERE v.OutsourcePartRequestId = @OutsourcePartRequestId");

            var parammeters = new List<SqlParameter>();
            parammeters.Add(new SqlParameter("@OutsourcePartRequestId", OutsourcePartRequestId));

            var extractInfo = (await _manufacturingDBContext.QueryDataTable(sql.ToString(), parammeters.Select(p => p.CloneSqlParam()).ToArray()))
                    .ConvertData<OutsourcePartRequestDetailInfo>();
            if (extractInfo.Count == 0)
                throw new BadRequestException(OutsourceErrorCode.NotFoundRequest);

            var rs = _mapper.Map<OutsourcePartRequestInfo>(extractInfo[0]);
            rs.OutsourcePartRequestStatus = GetOutsourcePartRequestStatus(extractInfo);
            rs.OutsourcePartRequestDetail = extractInfo.Where(x => x.OutsourcePartRequestDetailId > 0).ToList();
            return rs;
        }

        private string GetOutsourcePartRequestStatus(List<OutsourcePartRequestDetailInfo> req)
        {
            var sumStatus = req.Sum(x => (int)x.OutsourcePartRequestDetailStatusId);
            if (sumStatus == ((int)EnumOutsourceRequestStatusType.Unprocessed * req.Count))
                return EnumOutsourceRequestStatusType.Unprocessed.GetEnumDescription();
            else if (sumStatus == ((int)EnumOutsourceRequestStatusType.Processed * req.Count))
                return EnumOutsourceRequestStatusType.Processed.GetEnumDescription();
            else
                return EnumOutsourceRequestStatusType.Processing.GetEnumDescription();
        }

        public async Task<bool> UpdateOutsourcePartRequest(int OutsourcePartRequestId, OutsourcePartRequestInfo req)
        {
            var order = await _manufacturingDBContext.OutsourcePartRequest.FirstOrDefaultAsync(x => x.OutsourcePartRequestId == OutsourcePartRequestId);
            if (order == null)
                throw new BadRequestException(OutsourceErrorCode.NotFoundRequest, $"Không tìm thấy yêu cầu gia công có mã là {OutsourcePartRequestId}");

            var details = _manufacturingDBContext.OutsourcePartRequestDetail.Where(x => x.OutsourcePartRequestId == OutsourcePartRequestId).ToList();
            using var trans = await _manufacturingDBContext.Database.BeginTransactionAsync();
            try
            {
                // update order
                _mapper.Map(req, order);

                //Valid Update and action
                foreach (var u in details)
                {
                    var s = req.OutsourcePartRequestDetail.FirstOrDefault(x => x.OutsourcePartRequestDetailId == u.OutsourcePartRequestDetailId);
                    if (s != null)
                    {
                        await UpdateProductionStepLinkDataRelative(order.ProductionOrderDetailId, u.PathProductIdInBom, u.ProductId, s.Quantity, u.Quantity);
                        _mapper.Map(s, u);

                    }
                    else
                        u.IsDeleted = true;
                }

                // create new detail
                var lsNewDetail = req.OutsourcePartRequestDetail.Where(x => !details.Select(x => x.OutsourcePartRequestDetailId).Contains(x.OutsourcePartRequestDetailId)).ToList();
                var temp = _mapper.Map<List<OutsourcePartRequestDetail>>(lsNewDetail);
                temp.ForEach(x => x.OutsourcePartRequestId = order.OutsourcePartRequestId);
                await _manufacturingDBContext.OutsourcePartRequestDetail.AddRangeAsync(temp);

                await _manufacturingDBContext.SaveChangesAsync();
                trans.Commit();

                await _activityLogService.CreateLog(EnumObjectType.OutsourceRequest, req.OutsourcePartRequestId, $"Cập nhật yêu cầu gia công chi tiết {req.OutsourcePartRequestId}", req.JsonSerialize());
                return true;
            }
            catch (Exception ex)
            {
                trans.TryRollbackTransaction();
                _logger.LogError(ex, "UpdateRequestOutsourcePart");
                throw;
            }
        }

        public async Task<PageData<OutsourcePartRequestDetailInfo>> GetListOutsourcePartRequest(string keyword, int page, int size, Clause filters = null)
        {
            keyword = (keyword ?? "").Trim();
            var parammeters = new List<SqlParameter>();
            var whereCondition = new StringBuilder();
            if (!string.IsNullOrEmpty(keyword))
            {
                whereCondition.Append("(v.ProductionOrderCode LIKE @KeyWord ");
                whereCondition.Append("OR v.ProductCode LIKE @Keyword ");
                whereCondition.Append("OR v.ProductName LIKE @Keyword ");
                whereCondition.Append("OR v.RequestOutsourcePartCode LIKE @Keyword ");
                whereCondition.Append("OR v.ProductPartName LIKE @Keyword ) ");
                parammeters.Add(new SqlParameter("@Keyword", $"%{keyword}%"));
            }
            if (filters != null)
            {
                var suffix = 0;
                var filterCondition = new StringBuilder();
                filters.FilterClauseProcess("vOutsourcePartRequestExtractInfo", "v", ref filterCondition, ref parammeters, ref suffix);
                if (filterCondition.Length > 2)
                {
                    if (whereCondition.Length > 0) whereCondition.Append(" AND ");
                    whereCondition.Append(filterCondition);
                }
            }

            var sql = new StringBuilder("SELECT * FROM vOutsourcePartRequestExtractInfo v ");
            var totalSql = new StringBuilder("SELECT COUNT(v.OutsourcePartRequestDetailId) Total FROM vOutsourcePartRequestExtractInfo v ");
            if (whereCondition.Length > 0)
            {
                totalSql.Append("WHERE ");
                totalSql.Append(whereCondition);
                sql.Append("WHERE ");
                sql.Append(whereCondition);
            }

            sql.Append($" ORDER BY v.OutsourcePartRequestId");

            var table = await _manufacturingDBContext.QueryDataTable(totalSql.ToString(), parammeters.ToArray());

            var total = 0;
            if (table != null && table.Rows.Count > 0)
            {
                total = (table.Rows[0]["Total"] as int?).GetValueOrDefault();
            }

            if (size >= 0)
            {
                sql.Append(@$" OFFSET {(page - 1) * size} ROWS
                FETCH NEXT { size}
                ROWS ONLY");
            }

            var resultData = await _manufacturingDBContext.QueryDataTable(sql.ToString(), parammeters.Select(p => p.CloneSqlParam()).ToArray());
            var lst = resultData.ConvertData<OutsourcePartRequestDetailInfo>().ToList();

            return (lst, total);
        }

        public async Task<bool> DeletedOutsourcePartRequest(int OutsourcePartRequestId)
        {
            var trans = await _manufacturingDBContext.Database.BeginTransactionAsync();
            try
            {
                var order = await _manufacturingDBContext.OutsourcePartRequest.FirstOrDefaultAsync(x => x.OutsourcePartRequestId == OutsourcePartRequestId);
                if (order == null)
                    throw new BadRequestException(OutsourceErrorCode.NotFoundRequest);
                var details = await _manufacturingDBContext.OutsourcePartRequestDetail
                    .Where(x => x.OutsourcePartRequestId == order.OutsourcePartRequestId)
                    .ToListAsync();

                var lst = (from o in _manufacturingDBContext.OutsourceOrder
                           join d in _manufacturingDBContext.OutsourceOrderDetail
                             on o.OutsourceOrderId equals d.OutsourceOrderId
                           where o.OutsourceTypeId == (int)EnumOutsourceOrderType.OutsourcePart
                           select d).GroupBy(x => x.ObjectId).Select(x => new
                           {
                               ObjectId = x.Key,
                               QuantityProcessed = x.Sum(x => x.Quantity)
                           });
                foreach (var detail in details)
                {
                    if (lst.Where(y => y.ObjectId == detail.OutsourcePartRequestDetailId && y.QuantityProcessed > 0).Count() != 0)
                        throw new BadRequestException(OutsourceErrorCode.InValidRequestOutsource, $"Đã có đơn hàng gia công cho yêu cầu {order.OutsourcePartRequestCode}");
                    await UpdateProductionStepLinkDataRelative(order.ProductionOrderDetailId, detail.PathProductIdInBom, detail.ProductId, 0, detail.Quantity);
                    detail.IsDeleted = true;
                };
                order.IsDeleted = true;

                await _manufacturingDBContext.SaveChangesAsync();


                await trans.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await trans.TryRollbackTransactionAsync();
                _logger.LogError(ex, "DeletedOutsourcePartRequest");
                throw;
            }


        }

        public async Task<IList<OutsourcePartRequestOutput>> GetOutsourcePartRequestByProductionOrderId(long productionOrderId)
        {
            var data = await _manufacturingDBContext.OutsourcePartRequest.AsNoTracking()
                                .Include(x => x.ProductionOrderDetail)
                                .Where(x => x.ProductionOrderDetail.ProductionOrderId == productionOrderId)
                                .ProjectTo<OutsourcePartRequestOutput>(_mapper.ConfigurationProvider)
                                .ToListAsync();
            return data;
        }

        private async Task<bool> UpdateProductionStepLinkDataRelative(long productionOrderDetailId, string pathProductiIdBom, long productId, decimal newQuantity, decimal oldQuantity = 0)
        {
            var listProductionStep = await _manufacturingDBContext.ProductionStepOrder.AsNoTracking()
                                            .Where(x => x.ProductionOrderDetailId == productionOrderDetailId)
                                            .Select(x => x.ProductionStep)
                                            .ProjectTo<ProductionStepInfo>(_mapper.ConfigurationProvider)
                                            .ToListAsync();
            var pathProductId = Array.ConvertAll(pathProductiIdBom.Split(','), s => long.Parse(s));
            var productionSteps = listProductionStep
                                    .Where(x => x.ProductionStepLinkDatas.Any(x => x.ObjectId == productId
                                                && x.ObjectTypeId == ProductionStepLinkDataObjectType.Product
                                                && x.ProductionStepLinkDataRoleTypeId == EnumProductionStepLinkDataRoleType.Output)
                                    )
                                    .ToList();
            var dicProductionStep = new Dictionary<long, List<ProductionStepLinkDataInfo>>();
            var productionStepLinkDatas = new List<ProductionStepLinkDataInfo>();
            foreach (var productionStep in productionSteps)
            {
                var index = 0;
                var linkDataOutputs = productionStep.ProductionStepLinkDatas
                                                    .Where(x => x.ObjectId == productId
                                                            && x.ObjectTypeId == ProductionStepLinkDataObjectType.Product
                                                            && x.ProductionStepLinkDataRoleTypeId == EnumProductionStepLinkDataRoleType.Output)
                                                    .ToList();
                foreach (var linkDataOutput in linkDataOutputs)
                {
                    if (CheckLinkDataPosterior(listProductionStep, linkDataOutput, pathProductId, ref index))
                    {

                        if (!dicProductionStep.ContainsKey(productionStep.ProductionStepId))
                        {
                            var temp = new List<ProductionStepLinkDataInfo>();
                            temp.Add(linkDataOutput);
                            dicProductionStep.Add(productionStep.ProductionStepId, temp);
                        }
                        else
                        {
                            dicProductionStep[productionStep.ProductionStepId].Add(linkDataOutput);
                        }

                    }
                }

            }
            if (productionSteps.Count == 0) return true;

            var totalQuantityOrigin = dicProductionStep.Values.SelectMany(x => x).Sum(x => x.Quantity);
            var oldPercent = (oldQuantity / totalQuantityOrigin) / dicProductionStep.Values.SelectMany(x => x).Count();
            var newPercent = (newQuantity / totalQuantityOrigin) / dicProductionStep.Values.SelectMany(x => x).Count();
            foreach (var dic in dicProductionStep)
            {
                dic.Value.ForEach(x =>
                {
                    var oldValue = x.Quantity * oldPercent;
                    var newValue = x.Quantity * newPercent;
                    x.OutsourceQuantity += (newValue - oldValue);
                });
                productionStepLinkDatas.AddRange(dic.Value);

                var productionStep = productionSteps.FirstOrDefault(x => x.ProductionStepId == dic.Key);
                var linkDataInputs = productionStep.ProductionStepLinkDatas
                                                .Where(x => x.ProductionStepLinkDataRoleTypeId == EnumProductionStepLinkDataRoleType.Input)
                                                .ToList();
                var localOldPercent = oldPercent * dic.Value.Count;
                var localNewPercent = newPercent * dic.Value.Count;
                FoundLinkDataOutPutPrevious(listProductionStep, linkDataInputs, productionStepLinkDatas, localNewPercent, localOldPercent);
            }

            var productionLinkDataModel = await _manufacturingDBContext.ProductionStepLinkData
                .Where(x => productionStepLinkDatas.Select(x => x.ProductionStepLinkDataId).Contains(x.ProductionStepLinkDataId))
                .ToListAsync();
            foreach (var model in productionLinkDataModel)
            {
                var dto = productionStepLinkDatas.FirstOrDefault(x => x.ProductionStepLinkDataId == model.ProductionStepLinkDataId);
                if (dto != null)
                    _mapper.Map(dto, model);
            }

            await _manufacturingDBContext.SaveChangesAsync();
            return true;
        }

        private void FoundLinkDataOutPutPrevious(IList<ProductionStepInfo> listProductionStep, IList<ProductionStepLinkDataInfo> linkDataInputs, List<ProductionStepLinkDataInfo> productionStepLinkDatas, decimal newPercent, decimal oldPercent = 0)
        {
            foreach (var linkDataInput in linkDataInputs)
            {
                if (productionStepLinkDatas.Any(x => x.ProductionStepLinkDataId == linkDataInput.ProductionStepLinkDataId))
                {
                    productionStepLinkDatas.ForEach(x =>
                    {
                        if (x.ProductionStepLinkDataId == linkDataInput.ProductionStepLinkDataId)
                        {
                            var oldValue = linkDataInput.Quantity * oldPercent;
                            var newValue = linkDataInput.Quantity * newPercent;
                            x.OutsourceQuantity += (newValue - oldValue);
                        }
                    });
                }
                else
                {
                    var oldValue = linkDataInput.Quantity * oldPercent;
                    var newValue = linkDataInput.Quantity * newPercent;
                    linkDataInput.OutsourceQuantity += (newValue - oldValue);
                    productionStepLinkDatas.Add(linkDataInput);
                }

                var productionStep = listProductionStep
                                             .Where(x => x.ProductionStepLinkDatas.Any(x => x.ProductionStepLinkDataId == linkDataInput.ProductionStepLinkDataId
                                                         && x.ProductionStepLinkDataRoleTypeId == EnumProductionStepLinkDataRoleType.Output)
                                             )
                                             .FirstOrDefault();
                if (productionStep == null) return;

                var linkDataInputNexts = productionStep.ProductionStepLinkDatas
                                                            .Where(x => x.ProductionStepLinkDataRoleTypeId == EnumProductionStepLinkDataRoleType.Input)
                                                            .ToList();
                FoundLinkDataOutPutPrevious(listProductionStep, linkDataInputNexts, productionStepLinkDatas, newPercent, oldPercent);
            }
        }

        private bool CheckLinkDataPosterior(IList<ProductionStepInfo> listProductionStep, ProductionStepLinkDataInfo linkDataOutput, long[] pathProductId, ref int index)
        {
            if (pathProductId.Contains(linkDataOutput.ObjectId))
                index++;
            if (index == pathProductId.Length)
                return true;
            var productionStep = listProductionStep
                                    .Where(x => x.ProductionStepLinkDatas.Any(x => x.ProductionStepLinkDataId == linkDataOutput.ProductionStepLinkDataId
                                                && x.ProductionStepLinkDataRoleTypeId == EnumProductionStepLinkDataRoleType.Input)
                                    )
                                    .FirstOrDefault();
            var linkDataOutputAfter = productionStep.ProductionStepLinkDatas
                                                        .Where(x => x.ProductionStepLinkDataRoleTypeId == EnumProductionStepLinkDataRoleType.Output)
                                                        .FirstOrDefault(); ;
            if (linkDataOutputAfter == null)
                return false;

            return CheckLinkDataPosterior(listProductionStep, linkDataOutputAfter, pathProductId, ref index);
        }
    }
}
