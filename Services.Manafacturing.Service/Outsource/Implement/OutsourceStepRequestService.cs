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
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Manafacturing.Model.Outsource.RequestStep;
using VErp.Services.Manafacturing.Model.ProductionOrder;
using VErp.Services.Manafacturing.Model.ProductionStep;
using static VErp.Commons.Enums.Manafacturing.EnumProductionProcess;

namespace VErp.Services.Manafacturing.Service.Outsource.Implement
{
    public class OutsourceStepRequestService : IOutsourceStepRequestService
    {
        private readonly ManufacturingDBContext _manufacturingDBContext;
        private readonly IActivityLogService _activityLogService;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly ICustomGenCodeHelperService _customGenCodeHelperService;

        public OutsourceStepRequestService(ManufacturingDBContext manufacturingDB
            , IActivityLogService activityLogService
            , ILogger<OutsourceStepRequestService> logger
            , IMapper mapper
            , ICustomGenCodeHelperService customGenCodeHelperService)
        {
            _manufacturingDBContext = manufacturingDB;
            _activityLogService = activityLogService;
            _logger = logger;
            _mapper = mapper;
            _customGenCodeHelperService = customGenCodeHelperService;
        }

        public async Task<PageData<OutsourceStepRequestSearch>> GetListOutsourceStepRequest(string keyword, int page, int size, string orderByFieldName, bool asc, Clause filters = null)
        {
            var outsourceStepRequest = await _manufacturingDBContext.OutsourceStepRequest.AsNoTracking()
                .Include(x => x.OutsourceStepRequestData)
                .ProjectTo<OutsourceStepRequestModel>(_mapper.ConfigurationProvider)
                .ToListAsync();

            var data = outsourceStepRequest.GroupBy(x => x.ProductionOrderId);
            var kVReqestAndStep = new Dictionary<long, IList<long>>();

            foreach (var group in data)
            {
                var roles = await _manufacturingDBContext.ProductionStep.AsNoTracking()
                .Include(s => s.ProductionStepLinkDataRole)
                .Where(x => x.ContainerId == group.Key && x.ContainerTypeId == (int)EnumContainerType.ProductionOrder)
                .SelectMany(x => x.ProductionStepLinkDataRole, (s, d) => new ProductionStepLinkDataRoleModel
                {
                    ProductionStepId = s.ProductionStepId,
                    ProductionStepLinkDataId = d.ProductionStepLinkDataId,
                    ProductionStepLinkDataRoleTypeId = (EnumProductionStepLinkDataRoleType)d.ProductionStepLinkDataRoleTypeId,
                }).ToListAsync();

                foreach (var request in group)
                {
                    kVReqestAndStep.Add(request.OutsourceStepRequestId, FoundProductionStepInOutsourceStepRequest(request.OutsourceStepRequestData, roles));
                }
            }

            var total = 0;
            var lst = new List<OutsourceStepRequestSearch>();

            if (kVReqestAndStep.Count > 0)
            {
                var sql = new StringBuilder(@"SELECT * FROM vOutsourceStepRequestExtractInfo v ");
                var totalSql = new StringBuilder(@"SELECT COUNT(v.OutsourceStepRequestId) Total FROM vOutsourceStepRequestExtractInfo v ");
                var parammeters = new List<SqlParameter>();
                var whereCondition = new StringBuilder();
                var whereFilterProductionStep = new StringBuilder();
                if (!string.IsNullOrEmpty(keyword))
                {
                    whereCondition.Append(" (v.OutsourceStepRequestCode LIKE @KeyWord ");
                    whereCondition.Append("OR v.ProductionOrderCode LIKE @Keyword ");
                    whereCondition.Append("OR v.OrderCode LIKE @Keyword) ");
                    parammeters.Add(new SqlParameter("@Keyword", $"%{keyword}%"));
                }

                if (filters != null)
                {
                    var suffix = 0;
                    var filterCondition = new StringBuilder();
                    filters.FilterClauseProcess("vOutsourceStepRequestExtractInfo", "v", ref filterCondition, ref parammeters, ref suffix);
                    if (filterCondition.Length > 2)
                    {
                        if (whereCondition.Length > 0) whereCondition.Append(" AND ");
                        whereCondition.Append(filterCondition);
                    }
                }

                for (int i = 0; i < kVReqestAndStep.Keys.Count; i++)
                {
                    var outsourceStepRequestId = kVReqestAndStep.Keys.ToList()[i];
                    var pRequestName = $"@OutsourceStepRequestId_{i + 1}";
                    if (i != 0)
                        whereFilterProductionStep.Append(" OR ");
                    whereFilterProductionStep.Append($"( v.OutsourceStepRequestId = {pRequestName} AND  v.ProductionStepId IN ( ");
                    parammeters.Add(new SqlParameter(pRequestName, outsourceStepRequestId));

                    var lsProductionStepId = kVReqestAndStep[outsourceStepRequestId];
                    for (int j = 0; j < lsProductionStepId.Count; j++)
                    {
                        var number = lsProductionStepId[j];
                        string pName = $"@ProductionStepId_{i + 1}_{j + 1}";

                        if (j == lsProductionStepId.Count - 1)
                            whereFilterProductionStep.Append($"{pName} ) )");
                        else
                            whereFilterProductionStep.Append($"{pName}, ");
                        parammeters.Add(new SqlParameter(pName, number));
                    }

                }

                if (whereFilterProductionStep.Length > 0)
                {
                    sql.Append(" WHERE ");
                    sql.Append(whereFilterProductionStep);
                    totalSql.Append(" WHERE ");
                    totalSql.Append(whereFilterProductionStep);
                }

                if (whereCondition.Length > 0)
                {
                    sql.Append(" AND ");
                    sql.Append(whereCondition);
                }

                orderByFieldName = string.IsNullOrWhiteSpace(orderByFieldName) ? "OutsourceStepRequestId" : orderByFieldName;
                sql.Append($" ORDER BY v.[{orderByFieldName}] {(asc ? "" : "DESC")}");

                var table = await _manufacturingDBContext.QueryDataTable(totalSql.ToString(), parammeters.ToArray());

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
                lst = resultData.ConvertData<OutsourceStepRequestEntity>().AsQueryable().ProjectTo<OutsourceStepRequestSearch>(_mapper.ConfigurationProvider).ToList();
            }

            // Tính toán trạng thái cho các công đoạn
            var distinctKeys = (from rd in _manufacturingDBContext.OutsourceStepRequestData
                                join d in _manufacturingDBContext.OutsourceOrderDetail on rd.ProductionStepLinkDataId equals d.ObjectId
                                join o in _manufacturingDBContext.OutsourceOrder on d.OutsourceOrderId equals o.OutsourceOrderId
                                where o.OutsourceTypeId == (int)EnumOutsourceOrderType.OutsourceStep
                                select new { rd.OutsourceStepRequestId, o.OutsourceOrderId }).Distinct();

            var outsourceTracks = await (from d in distinctKeys
                                         join track in _manufacturingDBContext.OutsourceTrack on d.OutsourceOrderId equals track.OutsourceOrderId
                                         select new { d.OutsourceStepRequestId, track }).ToListAsync();

            foreach (var l in lst)
            {
                var track = outsourceTracks.Where(x => x.OutsourceStepRequestId == l.OutsourceStepRequestId).Select(x => x.track).GroupBy(x => x.OutsourceOrderId);

                var sumTrack = 0;
                foreach (var t in track)
                {
                    var lastTrack = t.Where(x => !x.ObjectId.HasValue || x.ObjectId.Value == l.ProductionStepId).Last();
                    sumTrack += lastTrack.OutsourceTrackStatusId;
                }
                if (sumTrack == 0) l.ProductionStepInRequestStatusId = EnumOutsourceRequestStatusType.Unprocessed;
                else if (sumTrack == ((int)EnumOutsourceTrack.EnumOutsourceTrackStatus.HandedOver * track.Count()))
                    l.ProductionStepInRequestStatusId = EnumOutsourceRequestStatusType.Processed;
                else if (sumTrack > 0)
                    l.ProductionStepInRequestStatusId = EnumOutsourceRequestStatusType.Processing;
            }

            return (lst, total);
        }


        public async Task<OutsourceStepRequestInfo> GetOutsourceStepRequest(long outsourceStepRequestId)
        {
            var outsourceStepRequest = await _manufacturingDBContext.OutsourceStepRequest.AsNoTracking()
                .Include(x => x.OutsourceStepRequestData)
                .ProjectTo<OutsourceStepRequestInfo>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(x => x.OutsourceStepRequestId == outsourceStepRequestId);
            if (outsourceStepRequest == null)
                throw new BadRequestException(OutsourceErrorCode.NotFoundRequest);

            var roles = await _manufacturingDBContext.ProductionStep.AsNoTracking()
                .Include(s => s.ProductionStepLinkDataRole)
                .Where(x => x.ContainerId == outsourceStepRequest.ProductionOrderId && x.ContainerTypeId == (int)EnumContainerType.ProductionOrder)
                .SelectMany(x => x.ProductionStepLinkDataRole, (s, d) => new ProductionStepLinkDataRoleModel
                {
                    ProductionStepId = s.ProductionStepId,
                    ProductionStepLinkDataId = d.ProductionStepLinkDataId,
                    ProductionStepLinkDataRoleTypeId = (EnumProductionStepLinkDataRoleType)d.ProductionStepLinkDataRoleTypeId,
                }).ToListAsync();

            var lsProductionStepId = FoundProductionStepInOutsourceStepRequest(outsourceStepRequest.OutsourceStepRequestData, roles);

            var productionStepOrder = await _manufacturingDBContext.ProductionStepOrder
                .AsNoTracking()
                .Include(x => x.ProductionStep)
                .ThenInclude(s => s.Step)
                .Where(x => lsProductionStepId.Contains(x.ProductionStepId)).ToListAsync();

            outsourceStepRequest.ProductionSteps = _mapper.Map<IList<ProductionStepModel>>(productionStepOrder.Select(x => x.ProductionStep));
            outsourceStepRequest.roles = roles.Where(x => lsProductionStepId.Contains(x.ProductionStepId)).ToList();

            var lsProductionOrderDetailId = productionStepOrder.Select(x => x.ProductionOrderDetailId).Distinct().ToList();

            if (lsProductionOrderDetailId.Count > 0)
            {
                var sql = new StringBuilder(@"SELECT * FROM vProductionOrderDetail v ");
                var parammeters = new List<SqlParameter>();
                var whereFilterProductionStep = new StringBuilder();

                whereFilterProductionStep.Append(" v.ProductionOrderDetailId IN ( ");
                for (int i = 0; i < lsProductionOrderDetailId.Count; i++)
                {
                    var number = lsProductionOrderDetailId[i];
                    string pName = $"@ProductionOrderDetailId{i + 1}";

                    if (i == lsProductionOrderDetailId.Count - 1)
                        whereFilterProductionStep.Append($"{pName} ) ");
                    else
                        whereFilterProductionStep.Append($"{pName}, ");

                    parammeters.Add(new SqlParameter(pName, number));
                }

                if (whereFilterProductionStep.Length > 0)
                {
                    sql.Append(" WHERE ");
                    sql.Append(whereFilterProductionStep);
                }

                var resultData = await _manufacturingDBContext.QueryDataTable(sql.ToString(), parammeters.Select(p => p.CloneSqlParam()).ToArray());
                var lst = resultData.ConvertData<ProductionOrderListEntity>().AsQueryable().ProjectTo<ProductionOrderListModel>(_mapper.ConfigurationProvider).ToList();

                outsourceStepRequest.ProductTitle = string.Join(", ", lst.Select(x => x.ProductTitle));
                outsourceStepRequest.OrderCode = string.Join(", ", lst.Where(x => !string.IsNullOrWhiteSpace(x.OrderCode)).Select(x => x.OrderCode).Distinct());
            }

            var distinctKeys = (from rd in _manufacturingDBContext.OutsourceStepRequestData
                                join d in _manufacturingDBContext.OutsourceOrderDetail on rd.ProductionStepLinkDataId equals d.ObjectId
                                join o in _manufacturingDBContext.OutsourceOrder on d.OutsourceOrderId equals o.OutsourceOrderId
                                where o.OutsourceTypeId == (int)EnumOutsourceOrderType.OutsourceStep
                                select new { rd.OutsourceStepRequestId, o.OutsourceOrderId }).Distinct();

            var outsourceTracks = await (from d in distinctKeys
                                         join track in _manufacturingDBContext.OutsourceTrack on d.OutsourceOrderId equals track.OutsourceOrderId
                                         select new { d.OutsourceStepRequestId, track }).ToListAsync();
            var totalTrack = 0;
            foreach (var l in outsourceStepRequest.ProductionSteps)
            {
                var track = outsourceTracks.Where(x => x.OutsourceStepRequestId == outsourceStepRequest.OutsourceStepRequestId).Select(x => x.track).GroupBy(x => x.OutsourceOrderId);

                var sumTrack = 0;
                foreach (var t in track)
                {
                    var lastTrack = t.Where(x => !x.ObjectId.HasValue || x.ObjectId.Value == l.ProductionStepId).Last();
                    sumTrack += lastTrack.OutsourceTrackStatusId;
                }
                totalTrack += sumTrack;

            }

            if (totalTrack == 0) outsourceStepRequest.OutsourceStepRequestStatus = EnumOutsourceRequestStatusType.Unprocessed.GetEnumDescription();
            else if (totalTrack == ((int)EnumOutsourceTrack.EnumOutsourceTrackStatus.HandedOver * outsourceStepRequest.ProductionSteps.Count()))
                outsourceStepRequest.OutsourceStepRequestStatus = EnumOutsourceRequestStatusType.Processed.GetEnumDescription();
            else if (totalTrack > 0)
                outsourceStepRequest.OutsourceStepRequestStatus = EnumOutsourceRequestStatusType.Processing.GetEnumDescription();

            return outsourceStepRequest;
        }

        private IList<long> FoundProductionStepInOutsourceStepRequest(IList<OutsourceStepRequestDataModel> outsourceStepRequestDatas, List<ProductionStepLinkDataRoleModel> roles)
        {
            var outputData = outsourceStepRequestDatas
                .Where(x => x.ProductionStepLinkDataRoleTypeId == EnumProductionStepLinkDataRoleType.Output)
                .Select(x => x.ProductionStepLinkDataId)
                .ToList();

            var inputData = outsourceStepRequestDatas
                .Where(x => x.ProductionStepLinkDataRoleTypeId == EnumProductionStepLinkDataRoleType.Input)
                .Select(x => x.ProductionStepLinkDataId)
                .ToList();

            var productionStepStartId = roles.Where(x => inputData.Contains(x.ProductionStepLinkDataId)
                   && x.ProductionStepLinkDataRoleTypeId == EnumProductionStepLinkDataRoleType.Input)
                .Select(x => x.ProductionStepId)
                .Distinct()
                .ToList();
            var productionStepEndId = roles.Where(x => outputData.Contains(x.ProductionStepLinkDataId)
                     && x.ProductionStepLinkDataRoleTypeId == EnumProductionStepLinkDataRoleType.Output)
                .Select(x => x.ProductionStepId)
                .Distinct()
                .ToList();

            var lsProductionStepId = new List<long>();
            foreach (var id in productionStepEndId)
                FindTraceProductionStep(inputData, roles, productionStepStartId, lsProductionStepId, id);

            return lsProductionStepId
                    .Union(productionStepEndId)
                    .Union(productionStepStartId)
                    .Distinct()
                    .ToList();
        }

        private void FindTraceProductionStep(List<long> inputLinkData, List<ProductionStepLinkDataRoleModel> roles, List<long> productionStepStartId, List<long> result, long productionStepId)
        {
            var roleInput = roles.Where(x => x.ProductionStepId == productionStepId
                    && !inputLinkData.Contains(x.ProductionStepLinkDataId)
                    && x.ProductionStepLinkDataRoleTypeId == EnumProductionStepLinkDataRoleType.Input)
                .ToList();
            foreach (var input in roleInput)
            {
                var roleOutput = roles.Where(x => x.ProductionStepLinkDataId == input.ProductionStepLinkDataId
                        && x.ProductionStepLinkDataRoleTypeId == EnumProductionStepLinkDataRoleType.Output)
                    .FirstOrDefault();

                if (roleOutput == null) continue;

                result.Add(roleOutput.ProductionStepId);
                FindTraceProductionStep(inputLinkData, roles, productionStepStartId, result, roleOutput.ProductionStepId);
            }
        }


        public async Task<long> CreateOutsourceStepRequest(OutsourceStepRequestModel req)
        {

            var productionStepHadOutsourceRequest = await GetProductionStepInOutsourceStepRequest(req.ProductionOrderId);
            var roles = await _manufacturingDBContext.ProductionStep.AsNoTracking()
               .Include(s => s.ProductionStepLinkDataRole)
               .Where(x => x.ContainerId == req.ProductionOrderId && x.ContainerTypeId == (int)EnumContainerType.ProductionOrder)
               .SelectMany(x => x.ProductionStepLinkDataRole, (s, d) => new ProductionStepLinkDataRoleModel
               {
                   ProductionStepId = s.ProductionStepId,
                   ProductionStepLinkDataId = d.ProductionStepLinkDataId,
                   ProductionStepLinkDataRoleTypeId = (EnumProductionStepLinkDataRoleType)d.ProductionStepLinkDataRoleTypeId,
               }).ToListAsync();

            var lsProductionStepId = (List<long>)FoundProductionStepInOutsourceStepRequest(req.OutsourceStepRequestData, roles);

            if (productionStepHadOutsourceRequest.Count > 0)
            {
                foreach (var productionStep in productionStepHadOutsourceRequest)
                {
                    if (lsProductionStepId.Contains(productionStep.ProductionStepId))
                        throw new BadRequestException(OutsourceErrorCode.EarlyExistsProductionStepHadOutsourceRequest);
                }
            }

            using var trans = await _manufacturingDBContext.Database.BeginTransactionAsync();
            try
            {
                // Get cấu hình sinh mã

                var currentConfig = await _customGenCodeHelperService.CurrentConfig(EnumObjectType.OutsourceRequest, EnumObjectType.OutsourceRequest, 0, null, req.OutsourceStepRequestCode, req.OutsourceStepRequestDate);

                if (currentConfig == null)
                {
                    throw new BadRequestException(GeneralCode.ItemNotFound, "Chưa thiết định cấu hình sinh mã");
                }
                var generated = await _customGenCodeHelperService.GenerateCode(currentConfig.CustomGenCodeId, currentConfig.CurrentLastValue.LastValue, null, req.OutsourceStepRequestCode, req.OutsourceStepRequestDate);
                if (generated == null)
                {
                    throw new BadRequestException(GeneralCode.InternalError, "Không thể sinh mã ");
                }


                // Create outsourceStepRequest
                var outsourceStepRequest = _mapper.Map<OutsourceStepRequest>(req);
                outsourceStepRequest.OutsourceStepRequestCode = generated.CustomCode;

                _manufacturingDBContext.OutsourceStepRequest.Add(outsourceStepRequest);
                await _manufacturingDBContext.SaveChangesAsync();

                // Create outsourceStepRequestData
                var outsourceStepRequestDatas = new List<OutsourceStepRequestData>();
                foreach (var data in req.OutsourceStepRequestData)
                {
                    data.OutsourceStepRequestId = outsourceStepRequest.OutsourceStepRequestId;
                    outsourceStepRequestDatas.Add(_mapper.Map<OutsourceStepRequestData>(data));
                }

                await _manufacturingDBContext.OutsourceStepRequestData.AddRangeAsync(outsourceStepRequestDatas);
                await _manufacturingDBContext.SaveChangesAsync();

                // Update outsourceQuanity của LinkData liên quan
                var tData = outsourceStepRequestDatas.FirstOrDefault(x => x.ProductionStepLinkDataRoleTypeId == (int)EnumProductionStepLinkDataRoleType.Output);
                var linkData = await _manufacturingDBContext.ProductionStepLinkData.FirstOrDefaultAsync(x => x.ProductionStepLinkDataId == tData.ProductionStepLinkDataId);
                await UpdateProductionStepLinkDataRelative(lsProductionStepId, (decimal)(tData.Quantity / linkData.Quantity));

                await _customGenCodeHelperService.ConfirmCode(currentConfig.CurrentLastValue);
                trans.Commit();



                await _activityLogService.CreateLog(EnumObjectType.ProductionOrder, outsourceStepRequest.OutsourceStepRequestId,
                    $"Thêm mới yêu cầu gia công công đoạn", req.JsonSerialize());
                return outsourceStepRequest.OutsourceStepRequestId;
            }
            catch (Exception ex)
            {
                trans.TryRollbackTransaction();
                _logger.LogError(ex, "CreateOutsourceStepRequest");
                throw;
            }
        }

        public async Task<bool> UpdateOutsourceStepRequest(long outsourceStepRequestId, OutsourceStepRequestModel req)
        {
            var outsourceStepRequest = await _manufacturingDBContext.OutsourceStepRequest.FirstOrDefaultAsync(x => x.OutsourceStepRequestId == outsourceStepRequestId);
            if (outsourceStepRequest == null)
                throw new BadRequestException(OutsourceErrorCode.NotFoundRequest);
            var trans = await _manufacturingDBContext.Database.BeginTransactionAsync();
            try
            {
                _mapper.Map(req, outsourceStepRequest);

                var tData_1 = req.OutsourceStepRequestData.FirstOrDefault(x => x.ProductionStepLinkDataRoleTypeId == EnumProductionStepLinkDataRoleType.Output);
                var linkData = await _manufacturingDBContext.ProductionStepLinkData.FirstOrDefaultAsync(x => x.ProductionStepLinkDataId == tData_1.ProductionStepLinkDataId);
                var newPercent = (decimal)(tData_1.OutsourceStepRequestDataQuantity / linkData.Quantity);

                var outsourceStepRequestDataOld = await _manufacturingDBContext.OutsourceStepRequestData
                    .Where(d => d.OutsourceStepRequestId == outsourceStepRequestId)
                    .ToListAsync();
                var outsourceStepRequestDataNew = _mapper.Map<IList<OutsourceStepRequestData>>(req.OutsourceStepRequestData);

                var tData_2 = outsourceStepRequestDataOld.FirstOrDefault(x => x.ProductionStepLinkDataId == tData_1.ProductionStepLinkDataId);
                var oldPercent = (decimal)(tData_2.Quantity / linkData.Quantity);

                _manufacturingDBContext.OutsourceStepRequestData.RemoveRange(outsourceStepRequestDataOld);
                await _manufacturingDBContext.OutsourceStepRequestData.AddRangeAsync(outsourceStepRequestDataNew);

                await _manufacturingDBContext.SaveChangesAsync();

                // Update outsourceQuanity của LinkData liên quan
                var roles = await _manufacturingDBContext.ProductionStep.AsNoTracking()
                                  .Include(s => s.ProductionStepLinkDataRole)
                                  .Where(x => x.ContainerId == req.ProductionOrderId && x.ContainerTypeId == (int)EnumContainerType.ProductionOrder)
                                  .SelectMany(x => x.ProductionStepLinkDataRole, (s, d) => new ProductionStepLinkDataRoleModel
                                  {
                                      ProductionStepId = s.ProductionStepId,
                                      ProductionStepLinkDataId = d.ProductionStepLinkDataId,
                                      ProductionStepLinkDataRoleTypeId = (EnumProductionStepLinkDataRoleType)d.ProductionStepLinkDataRoleTypeId,
                                  }).ToListAsync();

                var lsProductionStepId = (List<long>)FoundProductionStepInOutsourceStepRequest(req.OutsourceStepRequestData, roles);
                await UpdateProductionStepLinkDataRelative(lsProductionStepId, newPercent, oldPercent);

                await trans.CommitAsync();

                return true;
            }
            catch (Exception ex)
            {
                await trans.TryRollbackTransactionAsync();
                _logger.LogError(ex, "UpdateOutsourceStepRequest");
                throw;
            }
        }

        public async Task<bool> DeleteOutsourceStepRequest(long outsourceStepRequestId)
        {
            var outsourceStepRequest = await _manufacturingDBContext.OutsourceStepRequest.FirstOrDefaultAsync(x => x.OutsourceStepRequestId == outsourceStepRequestId);
            if (outsourceStepRequest == null)
                throw new BadRequestException(OutsourceErrorCode.NotFoundRequest);
            var trans = await _manufacturingDBContext.Database.BeginTransactionAsync();
            try
            {
                outsourceStepRequest.IsDeleted = true;
                var outsourceStepRequestDataOld = await _manufacturingDBContext.OutsourceStepRequestData
                    .Where(d => d.OutsourceStepRequestId == outsourceStepRequestId)
                    .ToListAsync();

                var lst = (from o in _manufacturingDBContext.OutsourceOrder
                           join d in _manufacturingDBContext.OutsourceOrderDetail
                             on o.OutsourceOrderId equals d.OutsourceOrderId
                           where o.OutsourceTypeId == (int)EnumOutsourceOrderType.OutsourceStep
                           select d).GroupBy(x => x.ObjectId).Select(x => new
                           {
                               ObjectId = x.Key,
                               QuantityProcessed = x.Sum(x => x.Quantity)
                           });

                outsourceStepRequestDataOld.ForEach(x =>
                {
                    if (lst.Where(y => y.ObjectId == x.ProductionStepLinkDataId && y.QuantityProcessed > 0).Count() != 0)
                        throw new BadRequestException(OutsourceErrorCode.InValidRequestOutsource, $"Đã có đơn hàng gia công cho yêu cầu {outsourceStepRequest.OutsourceStepRequestCode}");
                });

                _manufacturingDBContext.OutsourceStepRequestData.RemoveRange(outsourceStepRequestDataOld);
                await _manufacturingDBContext.SaveChangesAsync();

                // Update outsourceQuanity của LinkData liên quan
                var roles = await _manufacturingDBContext.ProductionStep.AsNoTracking()
                                 .Include(s => s.ProductionStepLinkDataRole)
                                 .Where(x => x.ContainerId == outsourceStepRequest.ProductionOrderId && x.ContainerTypeId == (int)EnumContainerType.ProductionOrder)
                                 .SelectMany(x => x.ProductionStepLinkDataRole, (s, d) => new ProductionStepLinkDataRoleModel
                                 {
                                     ProductionStepId = s.ProductionStepId,
                                     ProductionStepLinkDataId = d.ProductionStepLinkDataId,
                                     ProductionStepLinkDataRoleTypeId = (EnumProductionStepLinkDataRoleType)d.ProductionStepLinkDataRoleTypeId,
                                 }).ToListAsync();

                var lsProductionStepId = (List<long>)FoundProductionStepInOutsourceStepRequest(_mapper.Map<IList<OutsourceStepRequestDataModel>>(outsourceStepRequestDataOld), roles);
                var tData_1 = outsourceStepRequestDataOld.FirstOrDefault(x => x.ProductionStepLinkDataRoleTypeId == (int)EnumProductionStepLinkDataRoleType.Output);
                var linkData = await _manufacturingDBContext.ProductionStepLinkData.FirstOrDefaultAsync(x => x.ProductionStepLinkDataId == tData_1.ProductionStepLinkDataId);
                var oldPercent = (decimal)(tData_1.Quantity / linkData.Quantity);
                await UpdateProductionStepLinkDataRelative(lsProductionStepId, 0, oldPercent);

                await trans.CommitAsync();

                return true;
            }
            catch (Exception ex)
            {
                await trans.TryRollbackTransactionAsync();
                _logger.LogError(ex, "DeleteOutsourceStepRequest");
                throw;
            }
        }

        public async Task<IList<OutsourceStepRequestDataInfo>> GetOutsourceStepRequestData(long outsourceStepRequestId)
        {
            var outsourceStepRequest = await _manufacturingDBContext.OutsourceStepRequest.AsNoTracking()
                .Include(x => x.OutsourceStepRequestData)
                .ProjectTo<OutsourceStepRequestInfo>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(x => x.OutsourceStepRequestId == outsourceStepRequestId);
            if (outsourceStepRequest == null)
                throw new BadRequestException(OutsourceErrorCode.NotFoundRequest);

            var roles = await _manufacturingDBContext.ProductionStep.AsNoTracking()
                .Include(s => s.ProductionStepLinkDataRole)
                .Where(x => x.ContainerId == outsourceStepRequest.ProductionOrderId && x.ContainerTypeId == (int)EnumContainerType.ProductionOrder)
                .SelectMany(x => x.ProductionStepLinkDataRole, (s, d) => new ProductionStepLinkDataRoleModel
                {
                    ProductionStepId = s.ProductionStepId,
                    ProductionStepLinkDataId = d.ProductionStepLinkDataId,
                    ProductionStepLinkDataRoleTypeId = (EnumProductionStepLinkDataRoleType)d.ProductionStepLinkDataRoleTypeId,
                }).ToListAsync();

            var lsProductionStepId = FoundProductionStepInOutsourceStepRequest(outsourceStepRequest.OutsourceStepRequestData, roles);

            var productionSteps = await _manufacturingDBContext.ProductionStep.AsNoTracking()
                .Include(s => s.Step)
                .Include(s => s.ProductionStepLinkDataRole)
                .ThenInclude(r => r.ProductionStepLinkData)
                .Where(s => lsProductionStepId.Contains(s.ProductionStepId))
                .ProjectTo<ProductionStepInfo>(_mapper.ConfigurationProvider)
                .ToListAsync();

            var lst = new List<OutsourceStepRequestDataInfo>();
            foreach (var productionStep in productionSteps)
            {
                var outsourceStepRequestDatas = productionStep.ProductionStepLinkDatas
                    .Where(x => outsourceStepRequest.OutsourceStepRequestData
                                .Select(y => y.ProductionStepLinkDataId).Contains(x.ProductionStepLinkDataId))
                    .Select(x => new OutsourceStepRequestDataInfo
                    {
                        OutsourceStepRequestCode = outsourceStepRequest.OutsourceStepRequestCode,
                        OutsourceStepRequestId = outsourceStepRequest.OutsourceStepRequestId,
                        ProductionStepId = productionStep.ProductionStepId,
                        ProductionStepTitle = productionStep.Title,
                        ProductionStepLinkDataId = x.ProductionStepLinkDataId,
                        ProductionStepLinkDataQuantity = x.Quantity,
                        ProductionStepLinkDataRoleTypeId = x.ProductionStepLinkDataRoleTypeId,
                        OutsourceStepRequestDataQuantity = outsourceStepRequest.OutsourceStepRequestData.FirstOrDefault(s => s.ProductionStepLinkDataId == x.ProductionStepLinkDataId).OutsourceStepRequestDataQuantity,
                        ProductionStepLinkDataTitle = string.Empty
                    })
                    .ToList();
                if (outsourceStepRequestDatas.Count == 0)
                    lst.Add(new OutsourceStepRequestDataInfo
                    {
                        OutsourceStepRequestCode = outsourceStepRequest.OutsourceStepRequestCode,
                        OutsourceStepRequestId = outsourceStepRequest.OutsourceStepRequestId,
                        ProductionStepId = productionStep.ProductionStepId,
                        ProductionStepTitle = productionStep.Title,
                        ProductionStepLinkDataRoleTypeId = EnumProductionStepLinkDataRoleType.Input
                    });
                else
                    lst.AddRange(outsourceStepRequestDatas);
            }

            var groupbySumQuantityProcessed = await (from order in _manufacturingDBContext.OutsourceOrder.Where(x => x.OutsourceTypeId == (int)EnumContainerType.ProductionOrder)
                                                     join detail in _manufacturingDBContext.OutsourceOrderDetail on order.OutsourceOrderId equals detail.OutsourceOrderId
                                                     group new { order, detail } by detail.ObjectId into g
                                                     select new
                                                     {
                                                         ProductionStepLinkDataId = g.Key,
                                                         OutsourceStepRequestQuantityProcessed = g.Sum(x => x.detail.Quantity)
                                                     }).ToListAsync();

            var output = lst.FirstOrDefault(x => x.ProductionStepLinkDataRoleTypeId == EnumProductionStepLinkDataRoleType.Output);
            var outputProcessed = groupbySumQuantityProcessed.FirstOrDefault(x => x.ProductionStepLinkDataId == output.ProductionStepLinkDataId);

            var percent = output.OutsourceStepRequestDataQuantity / output.ProductionStepLinkDataQuantity;
            var percentProcessed = outputProcessed == null ? 0 : outputProcessed.OutsourceStepRequestQuantityProcessed / output.OutsourceStepRequestDataQuantity;
            lst.ForEach(x =>
            {
                x.OutsourceStepRequestDataQuantity = percent * x.ProductionStepLinkDataQuantity;
                x.OutsourceStepRequestDataQuantityProcessed = percentProcessed * x.OutsourceStepRequestDataQuantity;
            });

            return lst;
        }

        public async Task<IList<OutsourceStepRequestModel>> GetAllOutsourceStepRequest()
        {
            var lst = await _manufacturingDBContext.OutsourceStepRequest
                .AsNoTracking()
                .Include(x => x.ProductionOrder)
                .Include(x => x.ProductionStep)
                .ProjectTo<OutsourceStepRequestModel>(_mapper.ConfigurationProvider)
                .ToListAsync();
            return lst;
        }

        public async Task<IList<ProductionStepInOutsourceStepRequest>> GetProductionStepInOutsourceStepRequest(long productionOrderId)
        {
            var outsourceStepRequest = await _manufacturingDBContext.OutsourceStepRequest.AsNoTracking()
                .Include(x => x.OutsourceStepRequestData)
                .Where(x => x.ProductionOrderId == productionOrderId)
                .ProjectTo<OutsourceStepRequestModel>(_mapper.ConfigurationProvider)
                .ToListAsync();

            var groups = outsourceStepRequest.GroupBy(x => x.ProductionOrderId);
            var data = new List<ProductionStepInOutsourceStepRequest>();

            var roles = await _manufacturingDBContext.ProductionStep.AsNoTracking()
               .Include(s => s.ProductionStepLinkDataRole)
               .Where(x => x.ContainerId == productionOrderId && x.ContainerTypeId == (int)EnumContainerType.ProductionOrder)
               .SelectMany(x => x.ProductionStepLinkDataRole, (s, d) => new ProductionStepLinkDataRoleModel
               {
                   ProductionStepId = s.ProductionStepId,
                   ProductionStepLinkDataId = d.ProductionStepLinkDataId,
                   ProductionStepLinkDataRoleTypeId = (EnumProductionStepLinkDataRoleType)d.ProductionStepLinkDataRoleTypeId,
               }).ToListAsync();

            foreach (var request in outsourceStepRequest)
            {
                var lst = FoundProductionStepInOutsourceStepRequest(request.OutsourceStepRequestData, roles)
                    .Select(productionStepId => new ProductionStepInOutsourceStepRequest
                    {
                        ProductionProcessId = request.ProductionProcessId,
                        ProductionStepId = productionStepId,
                        OutsourceStepRequestCode = request.OutsourceStepRequestCode,
                        OutsourceStepRequestId = request.OutsourceStepRequestId
                    });
                data.AddRange(lst);
            }

            return data;
        }

        private async Task<bool> UpdateProductionStepLinkDataRelative(IList<long> lsProductionStep, decimal newPercent, decimal oldPercent = 0)
        {
            var productionSteps = await _manufacturingDBContext.ProductionStep.AsNoTracking()
                                        .Where(x => lsProductionStep.Contains(x.ProductionStepId))
                                        .ProjectTo<ProductionStepInfo>(_mapper.ConfigurationProvider)
                                        .ToListAsync();
            var selectLinkDataIds = productionSteps.SelectMany(x => x.ProductionStepLinkDatas).Select(x => x.ProductionStepLinkDataId).Distinct().ToList();
            var productionStepLinkDataEntity = await _manufacturingDBContext.ProductionStepLinkData
                                                        .Where(x => selectLinkDataIds.Contains(x.ProductionStepLinkDataId))
                                                        .ToListAsync();
            foreach (var linkData in productionStepLinkDataEntity)
            {
                var oldValue = linkData.Quantity * oldPercent;
                var newValue = linkData.Quantity * newPercent;
                linkData.OutsourceQuantity += (newValue - oldValue);
            }
            await _manufacturingDBContext.SaveChangesAsync();
            return true;
        }

        public async Task<IList<ProductionStepInfo>> GeneralOutsourceStepOfProductionOrder(long productionOrderId)
        {
            var outsourceStepRequest = await _manufacturingDBContext.OutsourceStepRequest.AsNoTracking()
                .Include(x => x.OutsourceStepRequestData)
                .Where(x => x.ProductionOrderId == productionOrderId)
                .ProjectTo<OutsourceStepRequestModel>(_mapper.ConfigurationProvider)
                .ToListAsync();

            var groups = outsourceStepRequest.GroupBy(x => x.ProductionOrderId);
            var data = new List<ProductionStepInOutsourceStepRequest>();

            var roles = await _manufacturingDBContext.ProductionStep.AsNoTracking()
               .Include(s => s.ProductionStepLinkDataRole)
               .Where(x => x.ContainerId == productionOrderId && x.ContainerTypeId == (int)EnumContainerType.ProductionOrder)
               .SelectMany(x => x.ProductionStepLinkDataRole, (s, d) => new ProductionStepLinkDataRoleModel
               {
                   ProductionStepId = s.ProductionStepId,
                   ProductionStepLinkDataId = d.ProductionStepLinkDataId,
                   ProductionStepLinkDataRoleTypeId = (EnumProductionStepLinkDataRoleType)d.ProductionStepLinkDataRoleTypeId,
               }).ToListAsync();

            var results = new List<ProductionStepInfo>();
            foreach (var request in outsourceStepRequest)
            {
                var lst = FoundProductionStepInOutsourceStepRequest(request.OutsourceStepRequestData, roles);
                var detail = request.OutsourceStepRequestData
                    .FirstOrDefault(x => x.ProductionStepLinkDataRoleTypeId == EnumProductionStepLinkDataRoleType.Output);
                var linkDataOrigin = _manufacturingDBContext.ProductionStepLinkData.FirstOrDefault(x => x.ProductionStepLinkDataId == detail.ProductionStepLinkDataId);

                var percent = detail.OutsourceStepRequestDataQuantity.Value / linkDataOrigin.Quantity;

                var productionSteps = await _manufacturingDBContext.ProductionStep.AsNoTracking()
                .Include(s => s.Step)
                .Include(s => s.ProductionStepLinkDataRole)
                .ThenInclude(r => r.ProductionStepLinkData)
                .Where(s => lst.Contains(s.ProductionStepId))
                .ProjectTo<ProductionStepInfo>(_mapper.ConfigurationProvider)
                .ToListAsync();

                foreach (var productionStep in productionSteps)
                {
                    productionStep.ProductionStepLinkDatas = productionStep.ProductionStepLinkDatas
                                                                .Where(x =>
                                                                            x.ProductionStepLinkDataRoleTypeId
                                                                            == EnumProductionStepLinkDataRoleType.Output
                                                                 )
                                                                .ToList();

                    productionStep.ProductionStepLinkDatas.ForEach(x =>
                        {
                            x.Quantity *= percent;
                        });
                }

                results.AddRange(productionSteps);
            }
            return results;
        }

    }
}
