using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.SqlServer.Management.SqlParser.SqlCodeDom;
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
using VErp.Services.Manafacturing.Model.Outsource.Order;
using VErp.Services.Manafacturing.Model.Outsource.RequestPart;
using VErp.Services.Manafacturing.Model.ProductionStep;
using static VErp.Commons.Enums.Manafacturing.EnumOutsourceTrack;
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
                var request = _mapper.Map<OutsourcePartRequest>(req as OutsourcePartRequestModel);
                request.OutsourcePartRequestCode = generated.CustomCode;

                _manufacturingDBContext.OutsourcePartRequest.Add(request);
                await _manufacturingDBContext.SaveChangesAsync();

                // Create order detail
                var requestDetails = new List<OutsourcePartRequestDetail>();
                foreach (var data in req.OutsourcePartRequestDetail)
                {
                    data.OutsourcePartRequestId = request.OutsourcePartRequestId;
                    var entity = _mapper.Map<OutsourcePartRequestDetail>(data as RequestOutsourcePartDetailModel);
                    requestDetails.Add(entity);
                }

                await _manufacturingDBContext.OutsourcePartRequestDetail.AddRangeAsync(requestDetails);
                await _manufacturingDBContext.SaveChangesAsync();

                //Check valid với quy trinh san xuat
                request.MarkInvalid = await MarkValidateOutsourcePartRequest(req.ProductionOrderId, requestDetails);
                await _manufacturingDBContext.SaveChangesAsync();

                if (customGenCodeId > 0)
                {
                    await _customGenCodeHelperService.ConfirmCode(currentConfig.CurrentLastValue);
                }

                trans.Commit();
                await _activityLogService.CreateLog(EnumObjectType.ProductionOrder, request.OutsourcePartRequestId, $"Thêm mới yêu cầu gia công chi tiết {request.OutsourcePartRequestId}", request.JsonSerialize());

                return request.OutsourcePartRequestId;
            }
            catch (Exception ex)
            {
                trans.TryRollbackTransaction();
                _logger.LogError(ex, "CreateRequestOutsourcePart");
                throw;
            }
        }

        public async Task<OutsourcePartRequestInfo> GetOutsourcePartRequestExtraInfo(long outsourcePartRequestId = 0)
        {
            var sql = new StringBuilder("SELECT * FROM vOutsourcePartRequestExtractInfo v WHERE v.OutsourcePartRequestId = @OutsourcePartRequestId");

            var parammeters = new List<SqlParameter>();
            parammeters.Add(new SqlParameter("@OutsourcePartRequestId", outsourcePartRequestId));

            var extractInfo = (await _manufacturingDBContext.QueryDataTable(sql.ToString(), parammeters.Select(p => p.CloneSqlParam()).ToArray()))
                .ConvertData<OutsourcePartRequestDetailExtractInfo>()
                .AsQueryable()
                .ProjectTo<OutsourcePartRequestDetailInfo>(_mapper.ConfigurationProvider)
                .ToList();

            if (extractInfo.Count == 0)
                throw new BadRequestException(OutsourceErrorCode.NotFoundRequest);

            var rs = _mapper.Map<OutsourcePartRequestInfo>(extractInfo[0]);
            rs.OutsourcePartRequestDetail = extractInfo.Where(x => x.OutsourcePartRequestDetailId > 0).ToList();
            return rs;
        }

        public async Task<bool> UpdateOutsourcePartRequest(long OutsourcePartRequestId, OutsourcePartRequestInfo req)
        {
            var request = await _manufacturingDBContext.OutsourcePartRequest.FirstOrDefaultAsync(x => x.OutsourcePartRequestId == OutsourcePartRequestId);
            if (request == null)
                throw new BadRequestException(OutsourceErrorCode.NotFoundRequest, $"Không tìm thấy yêu cầu gia công có mã là {OutsourcePartRequestId}");

            var details = _manufacturingDBContext.OutsourcePartRequestDetail.Where(x => x.OutsourcePartRequestId == OutsourcePartRequestId).ToList();
            using var trans = await _manufacturingDBContext.Database.BeginTransactionAsync();
            try
            {
                // update order
                _mapper.Map(req, request);

                //Valid Update and action
                foreach (var u in details)
                {
                    var s = req.OutsourcePartRequestDetail.FirstOrDefault(x => x.OutsourcePartRequestDetailId == u.OutsourcePartRequestDetailId);
                    if (s != null)
                        _mapper.Map(s, u);
                    else
                        u.IsDeleted = true;
                }

                // create new detail
                var newRequestDetails = req.OutsourcePartRequestDetail
                    .Where(x => !details.Select(x => x.OutsourcePartRequestDetailId).Contains(x.OutsourcePartRequestDetailId))
                    .AsQueryable()
                    .ProjectTo<OutsourcePartRequestDetail>(_mapper.ConfigurationProvider)
                    .ToList();
                newRequestDetails.ForEach(x => x.OutsourcePartRequestId = request.OutsourcePartRequestId);

                await _manufacturingDBContext.OutsourcePartRequestDetail.AddRangeAsync(newRequestDetails);
                await _manufacturingDBContext.SaveChangesAsync();

                //Check valid với quy trinh san xuat
                newRequestDetails.AddRange(details.Where(x => !x.IsDeleted).ToList());
                request.MarkInvalid = await MarkValidateOutsourcePartRequest(req.ProductionOrderId, newRequestDetails);
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

        public async Task<PageData<OutsourcePartRequestDetailInfo>> GetListOutsourcePartRequest(string keyword, int page, int size, long fromDate, long toDate, Clause filters = null)
        {
            keyword = (keyword ?? "").Trim().ToLower();

            var parammeters = new List<SqlParameter>();
            var whereCondition = new StringBuilder();

            if (!string.IsNullOrEmpty(keyword))
            {
                whereCondition.Append("(v.ProductionOrderCode LIKE @KeyWord ");
                whereCondition.Append("OR v.ProductCode LIKE @Keyword ");
                whereCondition.Append("OR v.ProductName LIKE @Keyword ");
                whereCondition.Append("OR v.OutsourcePartRequestCode LIKE @Keyword ");
                whereCondition.Append("OR v.ProductPartName LIKE @Keyword ) ");
                parammeters.Add(new SqlParameter("@Keyword", $"%{keyword}%"));
            }

            if(fromDate > 0 && toDate > 0)
            {
                if (whereCondition.Length > 0) whereCondition.Append(" AND ");
                whereCondition.Append(" (v.OutsourcePartRequestDate >= @FromDate AND v.OutsourcePartRequestDate <= @ToDate) ");
                parammeters.Add(new SqlParameter("@FromDate", fromDate.UnixToDateTime()));
                parammeters.Add(new SqlParameter("@ToDate", toDate.UnixToDateTime()));
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
            var lst = resultData.ConvertData<OutsourcePartRequestDetailExtractInfo>()
                .AsQueryable()
                .ProjectTo<OutsourcePartRequestDetailInfo>(_mapper.ConfigurationProvider)
                .ToList();

            return (lst, total);
        }

        public async Task<IList<OutsourcePartRequestDetailInfo>> GetRequestDetailByArrayRequestId(long[] outsourcePartRequestIds)
        {
            var parammeters = new List<SqlParameter>();
            var whereCondition = new StringBuilder();

            var sql = new StringBuilder("SELECT * FROM vOutsourcePartRequestExtractInfo v ");

            for (int i = 0; i < outsourcePartRequestIds.Length; i++)
            {
                var value = outsourcePartRequestIds[i];
                var keyParameter = $"@OutsourcePartRequestId_{i + 1}";

                if ((i + 1) == outsourcePartRequestIds.Length)
                    whereCondition.Append($"{keyParameter} )");
                else
                    whereCondition.Append($"{keyParameter}, ");
                parammeters.Add(new SqlParameter(keyParameter, value));
            }

            if (whereCondition.Length > 0)
            {
                sql.Append("WHERE v.OutsourcePartRequestId IN (  ");
                sql.Append(whereCondition);
            }

            sql.Append($" ORDER BY v.OutsourcePartRequestId");

            var resultData = await _manufacturingDBContext.QueryDataTable(sql.ToString(), parammeters.Select(p => p.CloneSqlParam()).ToArray());
            var lst = resultData.ConvertData<OutsourcePartRequestDetailExtractInfo>()
                .AsQueryable()
                .ProjectTo<OutsourcePartRequestDetailInfo>(_mapper.ConfigurationProvider)
                .ToList();

            return lst;
        }

        public async Task<bool> DeletedOutsourcePartRequest(long OutsourcePartRequestId)
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
                           where o.OutsourceTypeId == (int)EnumOutsourceType.OutsourcePart
                           select d).GroupBy(x => x.ObjectId).Select(x => new
                           {
                               ObjectId = x.Key,
                               QuantityProcessed = x.Sum(x => x.Quantity)
                           });
                foreach (var detail in details)
                {
                    if (lst.Where(y => y.ObjectId == detail.OutsourcePartRequestDetailId && y.QuantityProcessed > 0).Count() != 0)
                        throw new BadRequestException(OutsourceErrorCode.InValidRequestOutsource, $"Đã có đơn hàng gia công cho yêu cầu {order.OutsourcePartRequestCode}");
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

        public async Task<IList<OutsourcePartRequestDetailInfo>> GetOutsourcePartRequestDetailByProductionOrderId(long productionOrderId)
        {
            var sql = new StringBuilder($"SELECT * FROM vOutsourcePartRequestExtractInfo v WHERE v.ProductionOrderId = {productionOrderId}");
            var resultData = (await _manufacturingDBContext.QueryDataTable(sql.ToString(), Array.Empty<SqlParameter>()))
                .ConvertData<OutsourcePartRequestDetailExtractInfo>()
                .AsQueryable()
                .ProjectTo<OutsourcePartRequestDetailInfo>(_mapper.ConfigurationProvider)
                .ToList();

            return resultData;
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

        private async Task<bool> MarkValidateOutsourcePartRequest(long productionOrderId, IList<OutsourcePartRequestDetail> rqDetails)
        {
            var outsourcePartRequestDetailIds = rqDetails.Select(x => x.OutsourcePartRequestDetailId).ToArray();

            var totalQuantityAllocate = (await _manufacturingDBContext.ProductionStep.AsNoTracking()
                .Include(s => s.ProductionStepLinkDataRole)
                .ThenInclude(r => r.ProductionStepLinkData)
                .Where(x => x.ContainerId == productionOrderId && x.ContainerTypeId == (int)EnumContainerType.ProductionOrder)
                .ProjectTo<ProductionStepInfo>(_mapper.ConfigurationProvider).ToListAsync())
                .SelectMany(x => x.ProductionStepLinkDatas)
                .Where(x => outsourcePartRequestDetailIds.Contains(x.OutsourceRequestDetailId.GetValueOrDefault()))
                .GroupBy(x => x.OutsourceRequestDetailId.GetValueOrDefault())
                .ToDictionary(k => k.Key, v => v.Sum(x => x.Quantity));

            if (totalQuantityAllocate.Count() > 0)
            {
                foreach (var rqd in rqDetails)
                    if (!totalQuantityAllocate.ContainsKey(rqd.OutsourcePartRequestDetailId)
                        || totalQuantityAllocate[rqd.OutsourcePartRequestDetailId] != rqd.Quantity)
                        return true;

                return false;
            }

            return true;
        }

        public async Task<bool> UpdateOutsourcePartRequestStatus(long[] outsourcePartRequestId)
        {
            var lsOutsourceRequest = await _manufacturingDBContext.OutsourcePartRequest
                .Include(x => x.OutsourcePartRequestDetail)
                .Where(x => outsourcePartRequestId.Contains(x.OutsourcePartRequestId))
                .ToListAsync();
            foreach(var rq in lsOutsourceRequest)
            {
                var outsourcePartRequestDetailIds = rq.OutsourcePartRequestDetail.Select(x => x.OutsourcePartRequestDetailId);

                var outsourceOrderDetails = await _manufacturingDBContext.OutsourceOrderDetail.AsNoTracking()
                    .Where(x => x.OutsourceOrder.OutsourceTypeId == (int)EnumOutsourceType.OutsourcePart
                        && outsourcePartRequestDetailIds.Contains(x.ObjectId))
                    .ToListAsync();

                var outsourceOrderIds = outsourceOrderDetails.Select(x => x.OutsourceOrderId).Distinct();

                var totalStatus = (await _manufacturingDBContext.OutsourceTrack.AsNoTracking()
                    .Where(x => outsourceOrderIds.Contains(x.OutsourceOrderId)
                        && (!x.ObjectId.HasValue || outsourcePartRequestDetailIds.Contains(x.ObjectId.GetValueOrDefault())))
                    .ToListAsync())
                    .GroupBy(x => x.OutsourceOrderId)
                    .Select(g => g.OrderByDescending(x => x.OutsourceTrackId).Take(1).FirstOrDefault()?.OutsourceTrackStatusId)
                    .Sum();

                if (totalStatus.GetValueOrDefault() == 0)
                    rq.OutsourcePartRequestStatusId = (int)EnumOutsourceRequestStatusType.Unprocessed;
                else
                {
                    var quantityOrderByRequestDetail = outsourceOrderDetails.GroupBy(x => x.ObjectId)
                                    .ToDictionary(k => k.Key, v => v.Sum(x => x.Quantity));

                    var isCheckOrder = false;
                    foreach (var d in rq.OutsourcePartRequestDetail)
                    {
                        if (!quantityOrderByRequestDetail.ContainsKey(d.OutsourcePartRequestDetailId)
                            || (d.Quantity - quantityOrderByRequestDetail[d.OutsourcePartRequestDetailId]) != 0)
                        {
                            isCheckOrder = false;
                            break;
                        }

                        isCheckOrder = true;
                    }
                    if (isCheckOrder && (totalStatus.GetValueOrDefault() == ((int)EnumOutsourceTrackStatus.HandedOver * outsourceOrderIds.Count())))
                        rq.OutsourcePartRequestStatusId = (int)EnumOutsourceRequestStatusType.Processed;
                    else rq.OutsourcePartRequestStatusId = (int)EnumOutsourceRequestStatusType.Processing;
                }
            }
            await _manufacturingDBContext.SaveChangesAsync();
            return true;
        }

    }
}
