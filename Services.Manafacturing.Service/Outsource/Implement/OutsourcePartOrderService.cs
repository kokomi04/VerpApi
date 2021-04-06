using AutoMapper;
using AutoMapper.QueryableExtensions;
using DocumentFormat.OpenXml.Office.CustomUI;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenXmlPowerTools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.ErrorCodes;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.ManufacturingDB;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Manafacturing.Model.Outsource.Order;
using VErp.Services.Manafacturing.Model.Outsource.RequestPart;
using VErp.Services.Manafacturing.Model.Outsource.Track;
using VErp.Services.Manafacturing.Model.ProductionStep;
using static VErp.Commons.Enums.Manafacturing.EnumOutsourceTrack;
using static VErp.Commons.Enums.Manafacturing.EnumProductionProcess;

namespace VErp.Services.Manafacturing.Service.Outsource.Implement
{
    public class OutsourcePartOrderService : IOutsourcePartOrderService
    {
        private readonly ManufacturingDBContext _manufacturingDBContext;
        private readonly IActivityLogService _activityLogService;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly ICustomGenCodeHelperService _customGenCodeHelperService;
        private readonly IOutsourceTrackService _outsourceTrackService;
        private readonly IOutsourcePartRequestService _outsourcePartRequestService;

        public OutsourcePartOrderService(ManufacturingDBContext manufacturingDB
            , IActivityLogService activityLogService
            , ILogger<OutsourcePartOrderService> logger
            , IMapper mapper
            , ICustomGenCodeHelperService customGenCodeHelperService
            , IOutsourceTrackService outsourceTrackService
            , IOutsourcePartRequestService outsourcePartRequestService)
        {
            _manufacturingDBContext = manufacturingDB;
            _activityLogService = activityLogService;
            _logger = logger;
            _mapper = mapper;
            _customGenCodeHelperService = customGenCodeHelperService;
            _outsourceTrackService = outsourceTrackService;
            _outsourcePartRequestService = outsourcePartRequestService;
        }

        public async Task<long> CreateOutsourceOrderPart(OutsourceOrderInfo req)
        {
            await CheckMarkInvalidOutsourcePartRequest(req.OutsourceOrderDetail.Select(x => x.ObjectId).ToArray());

            using (var trans = _manufacturingDBContext.Database.BeginTransaction())
            {
                try
                {
                    CustomGenCodeOutputModel currentConfig = null;
                    string outsoureOrderCode = "";
                    if (string.IsNullOrWhiteSpace(req.OutsourceOrderCode))
                    {
                        currentConfig = await _customGenCodeHelperService.CurrentConfig(EnumObjectType.OutsourceOrder, EnumObjectType.OutsourceOrder, 0, null, req.OutsourceOrderCode, req.OutsourceOrderDate);
                        if (currentConfig == null)
                        {
                            throw new BadRequestException(GeneralCode.ItemNotFound, "Chưa thiết định cấu hình sinh mã");
                        }
                        var generated = await _customGenCodeHelperService.GenerateCode(currentConfig.CustomGenCodeId, currentConfig.CurrentLastValue.LastValue, null, req.OutsourceOrderCode, req.OutsourceOrderDate);
                        if (generated == null)
                        {
                            throw new BadRequestException(GeneralCode.InternalError, "Không thể sinh mã ");
                        }

                        outsoureOrderCode = generated.CustomCode;
                    }
                    else
                    {
                        // Validate unique
                        if (_manufacturingDBContext.OutsourceOrder.Any(o => o.OutsourceOrderCode == req.OutsourceOrderCode))
                            throw new BadRequestException(OutsourceErrorCode.OutsoureOrderCodeAlreadyExisted);
                    }
                    if (!req.OutsourceOrderDate.HasValue)
                    {
                        req.OutsourceOrderDate = DateTime.UtcNow.GetUnix();
                    }

                    var order = _mapper.Map<OutsourceOrder>(req as OutsourceOrderModel);
                    order.OutsourceTypeId = (int)EnumOutsourceType.OutsourcePart;
                    order.OutsourceOrderCode = string.IsNullOrWhiteSpace(order.OutsourceOrderCode) ? outsoureOrderCode : order.OutsourceOrderCode;

                    _manufacturingDBContext.OutsourceOrder.Add(order);
                    await _manufacturingDBContext.SaveChangesAsync();

                    var detail = _mapper.Map<List<OutsourceOrderDetail>>(req.OutsourceOrderDetail);
                    detail.ForEach(x => x.OutsourceOrderId = order.OutsourceOrderId);

                    _manufacturingDBContext.OutsourceOrderDetail.AddRange(detail);
                    await _manufacturingDBContext.SaveChangesAsync();


                    if (string.IsNullOrWhiteSpace(req.OutsourceOrderCode))
                    {
                        await _customGenCodeHelperService.ConfirmCode(currentConfig?.CurrentLastValue);
                    }

                    // Tạo lịch sử theo dõi lần đầu
                    await _outsourceTrackService.CreateOutsourceTrack(new OutsourceTrackModel
                    {
                        OutsourceTrackDate = DateTime.Now.GetUnix(),
                        OutsourceTrackDescription = "Tạo đơn hàng",
                        OutsourceTrackStatusId = EnumOutsourceTrackStatus.Created,
                        OutsourceTrackTypeId = EnumOutsourceTrackType.All,
                        OutsourceOrderId = order.OutsourceOrderId
                    });

                    await _manufacturingDBContext.SaveChangesAsync();

                    await UpdateOutsourcePartRequestStatus(detail.Select(x => x.ObjectId));

                    await _activityLogService.CreateLog(EnumObjectType.ProductionOrder, order.OutsourceOrderId, $"Thêm mới đơn hàng gia công chi tiết {order.OutsourceOrderId}", req.JsonSerialize());

                    await trans.CommitAsync();

                    return order.OutsourceOrderId;
                }
                catch (Exception ex)
                {
                    await trans.RollbackAsync();
                    _logger.LogError(ex,"CreateOutsourceOrderPart");
                    throw ;
                }
            }
        }

        public async Task<bool> DeleteOutsourceOrderPart(long outsourceOrderId)
        {
            var trans = await _manufacturingDBContext.Database.BeginTransactionAsync();
            try
            {
                var outsourceOrder = await _manufacturingDBContext.OutsourceOrder.SingleOrDefaultAsync(o => o.OutsourceOrderId == outsourceOrderId);
                if (outsourceOrder == null)
                    throw new BadRequestException(OutsourceErrorCode.NotFoundOutsourceOrder);
                var outsourceOrderDetail = await _manufacturingDBContext.OutsourceOrderDetail.Where(o => o.OutsourceOrderId == outsourceOrderId).ToListAsync();

                outsourceOrder.IsDeleted = true;
                outsourceOrderDetail.ForEach(x => x.IsDeleted = true);

                await _manufacturingDBContext.SaveChangesAsync();

                await UpdateOutsourcePartRequestStatus(outsourceOrderDetail.Select(x => x.ObjectId));

                await _activityLogService.CreateLog(EnumObjectType.ProductionOrder, outsourceOrder.OutsourceOrderId, $"Loại bỏ đơn hàng gia công chi tiết {outsourceOrder.OutsourceOrderId}", outsourceOrder.JsonSerialize());

                await trans.CommitAsync();

                return true;
            }
            catch (Exception ex)
            {
                await trans.RollbackAsync();
                _logger.LogError(ex, "DeleteOutsourceOrderPart");
                throw;
            }
        }

        public async Task<PageData<OutsourcePartOrderDetailInfo>> GetListOutsourceOrderPart(string keyword, int page, int size, Clause filters = null)
        {
            keyword = (keyword ?? "").Trim();
            var parammeters = new List<SqlParameter>();
            var whereCondition = new StringBuilder();
            if (!string.IsNullOrEmpty(keyword))
            {
                whereCondition.Append(" (v.OutsourceOrderCode LIKE @Keyword ");
                whereCondition.Append("OR v.ProductPartName LIKE @Keyword ");
                whereCondition.Append("OR v.ProductTitle LIKE @Keyword ");
                whereCondition.Append("OR v.RequestOutsourcePartCode LIKE @Keyword ");
                whereCondition.Append("OR v.OrderCode LIKE @Keyword ");
                whereCondition.Append("OR v.ProductionOrderCode LIKE @Keyword ) ");
                parammeters.Add(new SqlParameter("@Keyword", $"%{keyword}%"));
            }
            if (filters != null)
            {
                var suffix = 0;
                var filterCondition = new StringBuilder();
                filters.FilterClauseProcess("vOutsourcePartOrderDetailExtractInfo", "v", ref filterCondition, ref parammeters, ref suffix);
                if (filterCondition.Length > 2)
                {
                    if (whereCondition.Length > 0) whereCondition.Append(" AND ");
                    whereCondition.Append(filterCondition);
                }
            }

            var sql = new StringBuilder("SELECT * FROM vOutsourcePartOrderDetailExtractInfo v ");
            var totalSql = new StringBuilder("SELECT COUNT(v.OutsourceOrderDetailId) Total FROM vOutsourcePartOrderDetailExtractInfo v ");
            if (whereCondition.Length > 0)
            {
                totalSql.Append("WHERE ");
                totalSql.Append(whereCondition);
                sql.Append("WHERE ");
                sql.Append(whereCondition);
            }

            sql.Append($" ORDER BY v.OutsourceOrderId");

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
            var lst = resultData.ConvertData<OutsourcePartOrderDetailExtractInfo>().AsQueryable().ProjectTo<OutsourcePartOrderDetailInfo>(_mapper.ConfigurationProvider).ToList();

            return (lst, total);
        }

        public async Task<OutsourceOrderInfo> GetOutsourceOrderPart(long outsourceOrderId)
        {
            var outsourceOrder = await _manufacturingDBContext.OutsourceOrder
                .ProjectTo<OutsourceOrderInfo>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(o => o.OutsourceOrderId == outsourceOrderId);

            if (outsourceOrder == null)
                throw new BadRequestException(OutsourceErrorCode.NotFoundOutsourceOrder);

            var filter = new SingleClause
            {
                DataType = EnumDataType.BigInt,
                FieldName = "OutsourceOrderId",
                Operator = EnumOperator.Equal,
                Value = outsourceOrder.OutsourceOrderId
            };

            var data = (await GetListOutsourceOrderPart(string.Empty, 1, 9999, filter)).List;
            foreach (var item in data)
            {
                var outsourceOrderDetail = _mapper.Map<OutsourceOrderDetailInfo>(item);
                if (outsourceOrderDetail.OutsourceOrderDetailId > 0)
                    outsourceOrder.OutsourceOrderDetail.Add(outsourceOrderDetail);
            }

            return outsourceOrder;
        }

        public async Task<bool> UpdateOutsourceOrderPart(long outsourceOrderId, OutsourceOrderInfo req)
        {
            await CheckMarkInvalidOutsourcePartRequest(req.OutsourceOrderDetail.Select(x => x.ObjectId).ToArray());

            var trans = await _manufacturingDBContext.Database.BeginTransactionAsync();
            try
            {
                var outsourceOrder = await _manufacturingDBContext.OutsourceOrder.SingleOrDefaultAsync(o => o.OutsourceOrderId == outsourceOrderId);
                if (outsourceOrder == null)
                    throw new BadRequestException(OutsourceErrorCode.NotFoundOutsourceOrder);

                var outsourceOrderDetail = await _manufacturingDBContext.OutsourceOrderDetail.Where(o => o.OutsourceOrderId == outsourceOrderId).ToListAsync();

                //Update order
                _mapper.Map(req, outsourceOrder);

                //update detail
                foreach (var u in outsourceOrderDetail)
                {
                    var s = req.OutsourceOrderDetail.FirstOrDefault(x => x.OutsourceOrderDetailId == u.OutsourceOrderDetailId);
                    if (s != null)
                        _mapper.Map(s, u);
                    else
                        u.IsDeleted = true;
                }

                var lsNewDetail = req.OutsourceOrderDetail.Where(x => !outsourceOrderDetail.Select(x => x.OutsourceOrderDetailId).Contains(x.OutsourceOrderDetailId)).ToList();
                lsNewDetail.ForEach(x => x.OutsourceOrderId = outsourceOrder.OutsourceOrderId);
                var temp = _mapper.Map<List<OutsourceOrderDetail>>(lsNewDetail);

                await _manufacturingDBContext.OutsourceOrderDetail.AddRangeAsync(temp);
                await _manufacturingDBContext.SaveChangesAsync();

                var objectIds = outsourceOrderDetail.Select(x => x.ObjectId).ToList();
                objectIds.AddRange(lsNewDetail.Select(x => x.ObjectId));

                await UpdateOutsourcePartRequestStatus(objectIds);

                await _activityLogService.CreateLog(EnumObjectType.ProductionOrder, outsourceOrder.OutsourceOrderId, $"Cập nhật đơn hàng gia công chi tiết {outsourceOrder.OutsourceOrderId}", outsourceOrder.JsonSerialize());

                await trans.CommitAsync();

                return true;
            }
            catch (Exception ex)
            {
                await trans.RollbackAsync();
                _logger.LogError(ex, "UpdateOutsourceOrderPart");
                throw;
            }
        }

        public async Task<IList<Model.Outsource.Order.OutsourceOrderMaterials>> GetMaterials(long outsourceOrderId)
        {
            var results = new List<Model.Outsource.Order.OutsourceOrderMaterials>();
            var roleMaps = new Dictionary<long, IList<ProductionStepLinkDataRole>>();

            var outsourceOrder = await _manufacturingDBContext.OutsourceOrder.AsNoTracking()
                .Include(x => x.OutsourceOrderDetail)
                .FirstOrDefaultAsync(o => o.OutsourceOrderId == outsourceOrderId);

            if (outsourceOrder == null)
                throw new BadRequestException(OutsourceErrorCode.NotFoundOutsourceOrder);

            var requestMap = (await _manufacturingDBContext.OutsourcePartRequestDetail.AsNoTracking()
                .Where(x => outsourceOrder.OutsourceOrderDetail.Select(x => x.ObjectId).Contains(x.OutsourcePartRequestDetailId))
                .ToListAsync())
                .GroupBy(x => x.OutsourcePartRequestId);

            foreach (var rq in requestMap)
            {
                var sqlRequest = new StringBuilder($"SELECT * FROM vOutsourcePartRequestExtractInfo v WHERE v.OutsourcePartRequestId = {rq.Key}");

                var request = (await _manufacturingDBContext.QueryDataTable(sqlRequest.ToString(), Array.Empty<SqlParameter>()))
                     .ConvertData<OutsourcePartRequestDetailExtractInfo>()
                    .FirstOrDefault();

                if (request == null)
                    throw new BadRequestException(OutsourceErrorCode.NotFoundRequest);

                if (request.MarkInvalid)
                    throw new BadRequestException(OutsourceErrorCode.InValidRequestOutsource, $"YCGC \"{request.OutsourcePartRequestCode}\" chưa được xác thực với QTSX");

                var productionOrderId = request.ProductionOrderId;
                IList<ProductionStepLinkDataRole> role;

                // lấy role QTSX
                if (!roleMaps.ContainsKey(productionOrderId))
                {
                    role = await _manufacturingDBContext.ProductionStepLinkDataRole.AsNoTracking()
                        .Include(x => x.ProductionStep)
                        .Include(x => x.ProductionStepLinkData)
                        .Where(x => x.ProductionStep.ContainerId == productionOrderId && x.ProductionStep.ContainerTypeId == (int)EnumContainerType.ProductionOrder)
                        .ToArrayAsync();
                    roleMaps.Add(productionOrderId, role);
                }
                else
                {
                    role = roleMaps[productionOrderId];
                }

                // Tính toán steplink
                var productionStepLinks = CalcProdictonStepLink(role);

                var linkDataMap = role.Where(x => x.ProductionStepLinkDataRoleTypeId == (int)EnumProductionStepLinkDataRoleType.Input)
                    .Select(x => x.ProductionStepLinkData)
                    .Where(x => rq.Select(r => r.OutsourcePartRequestDetailId).Contains(x.OutsourceRequestDetailId ?? 0))
                    .GroupBy(g => g.OutsourceRequestDetailId);

                // Tìm các công đoạn đầu tiên
                var requestDetailMap = new Dictionary<long, Dictionary<long, Dictionary<long, IList<long>>>>(); 
                foreach(var map in linkDataMap)
                {
                    var d_1 =  new Dictionary<long, Dictionary<long, IList<long>>>();
                    foreach (var ld in map)
                    {
                        var productionStepId = role.FirstOrDefault(x => x.ProductionStepLinkDataId == ld.ProductionStepLinkDataId)?.ProductionStepId;
                        
                        var linkDataOrigin = role.Where(x => x.ProductionStepId == productionStepId 
                                                            && x.ProductionStepLinkData.ObjectId == ld.ObjectId 
                                                            && x.ProductionStepLinkData.ProductionStepLinkDataId != ld.ProductionStepLinkDataId)
                                                 .Select(x => x.ProductionStepLinkDataId)
                                                 .ToList();
                        var d_2 = new Dictionary<long, IList<long>>();

                        foreach (var ldOriginId in linkDataOrigin)
                        {
                            var t_1 = new List<long>();

                            productionStepId = role.FirstOrDefault(x => x.ProductionStepLinkDataId == ldOriginId
                             && x.ProductionStepLinkDataRoleTypeId == (int)EnumProductionStepLinkDataRoleType.Output)?.ProductionStepId;
                            if (!productionStepId.HasValue)
                                continue;

                            var stepLinks = productionStepLinks.Where(x => x.ToStepId == productionStepId);
                            if(stepLinks.Count() == 0)
                            {
                                t_1.Add(productionStepId.GetValueOrDefault());
                            }
                            foreach (var sl in stepLinks)
                            {
                                var s_id = sl.FromStepId;
                                t_1.AddRange(TracedStepStoreMaterials(s_id, productionStepLinks));
                            }
                            if (t_1.Count > 0 && !d_2.ContainsKey(ldOriginId))
                            {
                                d_2.Add(ldOriginId, t_1);
                            }
                        }
                        if(d_2.Count > 0)
                            d_1.Add(ld.ProductionStepLinkDataId, d_2);
                    }
                    requestDetailMap.Add(map.Key.GetValueOrDefault(), d_1);
                }

                // lấy các linkData NVL đầu vào
                foreach( var f_1 in requestDetailMap)
                {
                    var outsourceOrderDetail = outsourceOrder.OutsourceOrderDetail.FirstOrDefault(x => x.ObjectId == f_1.Key);
                    foreach(var f_2 in f_1.Value)
                    {
                        var totalQuantityOrigin = role.Where(x => f_2.Value.Keys.Contains(x.ProductionStepLinkDataId) && x.ProductionStepLinkDataRoleTypeId == (int)EnumProductionStepLinkDataRoleType.Output).Sum(x => x.ProductionStepLinkData.QuantityOrigin);

                        foreach (var f_3 in f_2.Value)
                        {
                            var quantityOrigin = role.FirstOrDefault(x => x.ProductionStepLinkDataId == f_3.Key).ProductionStepLinkData.QuantityOrigin;
                            var percent = ((quantityOrigin / totalQuantityOrigin) * outsourceOrderDetail.Quantity) / quantityOrigin;

                            var linkDataIds = role.Where(x => f_3.Value.Contains(x.ProductionStepId)
                                                && x.ProductionStepLinkDataRoleTypeId == (int)EnumProductionStepLinkDataRoleType.Input)
                                      .Select(x => x.ProductionStepLinkDataId)
                                      .ToArray();
                            var stepLinkDatas = new List<ProductionStepLinkDataInput>();

                            if (linkDataIds.Length > 0)
                            {
                                var sql = new StringBuilder("Select * from ProductionStepLinkDataExtractInfo v ");
                                var parammeters = new List<SqlParameter>();
                                var whereCondition = new StringBuilder();

                                whereCondition.Append("v.ProductionStepLinkDataId IN ( ");
                                for (int i = 0; i < linkDataIds.Length; i++)
                                {
                                    var number = linkDataIds[i];
                                    string pName = $"@ProductionStepLinkDataId_{i + 1}";

                                    if (i == linkDataIds.Length - 1)
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

                                stepLinkDatas.ForEach(ld => {
                                    results.Add(new Model.Outsource.Order.OutsourceOrderMaterials
                                    {
                                        CustomerId = outsourceOrder.CustomerId,
                                        Description = $"Xuất vật tư cho đơn hàng gia công {outsourceOrder.OutsourceOrderCode}",
                                        OrderCode = request.OrderCode,
                                        OutsourceOrderCode = outsourceOrder.OutsourceOrderCode,
                                        OutsourceOrderId = outsourceOrder.OutsourceOrderId,
                                        ProductId = ld.ObjectId,
                                        ProductionOrdeCode = request.ProductionOrderCode,
                                        UnitId = ld.UnitId,
                                        OutsourceRequestId = request.OutsourcePartRequestId,
                                        OutsourceRequestCode = request.OutsourcePartRequestCode,
                                        Quantity = decimal.Round((percent * ld.QuantityOrigin), 5)
                                    });

                                });
                            }
                        }
                            
                    }
                    
                }
                

            }

            return results;
        }

        #region private
        private IEnumerable<long> TracedStepStoreMaterials(long? currentStepId, IEnumerable<ProductionStepLinkModel> lsStepLink )
        {
            var rs = new List<long>();
            var stepLinks = lsStepLink.Where(x => x.ToStepId == currentStepId);

            if(stepLinks.Count() == 0)
            {
                rs.Add(currentStepId.GetValueOrDefault());
                return rs;
            }

            foreach (var sl in stepLinks)
            {
                var s_id = sl.FromStepId;
                rs.AddRange(TracedStepStoreMaterials(s_id, lsStepLink));
            }
            return rs;
        }

        private static IEnumerable<ProductionStepLinkModel> CalcProdictonStepLink(IEnumerable<ProductionStepLinkDataRole> roles)
        {
            var roleGroups = roles.GroupBy(r => r.ProductionStepLinkDataId);
            var productionStepLinks = new List<ProductionStepLinkModel>();

            foreach (var roleGroup in roleGroups)
            {
                var froms = roleGroup.Where(r => r.ProductionStepLinkDataRoleTypeId == (int) EnumProductionStepLinkDataRoleType.Output).ToList();
                var tos = roleGroup.Where(r => r.ProductionStepLinkDataRoleTypeId == (int)EnumProductionStepLinkDataRoleType.Input).ToList();
                foreach (var from in froms)
                {
                    bool bExisted = productionStepLinks.Any(r => r.FromStepId == from.ProductionStepId);
                    foreach (var to in tos)
                    {
                        if (!bExisted || !productionStepLinks.Any(r => r.FromStepId == from.ProductionStepId && r.ToStepId == to.ProductionStepId))
                        {
                            productionStepLinks.Add(new ProductionStepLinkModel
                            {
                                FromStepId = from.ProductionStepId,
                                ToStepId = to.ProductionStepId,
                            });
                        }
                    }
                }
            }
            return productionStepLinks;
        }

        private async Task CheckMarkInvalidOutsourcePartRequest(long[] outsourcePartrequestDetaildIds)
        {
            var lsInValid = (await _manufacturingDBContext.OutsourcePartRequestDetail.AsNoTracking()
               .Include(x => x.OutsourcePartRequest)
               .Where(x => outsourcePartrequestDetaildIds.Contains(x.OutsourcePartRequestDetailId))
               .ToListAsync())
               .Select(x => new
               {
                   OutsourcePartRequestCode = x.OutsourcePartRequest.OutsourcePartRequestCode,
                   MarkInvalid = x.OutsourcePartRequest.MarkInvalid
               })
               .Where(x => x.MarkInvalid)
               .Select(x => x.OutsourcePartRequestCode)
               .Distinct()
               .ToArray();
            if (lsInValid.Length > 0)
                throw new BadRequestException(OutsourceErrorCode.InValidRequestOutsource, $"YCGC \"{String.Join(", ", lsInValid)}\" chưa xác thực với QTSX");

        }

        private async Task UpdateOutsourcePartRequestStatus(IEnumerable<long> ObjectIds)
        {
            var stepIds = await _manufacturingDBContext.OutsourcePartRequestDetail.AsNoTracking()
                .Where(x => ObjectIds.Contains(x.OutsourcePartRequestDetailId))
                .Select(x => x.OutsourcePartRequestId)
                .Distinct()
                .ToArrayAsync();
            await _outsourcePartRequestService.UpdateOutsourcePartRequestStatus(stepIds);
        }
        #endregion
    }
}
