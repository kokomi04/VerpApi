using AutoMapper;
using AutoMapper.QueryableExtensions;
using DocumentFormat.OpenXml.EMMA;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NPOI.SS.Formula.Functions;
using Org.BouncyCastle.Ocsp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verp.Resources.Manafacturing.Handover;
using Verp.Resources.Master.Config.ActionButton;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.ManufacturingDB;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper.QueueHelper;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Manafacturing.Model.ProductionHandover;
using VErp.Services.Manafacturing.Model.ProductionOrder;
using VErp.Services.Manafacturing.Service.StatusProcess.Implement;
using static VErp.Commons.Enums.Manafacturing.EnumProductionProcess;
using static VErp.Commons.GlobalObject.QueueName.ManufacturingQueueNameConstants;
using ProductionHandoverEntity = VErp.Infrastructure.EF.ManufacturingDB.ProductionHandover;

namespace VErp.Services.Manafacturing.Service.ProductionHandover.Implement
{
    public class ProductionHandoverReceiptService : StatusProcessService, IProductionHandoverReceiptService
    {
        private readonly ManufacturingDBContext _manufacturingDBContext;
        private readonly ICurrentContextService _currentContextService;
        private readonly ObjectActivityLogFacade _objActivityLogFacade;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private const int STOCK_DEPARTMENT_ID = -1;
        private readonly IProductionOrderQueueHelperService _productionOrderQueueHelperService;
        private readonly ICustomGenCodeHelperService _customGenCodeHelperService;

        public ProductionHandoverReceiptService(ManufacturingDBContext manufacturingDB
            , IActivityLogService activityLogService
            , ILogger<ProductionHandoverReceiptService> logger
            , IMapper mapper
            , ICurrentContextService currentContextService, IQueueProcessHelperService queueProcessHelperService, ICustomGenCodeHelperService customGenCodeHelperService, IProductionOrderQueueHelperService productionOrderQueueHelperService) : base(manufacturingDB, activityLogService, logger, mapper)
        {
            _manufacturingDBContext = manufacturingDB;
            _objActivityLogFacade = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.ProductionHandoverReceipt);
            _logger = logger;
            _mapper = mapper;
            _currentContextService = currentContextService;
            _customGenCodeHelperService = customGenCodeHelperService;
            _productionOrderQueueHelperService = productionOrderQueueHelperService;
        }

        public async Task<PageData<ProductionHandoverHistoryReceiptModel>> GetList(string keyword, long? fromDate, long? toDate, int page, int size, string orderByFieldName, bool asc, Clause filters = null)
        {
            keyword = (keyword ?? "").Trim();
            var parammeters = new List<SqlParameter>();

            var whereCondition = new StringBuilder();


            if (!string.IsNullOrEmpty(keyword))
            {
                if (whereCondition.Length > 0)
                    whereCondition.Append(" AND ");

                whereCondition.Append("(v.ProductionHandoverReceiptCode LIKE @KeyWord ");
                whereCondition.Append("OR v.DepartmentCode LIKE @Keyword ");
                whereCondition.Append("OR v.DepartmentName LIKE @Keyword ");
                whereCondition.Append("OR v.ProductionStepTitle LIKE @Keyword ");
                whereCondition.Append("OR v.StepName LIKE @Keyword ");
                whereCondition.Append("OR v.ProductionOrderCode LIKE @Keyword ");
                whereCondition.Append("OR v.ProductCode LIKE @Keyword ");
                whereCondition.Append("OR v.ProductName LIKE @Keyword ");
                whereCondition.Append("OR v.HandoverNote LIKE @Keyword ");
                whereCondition.Append("OR v.ProductionNote LIKE @Keyword ) ");
                parammeters.Add(new SqlParameter("@Keyword", $"%{keyword}%"));
            }


            if (fromDate > 0 && toDate > 0)
            {
                if (whereCondition.Length > 0)
                    whereCondition.Append(" AND ");
                whereCondition.Append(" (v.Date >= @FromDate AND v.Date <= @ToDate ) ");
                parammeters.Add(new SqlParameter("@FromDate", fromDate.UnixToDateTime()));
                parammeters.Add(new SqlParameter("@ToDate", toDate.UnixToDateTime()));
            }

            if (filters != null)
            {
                var suffix = 0;
                var filterCondition = new StringBuilder();
                suffix = filters.FilterClauseProcess("vProductionHandoverHistoryReceipt", "v", filterCondition, parammeters, suffix);
                if (filterCondition.Length > 2)
                {
                    if (whereCondition.Length > 0) whereCondition.Append(" AND ");
                    whereCondition.Append(filterCondition);
                }
            }

            if (string.IsNullOrEmpty(orderByFieldName))
            {
                orderByFieldName = "Date";
                asc = false;
            }

            var sql = new StringBuilder(@$"SELECT * FROM vProductionHandoverHistoryReceipt v");


            var totalSql = new StringBuilder(@$"SELECT COUNT(0) Total FROM vProductionHandoverHistoryReceipt v");

            if (whereCondition.Length > 0)
            {
                totalSql.Append(" WHERE ");
                totalSql.Append(whereCondition);

                sql.Append(" WHERE ");
                sql.Append(whereCondition);
            }


            var totalData = await _manufacturingDBContext.QueryDataTableRaw(totalSql.ToString(), parammeters.ToArray());
            var total = 0;

            if (totalData != null && totalData.Rows.Count > 0)
            {
                total = (totalData.Rows[0]["Total"] as int?).GetValueOrDefault();
            }

            if (size > 0)
            {
                sql.Append(@$" ORDER BY {orderByFieldName} {(asc ? "" : "DESC")}
                            OFFSET {(page - 1) * size} ROWS
                            FETCH NEXT {size}
                            ROWS ONLY");
            }

            var lst = await _manufacturingDBContext.QueryListRaw<ProductionHandoverHistoryReceiptModel>(sql.ToString(), parammeters.Select(p => p.CloneSqlParam()).ToArray());

            return (lst, total);
        }

        public async Task<ProductionHandoverReceiptModel> Info(long receiptId)
        {
            var infoDb = await _manufacturingDBContext.ProductionHandoverReceipt
                .Include(r => r.ProductionHandover)
                .Include(r => r.ProductionHistory)
                .FirstOrDefaultAsync(r => r.ProductionHandoverReceiptId == receiptId);
            if (infoDb == null)
            {
                throw new BadRequestException(GeneralCode.InvalidParams, "Phiếu thống kê sản xuất không tồn tại");
            }
            var info = _mapper.Map<ProductionHandoverReceiptModel>(infoDb);
            info.Handovers = infoDb.ProductionHandover.Select(h => _mapper.Map<ProductionHandoverInputModel>(h)).ToList();
            info.Histories = infoDb.ProductionHistory.Select(h => _mapper.Map<ProductionHistoryInputModel>(h)).ToList();
            return info;
        }

        public async Task<bool> AcceptBatch(IList<long> receiptIds)
        {
            var receipts = await _manufacturingDBContext.ProductionHandoverReceipt.Include(r => r.ProductionHandover).Where(ho => receiptIds.Contains(ho.ProductionHandoverReceiptId)).ToListAsync();


            var productionOrderIds = receipts.SelectMany(r => r.ProductionHandover.Select(h => h.ProductionOrderId)).Distinct().ToList();
            var productionOrders = await _manufacturingDBContext.ProductionOrder.Where(o => productionOrderIds.Contains(o.ProductionOrderId))
                .Select(o => new { o.ProductionOrderCode, o.ProductionOrderId }).ToListAsync();

            using (var batchLog = _objActivityLogFacade.BeginBatchLog())
            {
                try
                {


                    foreach (var item in receiptIds)
                    {
                        var info = receipts.FirstOrDefault(r => r.ProductionHandoverReceiptId == item);

                        if (info == null)
                        {
                            throw new BadRequestException(GeneralCode.InvalidParams, "Phiếu thống kê sản xuất không tồn tại");
                        }
                        if (info.HandoverStatusId != (int)EnumHandoverStatus.Waiting) throw new BadRequestException(GeneralCode.InvalidParams, "Chỉ được phép xác nhận phiếu thống kê sản xuất đang chờ xác nhận");
                        info.HandoverStatusId = (int)EnumHandoverStatus.Accepted;
                        info.AcceptByUserId = _currentContextService.UserId;

                        _manufacturingDBContext.SaveChanges();

                        if (info.HandoverStatusId == (int)EnumHandoverStatus.Accepted)
                        {
                            foreach (var h in info.ProductionHandover)
                            {
                                await ChangeAssignedProgressStatus(h.ProductionOrderId, h.FromProductionStepId, h.FromDepartmentId);
                                await ChangeAssignedProgressStatus(h.ProductionOrderId, h.ToProductionStepId, h.ToDepartmentId);
                            }
                        }
                        await _objActivityLogFacade.LogBuilder(() => ProductionHandoverReceiptActivityLogMessage.AcceptBatch)
                              .MessageResourceFormatDatas(info.ProductionHandoverReceiptCode)
                              .ObjectId(info.ProductionHandoverReceiptId)
                              .JsonData(info)
                              .CreateLog();

                        //Xác nhận nút chức năng Xác nhận phiếu thống kê sản xuất

                    }


                    await batchLog.CommitAsync();

                    foreach (var pro in productionOrders)
                    {
                        var codes = receipts.Where(r => r.ProductionHandover.Any(h => h.ProductionOrderId == pro.ProductionOrderId)).Select(r => r.ProductionHandoverReceiptCode).Distinct().ToArray();
                        await _productionOrderQueueHelperService.ProductionOrderStatiticChanges(pro.ProductionOrderCode, $"Xác nhận phiếu thống kê {string.Join(",", codes)}");
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

        public async Task<bool> Confirm(long receiptId, EnumHandoverStatus status)
        {
            var info = _manufacturingDBContext.ProductionHandoverReceipt.Include(r => r.ProductionHandover).FirstOrDefault(ho => ho.ProductionHandoverReceiptId == receiptId);
            if (info == null) throw new BadRequestException(GeneralCode.InvalidParams, "Phiếu thống kê sản xuất không tồn tại");
            if (info.HandoverStatusId != (int)EnumHandoverStatus.Waiting) throw new BadRequestException(GeneralCode.InvalidParams, "Chỉ được phép xác nhận phiếu thống kê sản xuất đang chờ xác nhận");

            var productionOrderIds = info.ProductionHandover.Select(h => h.ProductionOrderId).Distinct().ToList();
            var productionOrderCodes = await _manufacturingDBContext.ProductionOrder.Where(o => productionOrderIds.Contains(o.ProductionOrderId)).Select(o => o.ProductionOrderCode).ToListAsync();

            try
            {
                info.HandoverStatusId = (int)status;

                if (status == EnumHandoverStatus.Accepted)
                    info.AcceptByUserId = _currentContextService.UserId;

                _manufacturingDBContext.SaveChanges();

                if (info.HandoverStatusId == (int)EnumHandoverStatus.Accepted)
                {
                    foreach (var h in info.ProductionHandover)
                    {
                        await ChangeAssignedProgressStatus(h.ProductionOrderId, h.FromProductionStepId, h.FromDepartmentId);
                        await ChangeAssignedProgressStatus(h.ProductionOrderId, h.ToProductionStepId, h.ToDepartmentId);
                    }
                }
                await _objActivityLogFacade.LogBuilder(() => ProductionHandoverReceiptActivityLogMessage.CheckBatch)
                              .MessageResourceFormatDatas((status == EnumHandoverStatus.Accepted ? "Chấp nhận" : "Từ chối") , info.ProductionHandoverReceiptCode)
                              .ObjectId(receiptId)
                              .JsonData(info)
                              .CreateLog();


                foreach (var code in productionOrderCodes)
                {
                    await _productionOrderQueueHelperService.ProductionOrderStatiticChanges(code, $"Xác nhận phiếu thống kê {info.ProductionHandoverReceiptCode}");
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreateProductHandover");
                throw;
            }
        }

        public async Task<long> Create(long productionOrderId, ProductionHandoverReceiptModel data)
        {
            foreach (var h in data.Handovers)
            {
                h.ProductionOrderId = productionOrderId;
            }

            foreach (var h in data.Histories)
            {
                h.ProductionOrderId = productionOrderId;
            }

            return await Create(data, EnumHandoverStatus.Waiting);
        }

        public async Task<bool> CreateBatch(IList<ProductionHandoverReceiptModel> datas)
        {
            var trans = await _manufacturingDBContext.Database.BeginTransactionAsync();
            try
            {
                foreach (var data in datas)
                {
                    await Create(data, EnumHandoverStatus.Waiting);
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

        public async Task<long> Create(ProductionHandoverReceiptModel data)
        {
            var trans = await _manufacturingDBContext.Database.BeginTransactionAsync();
            try
            {
                var productionHandoverReceiptId = await Create(data, EnumHandoverStatus.Waiting);
                await trans.CommitAsync();
                return productionHandoverReceiptId;
            }
            catch (Exception ex)
            {
                await trans.RollbackAsync();
                _logger.LogError(ex, "CreateProductHandover");
                throw;
            }
            
        }

        private async Task<long> Create(ProductionHandoverReceiptModel data, EnumHandoverStatus status)
        {
            try
            {

                foreach (var h in data.Handovers)
                {
                    if (h.FromDepartmentId == STOCK_DEPARTMENT_ID && h.ToDepartmentId == STOCK_DEPARTMENT_ID)
                    {
                        if (!_manufacturingDBContext.OutsourceStepRequestData.Any(o => o.ProductionStepId == h.FromProductionStepId))
                            throw new BadRequestException(GeneralCode.InvalidParams, "Công đoạn giao không có gia công công đoạn");
                        if (!_manufacturingDBContext.OutsourceStepRequestData.Any(o => o.ProductionStepId == h.ToProductionStepId))
                            throw new BadRequestException(GeneralCode.InvalidParams, "Công đoạn nhận không có gia công công đoạn");
                    }
                    else
                    {
                        if (!_manufacturingDBContext.ProductionAssignment.Any(a => a.ProductionStepId == h.FromProductionStepId && a.DepartmentId == h.FromDepartmentId && a.ProductionOrderId == h.ProductionOrderId))
                            throw new BadRequestException(GeneralCode.InvalidParams, "Công đoạn giao không tồn tại phân công công việc cho tổ bàn giao");
                        if (!_manufacturingDBContext.ProductionAssignment.Any(a => a.ProductionStepId == h.ToProductionStepId && a.DepartmentId == h.ToDepartmentId && a.ProductionOrderId == h.ProductionOrderId))
                            throw new BadRequestException(GeneralCode.InvalidParams, "Công đoạn nhận không tồn tại phân công công việc cho tổ nhận");
                    }
                }

                foreach (var h in data.Histories)
                {
                    if (h.DepartmentId == STOCK_DEPARTMENT_ID)
                    {
                        if (!_manufacturingDBContext.OutsourceStepRequestData.Any(o => o.ProductionStepId == h.ProductionStepId))
                            throw new BadRequestException(GeneralCode.InvalidParams, "Công đoạn giao không có gia công công đoạn");

                    }
                    else
                    {
                        if (!_manufacturingDBContext.ProductionAssignment.Any(a => a.ProductionStepId == h.ProductionStepId && a.DepartmentId == h.DepartmentId && a.ProductionOrderId == h.ProductionOrderId))
                            throw new BadRequestException(GeneralCode.InvalidParams, "Công đoạn giao không tồn tại phân công công việc cho tổ bàn giao");
                    }
                }


                var ctx = _customGenCodeHelperService.CreateGenerateCodeContext();

                var code = await ctx
                    .SetConfig(EnumObjectType.ProductionHandoverReceipt)
                    .SetConfigData(0, data.Handovers.FirstOrDefault()?.HandoverDatetime)
                    .TryValidateAndGenerateCode(_manufacturingDBContext.ProductionHandoverReceipt, data.ProductionHandoverReceiptCode, (s, code) => s.ProductionHandoverReceiptCode == code);

                data.ProductionHandoverReceiptCode = code;

                var receiptInfo = new ProductionHandoverReceipt()
                {
                    ProductionHandoverReceiptCode = data.ProductionHandoverReceiptCode,
                    HandoverStatusId = (int)status,
                    AcceptByUserId = status == EnumHandoverStatus.Accepted ? (int?)_currentContextService.UserId : null
                };

                _manufacturingDBContext.ProductionHandoverReceipt.Add(receiptInfo);

                _manufacturingDBContext.SaveChanges();

                var handovers = new List<ProductionHandoverEntity>();
                foreach (var h in data.Handovers)
                {
                    var productionHandover = _mapper.Map<ProductionHandoverEntity>(h);
                    productionHandover.Status = (int)status;
                    productionHandover.ProductionHandoverReceiptId = receiptInfo.ProductionHandoverReceiptId;
                    productionHandover.AcceptByUserId = status == EnumHandoverStatus.Accepted ? (int?)_currentContextService.UserId : null;
                    handovers.Add(productionHandover);
                }


                var histories = new List<ProductionHistory>();
                foreach (var h in data.Histories)
                {
                    var his = _mapper.Map<ProductionHistory>(h);
                    his.ProductionHandoverReceiptId = receiptInfo.ProductionHandoverReceiptId;
                    histories.Add(his);
                }


                SetRowIndex(handovers, histories);

                await _manufacturingDBContext.ProductionHandover.AddRangeAsync(handovers);

                await _manufacturingDBContext.ProductionHistory.AddRangeAsync(histories);

                _manufacturingDBContext.SaveChanges();
                foreach (var h in handovers)
                {
                    if (h.Status == (int)EnumHandoverStatus.Accepted && h.FromDepartmentId != STOCK_DEPARTMENT_ID && h.ToDepartmentId != STOCK_DEPARTMENT_ID)
                    {
                        await ChangeAssignedProgressStatus(h.ProductionOrderId, h.FromProductionStepId, h.FromDepartmentId);
                        await ChangeAssignedProgressStatus(h.ProductionOrderId, h.ToProductionStepId, h.ToDepartmentId);
                    }
                }
                await _objActivityLogFacade.LogBuilder(() => ProductionHandoverReceiptActivityLogMessage.CreateBatch)
                              .MessageResourceFormatDatas(receiptInfo.ProductionHandoverReceiptCode)
                              .ObjectId(receiptInfo.ProductionHandoverReceiptId)
                              .JsonData(data)
                              .CreateLog();

                await ctx.ConfirmCode();

                var productionOrderIds = data.Handovers.Select(h => h.ProductionOrderId).Union(data.Histories.Select(t => t.ProductionOrderId)).Distinct().ToList();
                var podCodes = await _manufacturingDBContext.ProductionOrder.Where(p => productionOrderIds.Contains(p.ProductionOrderId)).Select(p => p.ProductionOrderCode).ToListAsync();
                foreach (var podCode in podCodes)
                {
                    await _productionOrderQueueHelperService.ProductionOrderStatiticChanges(podCode, $"Tạo phiếu thống kê {receiptInfo.ProductionHandoverReceiptCode}");
                }

                
                return receiptInfo.ProductionHandoverReceiptId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Create");
                throw;
            }
        }


        private void SetRowIndex(ICollection<ProductionHandoverEntity> handovers, ICollection<ProductionHistory> histories)
        {
            var handoverGroups = handovers.GroupBy(h => new
            {
                h.ProductionHandoverReceiptId,
                h.HandoverDatetime,
                h.FromDepartmentId,
                h.FromProductionStepId,
                h.ProductionOrderId,
                h.ObjectTypeId,
                h.ObjectId
            }).ToList();

            foreach (var g in handoverGroups)
            {
                var rowIndex = 0;
                foreach (var row in g)
                {
                    row.RowIndex = rowIndex++;
                }
            }


            var historyGroups = histories.GroupBy(h => new
            {
                h.ProductionHandoverReceiptId,
                h.Date,
                h.DepartmentId,
                h.ProductionStepId,
                h.ProductionOrderId,
                h.ObjectTypeId,
                h.ObjectId
            }).ToList();

            foreach (var g in historyGroups)
            {
                var rowIndex = 0;
                foreach (var row in g)
                {
                    row.RowIndex = rowIndex++;
                }
            }

        }


        public async Task<bool> Delete(long productionHandoverReceiptId)
        {
            try
            {
                var receiptInfo = _manufacturingDBContext.ProductionHandoverReceipt
                    .Include(r => r.ProductionHandover)
                    .Include(r => r.ProductionHistory)
                    .Where(h => h.ProductionHandoverReceiptId == productionHandoverReceiptId)
                    .FirstOrDefault();

                if (receiptInfo == null)
                    throw new BadRequestException(GeneralCode.InvalidParams, "Không tồn tại phiếu thống kê sản xuất");
                receiptInfo.IsDeleted = true;
                foreach (var h in receiptInfo.ProductionHandover)
                {
                    h.IsDeleted = true;
                }

                foreach (var h in receiptInfo.ProductionHistory)
                {
                    h.IsDeleted = true;
                }

                _manufacturingDBContext.SaveChanges();
                if (receiptInfo.HandoverStatusId == (int)EnumHandoverStatus.Accepted)
                {
                    foreach (var h in receiptInfo.ProductionHandover)
                    {
                        await ChangeAssignedProgressStatus(h.ProductionOrderId, h.ToProductionStepId, h.ToDepartmentId);
                        await ChangeAssignedProgressStatus(h.ProductionOrderId, h.FromProductionStepId, h.FromDepartmentId);
                    }
                }
                await _objActivityLogFacade.LogBuilder(() => ProductionHandoverReceiptActivityLogMessage.AcceptBatch)
                              .MessageResourceFormatDatas(receiptInfo.ProductionHandoverReceiptCode)
                              .ObjectId(productionHandoverReceiptId)
                              .JsonData(receiptInfo)
                              .CreateLog();

                var podCodes = await _manufacturingDBContext.ProductionOrder.Where(p => receiptInfo.ProductionHandover.Select(h => h.ProductionOrderId).Contains(p.ProductionOrderId)).Select(p => p.ProductionOrderCode).ToListAsync();
                foreach (var podCode in podCodes)
                {
                    await _productionOrderQueueHelperService.ProductionOrderStatiticChanges(podCode, $"Xóa phiếu thống kê {receiptInfo.ProductionHandoverReceiptCode}");
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreateProductHandover");
                throw;
            }
        }

        public async Task<bool> Update(long productionHandoverReceiptId, ProductionHandoverReceiptModel data, EnumHandoverStatus status)
        {
            var receiptInfo = _manufacturingDBContext.ProductionHandoverReceipt
                .Include(r => r.ProductionHandover)
                .Include(r => r.ProductionHistory)
                .Where(h => h.ProductionHandoverReceiptId == productionHandoverReceiptId)
                .FirstOrDefault();

            if (receiptInfo == null)
                throw new BadRequestException(GeneralCode.InvalidParams, "Không tồn tại phiếu thống kê sản xuất");

            var productionOrderIds = receiptInfo.ProductionHandover.Select(h => h.ProductionOrderId).ToList();
            productionOrderIds.AddRange(data.Handovers.Select(d => d.ProductionOrderId).ToList());
            productionOrderIds.AddRange(data.Histories.Select(d => d.ProductionOrderId).ToList());

            using (var trans = await _manufacturingDBContext.Database.BeginTransactionAsync())
            {

                _mapper.Map(data, receiptInfo);
                receiptInfo.HandoverStatusId = (int)status;
                receiptInfo.AcceptByUserId = status == EnumHandoverStatus.Accepted ? (int?)_currentContextService.UserId : null;

                //handover
                var changedHandovers = new List<ProductionHandoverEntity>();

                foreach (var h in receiptInfo.ProductionHandover)
                {
                    var handoverData = data.Handovers.FirstOrDefault(d => d.ProductionHandoverId == h.ProductionHandoverId);
                    if (handoverData == null)
                    {
                        h.IsDeleted = true;
                    }
                    else
                    {
                        _mapper.Map(handoverData, h);
                        h.Status = (int)status;
                        h.ProductionHandoverReceiptId = productionHandoverReceiptId;
                        h.AcceptByUserId = status == EnumHandoverStatus.Accepted ? (int?)_currentContextService.UserId : null;
                    }
                    changedHandovers.Add(h);
                }


                foreach (var h in data.Handovers)
                {
                    if (h.ProductionHandoverId == null || h.ProductionHandoverId == 0)
                    {
                        var handoverInfo = _mapper.Map<ProductionHandoverEntity>(h);
                        handoverInfo.ProductionHandoverReceiptId = productionHandoverReceiptId;
                        _manufacturingDBContext.ProductionHandover.Add(handoverInfo);
                        handoverInfo.Status = (int)status;
                        handoverInfo.AcceptByUserId = status == EnumHandoverStatus.Accepted ? (int?)_currentContextService.UserId : null;
                        changedHandovers.Add(handoverInfo);
                    }
                }


                //history

                foreach (var h in receiptInfo.ProductionHistory)
                {
                    var historyData = data.Histories.FirstOrDefault(d => d.ProductionHistoryId == h.ProductionHistoryId);
                    if (historyData == null)
                    {
                        h.IsDeleted = true;
                    }
                    else
                    {
                        _mapper.Map(historyData, h);
                        h.ProductionHandoverReceiptId = productionHandoverReceiptId;
                    }
                }


                foreach (var h in data.Histories)
                {
                    if (h.ProductionHistoryId == null || h.ProductionHistoryId == 0)
                    {
                        var hisInfo = _mapper.Map<ProductionHistory>(h);
                        hisInfo.ProductionHandoverReceiptId = productionHandoverReceiptId;
                        _manufacturingDBContext.ProductionHistory.Add(hisInfo);
                    }
                }

                await _manufacturingDBContext.SaveChangesAsync();


                receiptInfo = _manufacturingDBContext.ProductionHandoverReceipt
               .Include(r => r.ProductionHandover)
               .Include(r => r.ProductionHistory)
               .Where(h => h.ProductionHandoverReceiptId == productionHandoverReceiptId)
               .FirstOrDefault();

                SetRowIndex(receiptInfo.ProductionHandover, receiptInfo.ProductionHistory);

                await _manufacturingDBContext.SaveChangesAsync();

                foreach (var h in changedHandovers)
                {
                    await ChangeAssignedProgressStatus(h.ProductionOrderId, h.ToProductionStepId, h.ToDepartmentId);
                    await ChangeAssignedProgressStatus(h.ProductionOrderId, h.FromProductionStepId, h.FromDepartmentId);
                }

                await trans.CommitAsync();

            }
            await _objActivityLogFacade.LogBuilder(() => ProductionHandoverReceiptActivityLogMessage.UpdateBatch)
                              .MessageResourceFormatDatas(receiptInfo.ProductionHandoverReceiptCode)
                              .ObjectId(productionHandoverReceiptId)
                              .JsonData(receiptInfo)
                              .CreateLog();

            var podCodes = await _manufacturingDBContext.ProductionOrder.Where(p => productionOrderIds.Contains(p.ProductionOrderId)).Select(p => p.ProductionOrderCode).ToListAsync();
            foreach (var podCode in podCodes)
            {
                await _productionOrderQueueHelperService.ProductionOrderStatiticChanges(podCode, $"Cập nhật phiếu thống kê {receiptInfo.ProductionHandoverReceiptCode}");
            }
            return true;

        }

        public async Task<long> CreateStatictic(long productionOrderId, ProductionHandoverReceiptModel data)
        {
            foreach (var h in data.Handovers)
            {
                h.ProductionOrderId = productionOrderId;
            }

            foreach (var h in data.Histories)
            {
                h.ProductionOrderId = productionOrderId;
            }

            return await Create(data, EnumHandoverStatus.Accepted);
        }
        /*
        public async Task<IList<ProductionHandoverModel>> CreateMultipleStatictic(long productionOrderId, IList<ProductionHandoverInputModel> data)
        {
            var poInfo = await _manufacturingDBContext.ProductionOrder.FirstOrDefaultAsync(p => p.ProductionOrderId == productionOrderId);

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

                    await _activityLogService.CreateLog(EnumObjectType.ProductionHandover, item.ProductionHandoverId, $"Tạo bàn giao công việc / yêu cầu xuất kho", data);
                }

                trans.Commit();

                await _queueProcessHelperService.EnqueueAsync(PRODUCTION_INVENTORY_STATITICS, poInfo?.ProductionOrderCode);

                return result;
            }
            catch (Exception ex)
            {
                trans.Rollback();
                _logger.LogError(ex, "CreateProductHandover");
                throw;
            }
        }*/

        public async Task<IList<ProductionHandoverModel>> GetProductionHandovers(long productionOrderId)
        {
            var handovers = _manufacturingDBContext.ProductionHandover.Where(h => h.ProductionOrderId == productionOrderId);

            return await GetProductionHandovers(handovers);
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

        public async Task<PageData<ProductionHandoverReceiptByDateModel>> GetDepartmentHandoversByDate(IList<long> fromDepartmentIds, IList<long> toDepartmentIds, IList<long> fromStepIds, IList<long> toStepIds, long? fromDate, long? toDate, bool? isInFinish, bool? isOutFinish, int page, int size)
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
            var lst = dataTable.ConvertData<ProductionHandoverReceiptByDateModel>();

            return (lst, totalRecord);
        }


        private async Task<IList<ProductionHandoverModel>> GetProductionHandovers(IQueryable<ProductionHandoverEntity> handovers)
        {
            return await (from r in _manufacturingDBContext.ProductionHandoverReceipt
                          join h in handovers on r.ProductionHandoverReceiptId equals h.ProductionHandoverReceiptId
                          select new ProductionHandoverModel
                          {
                              ProductionHandoverReceiptId = r.ProductionHandoverReceiptId,
                              ProductionHandoverReceiptCode = r.ProductionHandoverReceiptCode,
                              ProductionHandoverId = h.ProductionHandoverId,
                              HandoverStatusId = (EnumHandoverStatus)r.HandoverStatusId,
                              CreatedByUserId = r.CreatedByUserId,
                              AcceptByUserId = r.AcceptByUserId,

                              HandoverQuantity = h.HandoverQuantity,
                              ObjectId = h.ObjectId,
                              ObjectTypeId = (EnumProductionStepLinkDataObjectType)h.ObjectTypeId,
                              FromDepartmentId = h.FromDepartmentId,
                              FromProductionStepId = h.FromProductionStepId,
                              ToDepartmentId = h.ToDepartmentId,
                              ToProductionStepId = h.ToProductionStepId,
                              HandoverDatetime = h.HandoverDatetime.GetUnix(),
                              Note = h.Note,
                              ProductionOrderId = h.ProductionOrderId
                          }).ToListAsync();
        }


    }
}
