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
using VErp.Commons.Library;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.ManufacturingDB;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Manafacturing.Model.Outsource.Order;
using static VErp.Commons.Enums.Manafacturing.EnumProductionProcess;

namespace VErp.Services.Manafacturing.Service.Outsource.Implement
{
    public class OutsourceOrderService : IOutsourceOrderService
    {
        private readonly ManufacturingDBContext _manufacturingDBContext;
        private readonly IActivityLogService _activityLogService;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly ICustomGenCodeHelperService _customGenCodeHelperService;

        public OutsourceOrderService(ManufacturingDBContext manufacturingDB
            , IActivityLogService activityLogService
            , ILogger<OutsourceOrderService> logger
            , IMapper mapper
            , ICustomGenCodeHelperService customGenCodeHelperService)
        {
            _manufacturingDBContext = manufacturingDB;
            _activityLogService = activityLogService;
            _logger = logger;
            _mapper = mapper;
            _customGenCodeHelperService = customGenCodeHelperService;
        }

        public async Task<long> CreateOutsourceOrderPart(OutsourceOrderInfo req)
        {
            using (var trans = _manufacturingDBContext.Database.BeginTransaction())
            {
                try
                {
                    int customGenCodeId = 0;
                    string outsoureOrderCode = "";
                    if (string.IsNullOrWhiteSpace(req.OutsourceOrderCode))
                    {
                        var currentConfig = await _customGenCodeHelperService.CurrentConfig(EnumObjectType.OutsourceOrder, EnumObjectType.OutsourceOrder, 0);
                        if (currentConfig == null)
                        {
                            throw new BadRequestException(GeneralCode.ItemNotFound, "Chưa thiết định cấu hình sinh mã");
                        }
                        var generated = await _customGenCodeHelperService.GenerateCode(currentConfig.CustomGenCodeId, currentConfig.LastValue);
                        if (generated == null)
                        {
                            throw new BadRequestException(GeneralCode.InternalError, "Không thể sinh mã ");
                        }
                        customGenCodeId = currentConfig.CustomGenCodeId;
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
                    order.OutsourceTypeId = (int)OutsourceOrderType.OutsourcePart;
                    order.OutsourceOrderCode = string.IsNullOrWhiteSpace(order.OutsourceOrderCode) ? outsoureOrderCode : order.OutsourceOrderCode;

                    _manufacturingDBContext.OutsourceOrder.Add(order);
                    await _manufacturingDBContext.SaveChangesAsync();

                    var detail = _mapper.Map<List<OutsourceOrderDetail>>(req.OutsourceOrderDetail);
                    detail.ForEach(x => x.OutsourceOrderId = order.OutsourceOrderId);

                    _manufacturingDBContext.OutsourceOrderDetail.AddRange(detail);
                    await _manufacturingDBContext.SaveChangesAsync();
                    await UpdateStatusRequestOutsourcePartDetail(detail.Select(x => x.ObjectId).ToList(), OutsourcePartProcessType.Processing);

                    if (string.IsNullOrWhiteSpace(req.OutsourceOrderCode))
                    {
                        await _customGenCodeHelperService.ConfirmCode(customGenCodeId);
                    }

                    await trans.CommitAsync();

                    return order.OutsourceOrderId;
                }
                catch (Exception ex)
                {
                    await trans.RollbackAsync();
                    _logger.LogError("CreateOutsourceOrderPart");
                    throw ex;
                }
            }
        }

        public async Task<PageData<OutsourceOrderPartDetailOutput>> GetListOutsourceOrderPart(string keyword, int page, int size)
        {
            keyword = (keyword ?? "").Trim();
            var parammeters = new List<SqlParameter>();
            var whereCondition = new StringBuilder();
            if (!string.IsNullOrEmpty(keyword))
            {
                whereCondition.Append("(v.OutsoureOrderCode LIKE @KeyWord ");
                whereCondition.Append("OR v.ProductPartName LIKE @Keyword ");
                whereCondition.Append("OR v.ProductTitle LIKE @Keyword ");
                whereCondition.Append("OR v.RequestOutsourcePartCode LIKE @Keyword ");
                whereCondition.Append("OR v.OrderCode LIKE @Keyword ) ");
                whereCondition.Append("OR v.ProductionOrderCode LIKE @Keyword ) ");
                parammeters.Add(new SqlParameter("@Keyword", $"%{keyword}%"));
            }

            var sql = new StringBuilder("SELECT * FROM vOutsourceOrderExtractInfo v ");
            var totalSql = new StringBuilder("SELECT COUNT(v.OutsourceOrderDetailId) Total FROM vOutsourceOrderExtractInfo v ");
            if (whereCondition.Length > 0)
            {
                totalSql.Append("WHERE ");
                totalSql.Append(whereCondition);
                sql.Append("WHERE ");
                sql.Append(whereCondition);
            }

            sql.Append($" ORDER BY v.OutsourceOrderDetailId");

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
            var lst = resultData.ConvertData<OutsourceOrderPartDetailOutput>().ToList();

            return (lst, total);
        }

        private async Task UpdateStatusRequestOutsourcePartDetail(List<long> listID, OutsourcePartProcessType status)
        {
            var data = await _manufacturingDBContext.RequestOutsourcePartDetail.Where(x => listID.Contains(x.RequestOutsourcePartDetailId)).ToListAsync();
            foreach (var e in data)
                e.StatusId = (int)status;
            await _manufacturingDBContext.SaveChangesAsync();

        }
    }
}
