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
using static VErp.Commons.Enums.Manafacturing.EnumProductionProcess;

namespace VErp.Services.Manafacturing.Service.Outsource.Implement
{
    public class RequestOutsourcePartService : IRequestOutsourcePartService
    {
        private readonly ManufacturingDBContext _manufacturingDBContext;
        private readonly IActivityLogService _activityLogService;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly ICustomGenCodeHelperService _customGenCodeHelperService;

        public RequestOutsourcePartService(ManufacturingDBContext manufacturingDB
            , IActivityLogService activityLogService
            , ILogger<RequestOutsourcePartService> logger
            , IMapper mapper
            , ICustomGenCodeHelperService customGenCodeHelperService)
        {
            _manufacturingDBContext = manufacturingDB;
            _activityLogService = activityLogService;
            _logger = logger;
            _mapper = mapper;
            _customGenCodeHelperService = customGenCodeHelperService;
        }

        public async Task<bool> CreateRequestOutsourcePart(List<RequestOutsourcePartModel> datas)
        {
            using var trans = await _manufacturingDBContext.Database.BeginTransactionAsync();
            try
            {
                int customGenCodeId = 0;
                var currentConfig = await _customGenCodeHelperService.CurrentConfig(EnumObjectType.RequestOutsource, EnumObjectType.RequestOutsource, 0);
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

                var enties = new List<RequestOutsourcePart>();
                foreach (var data in datas)
                {
                    data.RequestOutsourcePartCode = generated.CustomCode;
                    data.Status = EnumProductionProcess.OutsourcePartProcessType.Unprocessed;
                    enties.Add(_mapper.Map<RequestOutsourcePart>(data));
                }

                await _manufacturingDBContext.RequestOutsourcePart.AddRangeAsync(enties);
                await _manufacturingDBContext.SaveChangesAsync();
                trans.Commit();

                if (customGenCodeId > 0)
                {
                    await _customGenCodeHelperService.ConfirmCode(customGenCodeId);
                }

                await _activityLogService.CreateLog(EnumObjectType.ProductionOrder, enties.First().ProductionOrderDetailId, $"Thêm mới yêu cầu gia công chi tiết {enties.First().RequestOutsourcePartCode}", enties.JsonSerialize());
                return true;
            }
            catch (Exception ex)
            {
                trans.TryRollbackTransaction();
                _logger.LogError(ex, "CreateRequestOutsourcePart");
                throw;
            }
        }

        public async Task<IList<RequestOutsourcePartInfo>> GetRequestOutsourcePartExtraInfo(int productionOrderDetailId = 0)
        {
            var sql = $@"SELECT        rp.RequestOutsourcePartId, rp.RequestOutsourcePartCode, rp.ProductionOrderDetailId, DATEDIFF(s, '1970-01-01 00:00:00', rp.DateRequiredComplete) AS DateRequiredComplete, DATEDIFF(s, 
                         '1970-01-01 00:00:00', rp.CreatedDatetimeUtc) AS CreateDateRequest, pod.ProductionOrderCode, pod.ProductCode, pod.ProductName, rp.Status, rp.Quanity, u.UnitName, p2.ProductName AS ProductPartName, 
                         rp.ProductId, rp.UnitId, pod.OrderCode, p2.ProductCode AS ProductPartCode
FROM            dbo.RequestOutsourcePart AS rp INNER JOIN
                         dbo.vProductionOrderDetail AS pod ON rp.ProductionOrderDetailId = pod.ProductionOrderDetailId INNER JOIN
                         MasterDB.dbo.Unit AS u ON u.UnitId = rp.UnitId INNER JOIN
                         StockDB.dbo.Product AS p2 ON rp.ProductId = p2.ProductId
WHERE        (rp.IsDeleted = 0) ";

            if (productionOrderDetailId != 0)
            {
                sql += " and rp.ProductionOrderDetailId = @ProductionOrderDetailId";
            }

            var parammeters = new SqlParameter[]
                {
                    new SqlParameter("@ProductionOrderDetailId", productionOrderDetailId)
                };

            var data = (await _manufacturingDBContext.QueryDataTable(sql, parammeters)).ConvertData<RequestOutsourcePartInfo>();

            return data;
        }

        public async Task<bool> UpdateRequestOutsourcePart(int productionOrderDetailId, List<RequestOutsourcePartModel> req)
        {
            var datas = _manufacturingDBContext.RequestOutsourcePart.Where(x => x.ProductionOrderDetailId == productionOrderDetailId).ToList();
            if (datas.Count <= 0)
                throw new BadRequestException(OutsourceErrorCode.NotFoundRequest, $"Không tìm thấy yêu cầu gia công của {productionOrderDetailId}");

            using var trans = await _manufacturingDBContext.Database.BeginTransactionAsync();
            try
            {
                var nData = req.Where(x => datas.Any(y => y.RequestOutsourcePartId != x.RequestOutsourcePartId)).ToList();
                var dData = datas.Where(x => req.Any(y => y.RequestOutsourcePartId != x.RequestOutsourcePartId)).ToList();
                var uData = datas.Where(x => req.Any(y => y.RequestOutsourcePartId == x.RequestOutsourcePartId)).ToList();

                foreach(var d in dData)
                {
                    if (d.Status != (int)OutsourcePartProcessType.Unprocessed)
                        throw new BadRequestException(OutsourceErrorCode.InValidRequestOutsource, "Yêu cầu gia công chi tiết đã có đơn hàng giao công");
                    d.IsDeleted = true;
                }
                foreach (var d in uData)
                {
                    if (d.Status != (int)OutsourcePartProcessType.Unprocessed)
                        throw new BadRequestException(OutsourceErrorCode.InValidRequestOutsource, "Yêu cầu gia công chi tiết đã có đơn hàng giao công");

                    var s = req.FirstOrDefault(x => x.RequestOutsourcePartId == d.RequestOutsourcePartId);
                    _mapper.Map(s, d);
                }
                await _manufacturingDBContext.SaveChangesAsync();
                await CreateRequestOutsourcePart(nData);
                trans.Commit();

                await _activityLogService.CreateLog(EnumObjectType.ProductionOrder, req.First().ProductionOrderDetailId, $"Cập nhật yêu cầu gia công chi tiết {req.First().RequestOutsourcePartCode}", req.JsonSerialize());
                return true;
            }
            catch (Exception ex)
            {
                trans.TryRollbackTransaction();
                _logger.LogError(ex, "CreateRequestOutsourcePart");
                throw;
            }

        }

        public async Task<PageData<RequestOutsourcePartInfo>> GetListRequestOutsourcePart(string keyword, int page, int size)
        {
            var data = await GetRequestOutsourcePartExtraInfo();
            var total = data.Count();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                data = data.Where(x => x.RequestOutsourcePartCode.Contains(keyword)).ToList();
            }

            data = data.Skip((page - 1) * size).Take(size).ToList();
            return (data, total);
        }
    }
}
