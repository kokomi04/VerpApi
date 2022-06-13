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
using ProductionHandoverEntity = VErp.Infrastructure.EF.ManufacturingDB.ProductionHandover;
using static VErp.Commons.Enums.Manafacturing.EnumProductionProcess;
using VErp.Services.Manafacturing.Model.ProductionHandover;
using ProductionAssignmentEntity = VErp.Infrastructure.EF.ManufacturingDB.ProductionAssignment;
using VErp.Services.Manafacturing.Model.ProductionOrder.Materials;
using Newtonsoft.Json;
using VErp.Services.Manafacturing.Service.StatusProcess.Implement;
using static VErp.Commons.GlobalObject.QueueName.ManufacturingQueueNameConstants;

namespace VErp.Services.Manafacturing.Service.ProductionHandover.Implement
{
    public class ProductionHandoverService : StatusProcessService, IProductionHandoverService
    {
        private readonly ManufacturingDBContext _manufacturingDBContext;
        private readonly ICurrentContextService _currentContextService;
        private readonly IActivityLogService _activityLogService;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private const int STOCK_DEPARTMENT_ID = -1;
        private readonly IQueueProcessHelperService _queueProcessHelperService;

        public ProductionHandoverService(ManufacturingDBContext manufacturingDB
            , IActivityLogService activityLogService
            , ILogger<ProductionHandoverService> logger
            , IMapper mapper
            , ICurrentContextService currentContextService, IQueueProcessHelperService queueProcessHelperService) : base(manufacturingDB, activityLogService, logger, mapper)
        {
            _manufacturingDBContext = manufacturingDB;
            _activityLogService = activityLogService;
            _logger = logger;
            _mapper = mapper;
            _currentContextService = currentContextService;
            _queueProcessHelperService = queueProcessHelperService;
        }

        public async Task<bool> AcceptProductionHandoverBatch(IList<ProductionHandoverAcceptBatchInput> req)
        {
            var handoverIds = req.Select(h => h.productionHandoverId).Distinct().ToList();
            var productionHandovers = await _manufacturingDBContext.ProductionHandover.Where(ho => handoverIds.Contains(ho.ProductionHandoverId)).ToListAsync();

            foreach (var item in req)
            {
                var info = productionHandovers.FirstOrDefault(ho => ho.ProductionOrderId == item.ProductionOrderId && ho.ProductionHandoverId == item.productionHandoverId);

                if (info == null)
                {
                    throw new BadRequestException(GeneralCode.InvalidParams, "Bàn giao công việc không tồn tại");
                }
                if (info.Status != (int)EnumHandoverStatus.Waiting) throw new BadRequestException(GeneralCode.InvalidParams, "Chỉ được phép xác nhận các bàn giao đang chờ xác nhận");

            }

            var productionOrderIds = req.Select(h => h.ProductionOrderId).Distinct().ToList();
            var productionOrderCodes = await _manufacturingDBContext.ProductionOrder.Where(o => productionOrderIds.Contains(o.ProductionOrderId)).Select(o => o.ProductionOrderCode).ToListAsync();

            using (var batchLog = _activityLogService.BeginBatchLog())
            {
                try
                {


                    foreach (var item in req)
                    {
                        var info = productionHandovers.FirstOrDefault(ho => ho.ProductionOrderId == item.ProductionOrderId && ho.ProductionHandoverId == item.productionHandoverId);

                        if (info == null)
                        {
                            throw new BadRequestException(GeneralCode.InvalidParams, "Bàn giao công việc không tồn tại");
                        }
                        if (info.Status != (int)EnumHandoverStatus.Waiting) throw new BadRequestException(GeneralCode.InvalidParams, "Chỉ được phép xác nhận các bàn giao đang chờ xác nhận");
                        info.Status = (int)EnumHandoverStatus.Accepted;
                        info.AcceptByUserId = _currentContextService.UserId;

                        _manufacturingDBContext.SaveChanges();

                        if (info.Status == (int)EnumHandoverStatus.Accepted)
                        {
                            await ChangeAssignedProgressStatus(info.ProductionOrderId, info.FromProductionStepId, info.FromDepartmentId);
                            await ChangeAssignedProgressStatus(info.ProductionOrderId, info.ToProductionStepId, info.ToDepartmentId);
                        }

                        await _activityLogService.CreateLog(EnumObjectType.ProductionHandover, info.ProductionHandoverId, $"Xác nhận bàn giao công việc", info.JsonSerialize());

                    }


                    await batchLog.CommitAsync();

                    foreach (var code in productionOrderCodes)
                    {
                        await _queueProcessHelperService.EnqueueAsync(PRODUCTION_INVENTORY_STATITICS, code);
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "AcceptProductionHandoverBatch");
                    throw;
                }
            }
        }

        public async Task<ProductionHandoverModel> ConfirmProductionHandover(long productionOrderId, long productionHandoverId, EnumHandoverStatus status)
        {
            var productionHandover = _manufacturingDBContext.ProductionHandover.FirstOrDefault(ho => ho.ProductionOrderId == productionOrderId && ho.ProductionHandoverId == productionHandoverId);
            if (productionHandover == null) throw new BadRequestException(GeneralCode.InvalidParams, "Bàn giao công việc không tồn tại");
            if (productionHandover.Status != (int)EnumHandoverStatus.Waiting) throw new BadRequestException(GeneralCode.InvalidParams, "Chỉ được phép xác nhận các bàn giao đang chờ xác nhận");

            var productionOrderCode = await _manufacturingDBContext.ProductionOrder.Where(o => productionOrderId == o.ProductionOrderId).Select(o => o.ProductionOrderCode).FirstOrDefaultAsync();

            try
            {
                productionHandover.Status = (int)status;

                if (status == EnumHandoverStatus.Accepted)
                    productionHandover.AcceptByUserId = _currentContextService.UserId;

                _manufacturingDBContext.SaveChanges();

                if (productionHandover.Status == (int)EnumHandoverStatus.Accepted)
                {
                    await ChangeAssignedProgressStatus(productionOrderId, productionHandover.FromProductionStepId, productionHandover.FromDepartmentId);
                    await ChangeAssignedProgressStatus(productionOrderId, productionHandover.ToProductionStepId, productionHandover.ToDepartmentId);
                }
                await _activityLogService.CreateLog(EnumObjectType.ProductionHandover, productionHandover.ProductionHandoverId, $"Xác nhận bàn giao công việc", productionHandover.JsonSerialize());


                await _queueProcessHelperService.EnqueueAsync(PRODUCTION_INVENTORY_STATITICS, productionOrderCode);


                return _mapper.Map<ProductionHandoverModel>(productionHandover);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreateProductHandover");
                throw;
            }
        }

        public async Task<ProductionHandoverModel> CreateProductionHandover(long productionOrderId, ProductionHandoverInputModel data)
        {
            return await CreateProductionHandover(productionOrderId, data, EnumHandoverStatus.Waiting);
        }

        public async Task<bool> CreateProductionHandoverPatch(IList<ProductionHandoverInputModel> datas)
        {
            var trans = await _manufacturingDBContext.Database.BeginTransactionAsync();
            try
            {
                foreach (var data in datas)
                {
                    await CreateProductionHandover(data.ProductionOrderId, data, EnumHandoverStatus.Waiting);
                }

                await trans.CommitAsync();
                return true;
            }
            catch (System.Exception ex)
            {
                await trans.RollbackAsync();
                _logger.LogError(ex, "CreateProductHandover");
                throw;
            }
        }

        private async Task<ProductionHandoverModel> CreateProductionHandover(long productionOrderId, ProductionHandoverInputModel data, EnumHandoverStatus status)
        {
            try
            {
                if (data.FromDepartmentId == STOCK_DEPARTMENT_ID && data.ToDepartmentId == STOCK_DEPARTMENT_ID)
                {
                    if (!_manufacturingDBContext.OutsourceStepRequestData.Any(o => o.ProductionStepId == data.FromProductionStepId))
                        throw new BadRequestException(GeneralCode.InvalidParams, "Công đoạn giao không có gia công công đoạn");
                    if (!_manufacturingDBContext.OutsourceStepRequestData.Any(o => o.ProductionStepId == data.ToProductionStepId))
                        throw new BadRequestException(GeneralCode.InvalidParams, "Công đoạn nhận không có gia công công đoạn");
                }
                else
                {
                    if (!_manufacturingDBContext.ProductionAssignment.Any(a => a.ProductionStepId == data.FromProductionStepId && a.DepartmentId == data.FromDepartmentId && a.ProductionOrderId == productionOrderId))
                        throw new BadRequestException(GeneralCode.InvalidParams, "Công đoạn giao không tồn tại phân công công việc cho tổ bàn giao");
                    if (!_manufacturingDBContext.ProductionAssignment.Any(a => a.ProductionStepId == data.ToProductionStepId && a.DepartmentId == data.ToDepartmentId && a.ProductionOrderId == productionOrderId))
                        throw new BadRequestException(GeneralCode.InvalidParams, "Công đoạn nhận không tồn tại phân công công việc cho tổ nhận");
                }

                var productionHandover = _mapper.Map<ProductionHandoverEntity>(data);
                productionHandover.Status = (int)status;
                productionHandover.ProductionOrderId = productionOrderId;
                _manufacturingDBContext.ProductionHandover.Add(productionHandover);
                _manufacturingDBContext.SaveChanges();
                if (productionHandover.Status == (int)EnumHandoverStatus.Accepted && data.FromDepartmentId != STOCK_DEPARTMENT_ID && data.ToDepartmentId != STOCK_DEPARTMENT_ID)
                {
                    await ChangeAssignedProgressStatus(productionOrderId, productionHandover.FromProductionStepId, productionHandover.FromDepartmentId);
                    await ChangeAssignedProgressStatus(productionOrderId, productionHandover.ToProductionStepId, productionHandover.ToDepartmentId);
                }
                await _activityLogService.CreateLog(EnumObjectType.ProductionHandover, productionHandover.ProductionHandoverId, $"Tạo bàn giao công việc / yêu cầu xuất kho", data.JsonSerialize());
                return _mapper.Map<ProductionHandoverModel>(productionHandover);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreateProductHandover");
                throw;
            }
        }

        public async Task<bool> DeleteProductionHandover(long productionHandoverId)
        {
            try
            {
                var productionHandover = _manufacturingDBContext.ProductionHandover
                    .Where(h => h.ProductionHandoverId == productionHandoverId)
                    .FirstOrDefault();

                if (productionHandover == null)
                    throw new BadRequestException(GeneralCode.InvalidParams, "Không tồn tại bàn giao công việc");
                productionHandover.IsDeleted = true;
                _manufacturingDBContext.SaveChanges();
                if (productionHandover.Status == (int)EnumHandoverStatus.Accepted)
                {
                    await ChangeAssignedProgressStatus(productionHandover.ProductionOrderId, productionHandover.ToProductionStepId, productionHandover.ToDepartmentId);
                    await ChangeAssignedProgressStatus(productionHandover.ProductionOrderId, productionHandover.FromProductionStepId, productionHandover.FromDepartmentId);
                }
                await _activityLogService.CreateLog(EnumObjectType.ProductionHandover, productionHandoverId, $"Xoá bàn giao công việc", productionHandover.JsonSerialize());
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreateProductHandover");
                throw;
            }
        }

        public async Task<ProductionHandoverModel> CreateStatictic(long productionOrderId, ProductionHandoverInputModel data)
        {
            return await CreateProductionHandover(productionOrderId, data, EnumHandoverStatus.Accepted);
        }

        public async Task<IList<ProductionHandoverModel>> CreateMultipleStatictic(long productionOrderId, IList<ProductionHandoverInputModel> data)
        {
            var insertData = new List<ProductionHandoverEntity>();
            using var trans = await _manufacturingDBContext.Database.BeginTransactionAsync();
            try
            {
                foreach (var item in data)
                {
                    if (item.FromDepartmentId == STOCK_DEPARTMENT_ID && item.ToDepartmentId == STOCK_DEPARTMENT_ID)
                    {
                        if (!_manufacturingDBContext.OutsourceStepRequestData.Any(o => o.ProductionStepId == item.FromProductionStepId))
                            throw new BadRequestException(GeneralCode.InvalidParams, "Công đoạn giao không có gia công công đoạn");
                        if (!_manufacturingDBContext.OutsourceStepRequestData.Any(o => o.ProductionStepId == item.ToProductionStepId))
                            throw new BadRequestException(GeneralCode.InvalidParams, "Công đoạn nhận không có gia công công đoạn");
                    }
                    else
                    {
                        if (!_manufacturingDBContext.ProductionAssignment.Any(a => a.ProductionStepId == item.FromProductionStepId && a.DepartmentId == item.FromDepartmentId && a.ProductionOrderId == productionOrderId))
                            throw new BadRequestException(GeneralCode.InvalidParams, "Công đoạn giao không tồn tại phân công công việc cho tổ bàn giao");
                        if (!_manufacturingDBContext.ProductionAssignment.Any(a => a.ProductionStepId == item.ToProductionStepId && a.DepartmentId == item.ToDepartmentId && a.ProductionOrderId == productionOrderId))
                            throw new BadRequestException(GeneralCode.InvalidParams, "Công đoạn nhận không tồn tại phân công công việc cho tổ nhận");
                    }

                    var productionHandover = _mapper.Map<ProductionHandoverEntity>(item);
                    productionHandover.Status = (int)EnumHandoverStatus.Accepted;
                    productionHandover.ProductionOrderId = productionOrderId;
                    _manufacturingDBContext.ProductionHandover.Add(productionHandover);

                    insertData.Add(productionHandover);
                }

                _manufacturingDBContext.SaveChanges();

                var result = insertData.AsQueryable().ProjectTo<ProductionHandoverModel>(_mapper.ConfigurationProvider).ToList();


                var departmentHandoverDetails = await GetDepartmentHandoverDetail(productionOrderId);

                // Đổi trạng thái phân công
                foreach (var item in insertData)
                {
                    if (item.FromDepartmentId != STOCK_DEPARTMENT_ID && item.ToDepartmentId != STOCK_DEPARTMENT_ID)
                    {
                        await ChangeAssignedProgressStatus(productionOrderId, item.FromProductionStepId, item.FromDepartmentId, null, departmentHandoverDetails);
                        await ChangeAssignedProgressStatus(productionOrderId, item.ToProductionStepId, item.ToDepartmentId, null, departmentHandoverDetails);
                    }

                    await _activityLogService.CreateLog(EnumObjectType.ProductionHandover, item.ProductionHandoverId, $"Tạo bàn giao công việc / yêu cầu xuất kho", data.JsonSerialize());
                }

                trans.Commit();

                return result;
            }
            catch (Exception ex)
            {
                trans.Rollback();
                _logger.LogError(ex, "CreateProductHandover");
                throw;
            }
        }

        public async Task<IList<ProductionHandoverModel>> GetProductionHandovers(long productionOrderId)
        {
            return await _manufacturingDBContext.ProductionHandover
                .Where(h => h.ProductionOrderId == productionOrderId)
                .ProjectTo<ProductionHandoverModel>(_mapper.ConfigurationProvider)
                .ToListAsync();

        }

        public async Task<PageData<DepartmentHandoverModel>> GetDepartmentHandovers(long departmentId, string keyword, int page, int size, long fromDate, long toDate, int? stepId, int? productId, bool? isInFinish, bool? isOutFinish, EnumProductionStepLinkDataRoleType? productionStepLinkDataRoleTypeId)
        {
            keyword = (keyword ?? "").Trim();
            var parammeters = new List<SqlParameter>()
            {
                new SqlParameter("@Keyword", keyword),
                new SqlParameter("@DepartmentId", departmentId),
                new SqlParameter("@Size", size),
                new SqlParameter("@Page", page),
                new SqlParameter("@FromDate", fromDate.UnixToDateTime()),
                new SqlParameter("@ToDate", toDate.UnixToDateTime()),
                new SqlParameter("@StepId", stepId.GetValueOrDefault()),
                new SqlParameter("@ProductId", productId.GetValueOrDefault()),
                new SqlParameter("@ProductionStepLinkDataRoleTypeId", (int?)productionStepLinkDataRoleTypeId),
                new SqlParameter("@IsInFinish", isInFinish),
                new SqlParameter("@IsOutFinish", isOutFinish)
            };

            var dataSet = await _manufacturingDBContext.ExecuteMultipleDataProcedure("asp_ProductionDepartmentHandover", parammeters.ToArray());

            var total = 0;
            IList<DepartmentHandoverModel> lst = null;
            Dictionary<string, object> additionResult = new Dictionary<string, object>();
            if (dataSet != null && dataSet.Tables.Count > 0)
            {
                IList<NonCamelCaseDictionary> data = dataSet.Tables[0].ConvertData();
                total = (data[0]["Total"] as int?).GetValueOrDefault();
                foreach (var item in dataSet.Tables[0].ConvertFirstRowData())
                {
                    additionResult.Add(item.Key, item.Value.value);
                }
                IList<DepartmentHandoverEntity> resultData = dataSet.Tables[1].ConvertData<DepartmentHandoverEntity>();
                lst = resultData.AsQueryable().ProjectTo<DepartmentHandoverModel>(_mapper.ConfigurationProvider).ToList();
            }

            return (lst, total, additionResult);
        }

        public async Task<PageData<ProductionHandoverByDateModel>> GetDepartmentHandoversByDate(IList<long> fromDepartmentIds, IList<long> toDepartmentIds, IList<long> fromStepIds, IList<long> toStepIds, long? fromDate, long? toDate, bool? isInFinish, bool? isOutFinish, int page, int size)
        {
            var parammeters = new List<SqlParameter>()
            {
                fromDepartmentIds.ToSqlParameter("@FromDepartmentIds"),
                toDepartmentIds.ToSqlParameter("@ToDepartmentIds"),
                fromStepIds.ToSqlParameter("@FromStepIds"),
                toStepIds.ToSqlParameter("@ToStepIds"),

                new SqlParameter("@FromDate", EnumDataType.Date.GetSqlValue(fromDate)),
                new SqlParameter("@ToDate", EnumDataType.Date.GetSqlValue(toDate)),
                new SqlParameter("@IsInFinish", isInFinish),
                new SqlParameter("@IsOutFinish", isOutFinish),
                new SqlParameter("@Size", size),
                new SqlParameter("@Page", page),
            };

            var dataTable = await _manufacturingDBContext.ExecuteDataProcedure("asp_ProductionDepartmentHandover_ByDate", parammeters.ToArray());

            var totalRecord = 0;
            if (dataTable.Rows.Count > 0)
            {
                totalRecord = Convert.ToInt32(dataTable.Rows[0]["TotalRecord"]);
            }
            var lst = dataTable.ConvertData<ProductionHandoverByDateModel>();

            return (lst, totalRecord);
        }

    }
}
