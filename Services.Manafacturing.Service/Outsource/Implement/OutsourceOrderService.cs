using AutoMapper;
using AutoMapper.QueryableExtensions;
using DocumentFormat.OpenXml.Office.CustomUI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenXmlPowerTools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Threading.Tasks;
using VErp.Commons.Enums.ErrorCodes;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
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
        private readonly ManufacturingDBContext _manuDBContext;
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
            _manuDBContext = manufacturingDB;
            _activityLogService = activityLogService;
            _logger = logger;
            _mapper = mapper;
            _customGenCodeHelperService = customGenCodeHelperService;
        }

        public async Task<long> CreateOutsourceOrderPart(OutsourceOrderInfo req)
        {
            using (var trans = _manuDBContext.Database.BeginTransaction())
            {
                try
                {
                    int customGenCodeId = 0;
                    string outsoureOrderCode = "";
                    if (string.IsNullOrWhiteSpace(req.OutsoureOrderCode))
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
                        if (_manuDBContext.OutsourceOrder.Any(o => o.OutsoureOrderCode == req.OutsoureOrderCode))
                            throw new BadRequestException(OutsourceErrorCode.OutsoureOrderCodeAlreadyExisted);
                    }
                    if (!req.CreateDateOrder.HasValue)
                    {
                        req.CreateDateOrder = DateTime.UtcNow.GetUnix();
                    }

                    var order = _mapper.Map<OutsourceOrder>(req as OutsourceOrderModel);
                    order.OutsourceTypeId = (int)OutsourceOrderType.OutsourcePart;
                    order.OutsoureOrderCode = string.IsNullOrWhiteSpace(order.OutsoureOrderCode) ? outsoureOrderCode : order.OutsoureOrderCode;

                    _manuDBContext.OutsourceOrder.Add(order);
                    await _manuDBContext.SaveChangesAsync();

                    var detail = _mapper.Map<List<OutsourceOrderDetail>>(req.OutsourceOrderDetail);
                    detail.ForEach(x => x.OutsoureOrderId = order.OutsoureOrderId);

                    _manuDBContext.OutsourceOrderDetail.AddRange(detail);
                    await _manuDBContext.SaveChangesAsync();
                    await UpdateStatusRequestOutsourcePartDetail(detail.Select(x => x.ObjectId).ToList(), OutsourcePartProcessType.Processing);

                    if (string.IsNullOrWhiteSpace(req.OutsoureOrderCode))
                    {
                        await _customGenCodeHelperService.ConfirmCode(customGenCodeId);
                    }

                    await trans.CommitAsync();

                    return order.OutsoureOrderId;
                }
                catch (Exception ex)
                {
                    await trans.RollbackAsync();
                    _logger.LogError("CreateOutsourceOrderPart");
                    throw ex;
                }
            }
        }

        private async Task UpdateStatusRequestOutsourcePartDetail(List<long> listID, OutsourcePartProcessType status)
        {
            var data = await _manuDBContext.RequestOutsourcePartDetail.Where(x => listID.Contains(x.RequestOutsourcePartDetailId)).ToListAsync();
            foreach (var e in data)
                e.StatusId = (int)status;
            await _manuDBContext.SaveChangesAsync();

        }
    }
}
