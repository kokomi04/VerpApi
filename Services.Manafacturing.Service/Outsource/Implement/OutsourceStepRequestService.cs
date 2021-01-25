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
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Manafacturing.Model.Outsource.RequestStep;
using VErp.Services.Manafacturing.Model.ProductionOrder;
using VErp.Services.Manafacturing.Model.ProductionStep;
using static VErp.Commons.Enums.Manafacturing.EnumOutsourceTrack;
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

        public async Task<PageData<OutsourceStepRequestSearch>> SearchOutsourceStepRequest(string keyword, int page, int size, string orderByFieldName, bool asc, Clause filters = null)
        {
            var sql = new StringBuilder(@"SELECT * FROM vOutsourceStepRequestExtractInfo2 v ");
            var totalSql = new StringBuilder(@"SELECT COUNT(v.OutsourceStepRequestId) Total FROM vOutsourceStepRequestExtractInfo2 v ");

            var parammeters = new List<SqlParameter>();
            var whereCondition = new StringBuilder();

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

            if (whereCondition.Length > 0)
            {
                sql.Append(" AND ");
                sql.Append(whereCondition);
            }

            orderByFieldName = string.IsNullOrWhiteSpace(orderByFieldName) ? "OutsourceStepRequestId" : orderByFieldName;
            sql.Append($" ORDER BY v.[{orderByFieldName}] {(asc ? "" : "DESC")}");

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

            var queryData = (await _manufacturingDBContext.QueryDataTable(sql.ToString(), parammeters.Select(p => p.CloneSqlParam()).ToArray()))
                .ConvertData<OutsourceStepRequestExtractInfo>()
                .AsQueryable()
                .ProjectTo<OutsourceStepRequestSearch>(_mapper.ConfigurationProvider)
                .ToList();

            var outsourceStepRequest = await _manufacturingDBContext.OutsourceStepRequest.AsNoTracking()
                .Include(x => x.OutsourceStepRequestData)
                .Where(x => queryData.Select(y => y.OutsourceStepRequestId).Contains(x.OutsourceStepRequestId))
                .ProjectTo<OutsourceStepRequestModel>(_mapper.ConfigurationProvider)
                .ToListAsync();

            // Tìm kiếm những công đoạn trong YCGC
            var byProductionOrder = outsourceStepRequest.GroupBy(x => x.ProductionOrderId);
            var stepMapping = new Dictionary<long, IEnumerable<ProductionStepSimpleModel>>();

            foreach (var group in byProductionOrder)
            {
                var steps = await _manufacturingDBContext.ProductionStep.AsNoTracking()
                .Include(x => x.Step)
                .Include(s => s.ProductionStepLinkDataRole)
                .Where(x => x.ContainerId == group.Key && x.ContainerTypeId == (int)EnumContainerType.ProductionOrder)
                .ToListAsync();
                var roles = steps
                .SelectMany(x => x.ProductionStepLinkDataRole, (s, d) => new ProductionStepLinkDataRoleModel
                {
                    ProductionStepId = s.ProductionStepId,
                    ProductionStepLinkDataId = d.ProductionStepLinkDataId,
                    ProductionStepLinkDataRoleTypeId = (EnumProductionStepLinkDataRoleType)d.ProductionStepLinkDataRoleTypeId,
                }).ToList();

                foreach (var rq in group)
                {
                    var stepIds = FoundProductionStepInOutsourceStepRequest(rq.OutsourceStepRequestData, roles);

                    stepMapping.Add(rq.OutsourceStepRequestId,
                            steps.Where(x => stepIds.Contains(x.ProductionStepId))
                            .Select(x => new ProductionStepSimpleModel
                            {
                                Title = x.Step?.StepName,
                                ProductionStepCode = x.ProductionStepCode,
                                ProductionStepId = x.ProductionStepId
                            }));
                }
            }

            queryData.ForEach(l => l.ProductionSteps = stepMapping[l.OutsourceStepRequestId]);

            return (queryData, total);
        }

        public async Task<OutsourceStepRequestOutput> GetOutsourceStepRequestOutput(long outsourceStepRequestId)
        {
            var sqlRequest = new StringBuilder(@$"SELECT * FROM vOutsourceStepRequestExtractInfo2 v WHERE v.OutsourceStepRequestId = {outsourceStepRequestId}");
            var sqlData = new StringBuilder(@$"SELECT * FROM vOutsourceStepRequestDataExtractInfo v WHERE v.OutsourceStepRequestId = {outsourceStepRequestId}");

            var outsourceStepRequest = (await _manufacturingDBContext.QueryDataTable(sqlRequest.ToString(), Array.Empty<SqlParameter>()))
                   .ConvertData<OutsourceStepRequestExtractInfo>()
                   .AsQueryable()
                   .ProjectTo<OutsourceStepRequestOutput>(_mapper.ConfigurationProvider)
                   .FirstOrDefault();
            if (outsourceStepRequest == null)
                throw new BadRequestException(OutsourceErrorCode.NotFoundRequest);

            var outsourceStepRequestDatas = (await _manufacturingDBContext.QueryDataTable(sqlData.ToString(), Array.Empty<SqlParameter>()))
                  .ConvertData<OutsourceStepRequestDataOutput>();

            var itemOutput = outsourceStepRequestDatas.FirstOrDefault(x => x.ProductionStepLinkDataRoleTypeId == EnumProductionStepLinkDataRoleType.Output);
            var percent = itemOutput.OutsourceStepRequestDataQuantityProcessed / itemOutput.OutsourceStepRequestDataQuantity;
            outsourceStepRequestDatas.ForEach(x =>
            {
                if (x.ProductionStepLinkDataRoleTypeId == EnumProductionStepLinkDataRoleType.Input)
                    x.OutsourceStepRequestDataQuantityProcessed = (decimal)(percent * x.OutsourceStepRequestDataQuantity);
            });


            outsourceStepRequest.OutsourceStepRequestDatas.AddRange(outsourceStepRequestDatas);

            return outsourceStepRequest;
        }

        public async Task<long> CreateOutsourceStepRequest(OutsourceStepRequestModel req)
        {
            /*
             * Validate các công đoạn gia công đã tồn tại trong YCGC nào hay chưa?
             */
            var productionStepHadOutsourceRequest = await GetProductionStepHadOutsourceStepRequest(req.ProductionOrderId);
            List<long> lsProductionStepId = await GetProductionStepInOutsourceStepRequest(req);

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
                outsourceStepRequest.MarkInvalid = false;
                outsourceStepRequest.OutsourceStepRequestStatusId = (int)EnumOutsourceRequestStatusType.Unprocessed;

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
                await UpdateProductionStepLinkDataRelative(outsourceStepRequest.OutsourceStepRequestId, outsourceStepRequestDatas, lsProductionStepId, (decimal)(tData.Quantity / linkData.Quantity));

                await _customGenCodeHelperService.ConfirmCode(currentConfig.CurrentLastValue);
                await trans.CommitAsync();

                await _activityLogService.CreateLog(EnumObjectType.OutsourceRequest, outsourceStepRequest.OutsourceStepRequestId,
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
                outsourceStepRequest.MarkInvalid = false;

                var outsourceStepRequestDataOld = await _manufacturingDBContext.OutsourceStepRequestData
                    .Where(d => d.OutsourceStepRequestId == outsourceStepRequestId)
                    .ToListAsync();
                var outsourceStepRequestDataNew = _mapper.Map<List<OutsourceStepRequestData>>(req.OutsourceStepRequestData);

                _manufacturingDBContext.OutsourceStepRequestData.RemoveRange(outsourceStepRequestDataOld);
                await _manufacturingDBContext.OutsourceStepRequestData.AddRangeAsync(outsourceStepRequestDataNew);
                await _manufacturingDBContext.SaveChangesAsync();

                // Update outsourceQuanity của LinkData liên quan
                List<long> lsProductionStepId = await GetProductionStepInOutsourceStepRequest(req);

                var d_1 = req.OutsourceStepRequestData
                    .FirstOrDefault(x => x.ProductionStepLinkDataRoleTypeId == EnumProductionStepLinkDataRoleType.Output);

                var d_2 = outsourceStepRequestDataOld
                    .FirstOrDefault(x => x.ProductionStepLinkDataId == d_1.ProductionStepLinkDataId);

                var quantityOrigin = (await _manufacturingDBContext.ProductionStepLinkData
                    .FirstOrDefaultAsync(x => x.ProductionStepLinkDataId == d_1.ProductionStepLinkDataId))
                    ?.Quantity;

                var newPercent = (decimal)(d_1.OutsourceStepRequestDataQuantity / quantityOrigin);
                var oldPercent = (decimal)(d_2.Quantity / quantityOrigin);

                await UpdateProductionStepLinkDataRelative(outsourceStepRequest.OutsourceStepRequestId, outsourceStepRequestDataNew, lsProductionStepId, newPercent, oldPercent);

                await trans.CommitAsync();
                await _activityLogService.CreateLog(EnumObjectType.OutsourceRequest, outsourceStepRequest.OutsourceStepRequestId,
                    $"Cập nhật yêu cầu gia công công đoạn", req.JsonSerialize());

                return true;
            }
            catch (Exception ex)
            {
                await trans.TryRollbackTransactionAsync();
                _logger.LogError(ex, "UpdateOutsourceStepRequest");
                throw;
            }
        }

        private async Task<List<long>> GetProductionStepInOutsourceStepRequest(OutsourceStepRequestModel req)
        {
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
            return lsProductionStepId;
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
                           where o.OutsourceTypeId == (int)EnumOutsourceType.OutsourceStep
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

                await UpdateProductionStepLinkDataRelative(outsourceStepRequest.OutsourceStepRequestId, outsourceStepRequestDataOld, lsProductionStepId, 0, oldPercent);

                //commit
                await trans.CommitAsync();
                await _activityLogService.CreateLog(EnumObjectType.OutsourceRequest, outsourceStepRequest.OutsourceStepRequestId,
                    $"Xóa yêu cầu gia công công đoạn", outsourceStepRequest.JsonSerialize());
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
                        ProductionStepLinkDataTitle = string.Empty,
                        OutsourceStepRequestFinishDate = outsourceStepRequest.OutsourceStepRequestFinishDate
                    })
                    .ToList();
                if (outsourceStepRequestDatas.Count == 0)
                    lst.Add(new OutsourceStepRequestDataInfo
                    {
                        OutsourceStepRequestCode = outsourceStepRequest.OutsourceStepRequestCode,
                        OutsourceStepRequestId = outsourceStepRequest.OutsourceStepRequestId,
                        ProductionStepId = productionStep.ProductionStepId,
                        ProductionStepTitle = productionStep.Title,
                        ProductionStepLinkDataRoleTypeId = EnumProductionStepLinkDataRoleType.Input,
                        OutsourceStepRequestFinishDate = outsourceStepRequest.OutsourceStepRequestFinishDate
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

        public async Task<IList<ProductionStepInOutsourceStepRequest>> GetProductionStepHadOutsourceStepRequest(long productionOrderId)
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
               .SelectMany(x => x.ProductionStepLinkDataRole, (s, d) => new ProductionStepLinkDataRoleInput
               {
                   ProductionStepId = s.ProductionStepId,
                   ProductionStepLinkDataId = d.ProductionStepLinkDataId,
                   ProductionStepLinkDataRoleTypeId = (EnumProductionStepLinkDataRoleType)d.ProductionStepLinkDataRoleTypeId,
                   ProductionStepCode = s.ProductionStepCode,
               }).ToListAsync();

            foreach (var request in outsourceStepRequest)
            {
                var lst = FoundProductionStepInOutsourceStepRequest(request.OutsourceStepRequestData, roles.Cast<ProductionStepLinkDataRoleModel>().ToList())
                    .Select(productionStepId => new ProductionStepInOutsourceStepRequest
                    {
                        ProductionProcessId = request.ProductionProcessId,
                        ProductionStepId = productionStepId,
                        OutsourceStepRequestCode = request.OutsourceStepRequestCode,
                        OutsourceStepRequestId = request.OutsourceStepRequestId,
                        ProductionStepCode = roles.FirstOrDefault(x => x.ProductionStepId == productionStepId)?.ProductionStepCode
                    });
                data.AddRange(lst);
            }

            return data;
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

        public async Task<bool> UpdateOutsourceStepRequestStatus(long[] outsourceStepRequestId)
        {
            var lsOutsourceRequest = await _manufacturingDBContext.OutsourceStepRequest
                .Include(x => x.OutsourceStepRequestData)
                .Where(x => outsourceStepRequestId.Contains(x.OutsourceStepRequestId))
                .ToListAsync();
            foreach (var rq in lsOutsourceRequest)
            {
                var productionLinkDataIds = rq.OutsourceStepRequestData.Select(x => x.ProductionStepLinkDataId);

                var outsourceOrderDetails = await _manufacturingDBContext.OutsourceOrderDetail.AsNoTracking()
                    .Where(x => x.OutsourceOrder.OutsourceTypeId == (int)EnumOutsourceType.OutsourceStep
                        && productionLinkDataIds.Contains(x.ObjectId))
                    .ToListAsync();

                var outsourceOrderIds = outsourceOrderDetails.Select(x => x.OutsourceOrderId);

                var totalStatus = (await _manufacturingDBContext.OutsourceTrack.AsNoTracking()
                    .Where(x => outsourceOrderIds.Contains(x.OutsourceOrderId)
                        && (!x.ObjectId.HasValue || productionLinkDataIds.Contains(x.ObjectId.GetValueOrDefault())))
                    .ToListAsync())
                    .GroupBy(x => x.OutsourceOrderId)
                    .Select(g => g.OrderByDescending(x => x.OutsourceTrackId).Take(1).FirstOrDefault()?.OutsourceTrackStatusId)
                    .Sum();

                if (!totalStatus.HasValue)
                    rq.OutsourceStepRequestStatusId = (int)EnumOutsourceRequestStatusType.Unprocessed;
                else
                {
                    var quantityOrderByRequestDetail = outsourceOrderDetails.GroupBy(x => x.ObjectId)
                                    .ToDictionary(k => k.Key, v => v.Sum(x => x.Quantity));

                    var isCheckOrder = false;
                    foreach (var d in rq.OutsourceStepRequestData)
                    {
                        if (!quantityOrderByRequestDetail.ContainsKey(d.ProductionStepLinkDataId)
                            || (d.Quantity - quantityOrderByRequestDetail[d.ProductionStepLinkDataId]) != 0)
                        {
                            isCheckOrder = false;
                            break;
                        }

                        isCheckOrder = true;
                    }
                    if (isCheckOrder && (totalStatus.GetValueOrDefault() == ((int)EnumOutsourceTrackStatus.HandedOver * outsourceOrderIds.Count())))
                        rq.OutsourceStepRequestStatusId = (int)EnumOutsourceRequestStatusType.Processed;
                    else rq.OutsourceStepRequestStatusId = (int)EnumOutsourceRequestStatusType.Processing;
                }
            }
            await _manufacturingDBContext.SaveChangesAsync();
            return true;
        }

        #region private
        private async Task<IList<ProductionStepLinkDataInput>> GetProductionStepLinkDataByListId(List<long> lsProductionStepLinkDataId)
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

        private async Task<bool> UpdateProductionStepLinkDataRelative(long outsourceStepRequestId, IList<OutsourceStepRequestData> outsourceStepRequestDatas, IList<long> lsProductionStep, decimal newPercent, decimal oldPercent = 0)
        {
            var productionSteps = await _manufacturingDBContext.ProductionStep.AsNoTracking()
                                        .Where(x => lsProductionStep.Contains(x.ProductionStepId))
                                        .ProjectTo<ProductionStepInfo>(_mapper.ConfigurationProvider)
                                        .ToListAsync();

            var selectLinkDataIds = productionSteps.SelectMany(x => x.ProductionStepLinkDatas.Where(x => x.ProductionStepLinkDataTypeId == EnumProductionStepLinkDataType.None)).Select(x => x.ProductionStepLinkDataId).Distinct().ToList();
            var productionStepLinkDataEntity = await _manufacturingDBContext.ProductionStepLinkData
                                                        .Where(x => selectLinkDataIds.Contains(x.ProductionStepLinkDataId))
                                                        .ToListAsync();
            var productionStepLinkDataInfo = await GetProductionStepLinkDataByListId(productionStepLinkDataEntity.Select(x => x.ProductionStepLinkDataId).ToList());

            foreach (var linkData in productionStepLinkDataEntity)
            {
                var info = productionStepLinkDataInfo.FirstOrDefault(x => x.ProductionStepLinkDataId == linkData.ProductionStepLinkDataId);

                if (!linkData.OutsourceQuantity.HasValue)
                    linkData.OutsourceQuantity = decimal.Zero;
                if (!linkData.ExportOutsourceQuantity.HasValue)
                    linkData.ExportOutsourceQuantity = decimal.Zero;
                var oldValue = linkData.Quantity * oldPercent;
                var newValue = linkData.Quantity * newPercent;


                var requestData = outsourceStepRequestDatas.FirstOrDefault(x => x.ProductionStepLinkDataId == linkData.ProductionStepLinkDataId);

                if (requestData != null && requestData.ProductionStepLinkDataRoleTypeId == (int)EnumProductionStepLinkDataRoleType.Input)
                {
                    linkData.ExportOutsourceQuantity += (newValue - oldValue);
                    if (linkData.ExportOutsourceQuantity > linkData.Quantity)
                        throw new BadRequestException(OutsourceErrorCode.InValidRequestOutsource, $"Số lượng gia công của chi tiết \"{info.ObjectTitle}\" vượt qua cho phép");
                }
                else
                {
                    linkData.OutsourceQuantity += (newValue - oldValue);
                    if (linkData.OutsourceQuantity > linkData.Quantity && requestData != null)
                        throw new BadRequestException(OutsourceErrorCode.InValidRequestOutsource, $"Số lượng gia công của chi tiết \"{info.ObjectTitle}\" vượt qua cho phép");
                }
            }

            await _manufacturingDBContext.SaveChangesAsync();

            //if (oldPercent == decimal.Zero)
            //    await CreateLinkDataAndRoleOutsourceStep(outsourceStepRequestDatas, productionStepLinkDataEntity);
            //else if (newPercent == decimal.Zero)
            //    await DeleteLinkDataAndRoleOutsourceStep(outsourceStepRequestId);
            //else
            //    await UpdateLinkDataAndRoleOutsourceStep(outsourceStepRequestId, productionStepLinkDataEntity);
            return true;
        }

        private async Task CreateLinkDataAndRoleOutsourceStep(List<OutsourceStepRequestData> outsourceStepRequestDatas, List<ProductionStepLinkData> productionStepLinkDataEntity)
        {
            var roles = await _manufacturingDBContext.ProductionStepLinkDataRole.AsNoTracking()
                                        .Where(x => outsourceStepRequestDatas.Select(y => y.ProductionStepLinkDataId).Contains(x.ProductionStepLinkDataId))
                                        .ToListAsync();

            var newStepLinkDataEntity = new List<ProductionStepLinkData>();
            var newRoleEntity = new List<ProductionStepLinkDataRoleInput>();

            foreach (var outsourceData in outsourceStepRequestDatas)
            {
                var roleType = (int)EnumProductionStepLinkDataRoleType.Input;
                if (outsourceData.ProductionStepLinkDataRoleTypeId == (int)EnumProductionStepLinkDataRoleType.Input)
                    roleType = (int)EnumProductionStepLinkDataRoleType.Output;

                var linkData = productionStepLinkDataEntity.FirstOrDefault(x => x.ProductionStepLinkDataId == outsourceData.ProductionStepLinkDataId);
                var role = roles.FirstOrDefault(x => x.ProductionStepLinkDataRoleTypeId == roleType && x.ProductionStepLinkDataId == linkData.ProductionStepLinkDataId);
                if (role == null) continue;

                var nLinkData = new ProductionStepLinkData
                {
                    ProductionStepLinkDataCode = $"{linkData.ProductionStepLinkDataCode}/{outsourceData.OutsourceStepRequestId}",
                    Quantity = linkData.OutsourceQuantity.Value,
                    ObjectId = linkData.ObjectId,
                    ObjectTypeId = linkData.ObjectTypeId,
                    OutsourceRequestDetailId = outsourceData.OutsourceStepRequestId,
                    ProductionStepLinkDataTypeId = (int)EnumProductionStepLinkDataType.StepLinkDataOutsourceStep,
                };
                var nRole = new ProductionStepLinkDataRoleInput
                {
                    ProductionStepId = role.ProductionStepId,
                    ProductionStepLinkDataRoleTypeId = (EnumProductionStepLinkDataRoleType)role.ProductionStepLinkDataRoleTypeId,
                    ProductionStepLinkDataCode = nLinkData.ProductionStepLinkDataCode
                };

                newStepLinkDataEntity.Add(nLinkData);
                newRoleEntity.Add(nRole);
            }

            await _manufacturingDBContext.ProductionStepLinkData.AddRangeAsync(newStepLinkDataEntity);
            await _manufacturingDBContext.SaveChangesAsync();
            foreach (var r in newRoleEntity)
            {
                var d = newStepLinkDataEntity.FirstOrDefault(x => x.ProductionStepLinkDataCode == r.ProductionStepLinkDataCode);
                r.ProductionStepLinkDataId = d.ProductionStepLinkDataId;
            }
            await _manufacturingDBContext.ProductionStepLinkDataRole.AddRangeAsync(_mapper.Map<IList<ProductionStepLinkDataRole>>(newRoleEntity));
            await _manufacturingDBContext.SaveChangesAsync();
        }

        private async Task UpdateLinkDataAndRoleOutsourceStep(long outsourceStepRequestId, List<ProductionStepLinkData> productionStepLinkDataEntity)
        {
            var linkDataOutsource = await _manufacturingDBContext.ProductionStepLinkData
                                            .Where(x => x.ProductionStepLinkDataTypeId == (int)EnumProductionStepLinkDataType.StepLinkDataOutsourceStep
                                                      && x.OutsourceRequestDetailId == outsourceStepRequestId)
                                            .ToListAsync();
            foreach (var ld in linkDataOutsource)
            {
                var t = productionStepLinkDataEntity.FirstOrDefault(x => x.ProductionStepLinkDataCode == ld.ProductionStepLinkDataCode.Substring(0, ld.ProductionStepLinkDataCode.IndexOf("/")));
                ld.Quantity = t.OutsourceQuantity.Value;
            }

            await _manufacturingDBContext.SaveChangesAsync();
        }

        private async Task DeleteLinkDataAndRoleOutsourceStep(long outsourceStepRequestId)
        {
            var linkDataOutsource = await _manufacturingDBContext.ProductionStepLinkData
                                            .Where(x => x.ProductionStepLinkDataTypeId == (int)EnumProductionStepLinkDataType.StepLinkDataOutsourceStep
                                                      && x.OutsourceRequestDetailId == outsourceStepRequestId)
                                            .ToListAsync();
            var roles = await _manufacturingDBContext.ProductionStepLinkDataRole
                                .Where(x => linkDataOutsource.Select(y => y.ProductionStepLinkDataId).Contains(x.ProductionStepLinkDataId))
                                .ToListAsync();

            linkDataOutsource.ForEach(x => x.IsDeleted = true);
            _manufacturingDBContext.ProductionStepLinkDataRole.RemoveRange(roles);

            await _manufacturingDBContext.SaveChangesAsync();
        }
        #endregion
    }
}
