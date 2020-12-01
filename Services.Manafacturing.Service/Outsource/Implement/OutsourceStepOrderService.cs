using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
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
using VErp.Services.Manafacturing.Service.ProductionProcess;
using static VErp.Commons.Enums.Manafacturing.EnumProductionProcess;

namespace VErp.Services.Manafacturing.Service.Outsource.Implement
{
    public class OutsourceStepOrderService : IOutsourceStepOrderService
    {
        private readonly ManufacturingDBContext _manufacturingDBContext;
        private readonly IActivityLogService _activityLogService;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly ICustomGenCodeHelperService _customGenCodeHelperService;
        private readonly IOutsourceStepRequestService _outsourceStepRequestService;

        public OutsourceStepOrderService(ManufacturingDBContext manufacturingDB
            , IActivityLogService activityLogService
            , ILogger<OutsourceStepOrderService> logger
            , IMapper mapper
            , ICustomGenCodeHelperService customGenCodeHelperService
            , IOutsourceStepRequestService outsourceStepRequestService)
        {
            _manufacturingDBContext = manufacturingDB;
            _activityLogService = activityLogService;
            _logger = logger;
            _mapper = mapper;
            _customGenCodeHelperService = customGenCodeHelperService;
            _outsourceStepRequestService = outsourceStepRequestService;
        }

        public async Task<long> CreateOutsourceStepOrderPart(OutsourceStepOrderModel req)
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
                    order.OutsourceTypeId = (int)EnumOutsourceOrderType.OutsourceStep;
                    order.OutsourceOrderCode = string.IsNullOrWhiteSpace(order.OutsourceOrderCode) ? outsoureOrderCode : order.OutsourceOrderCode;

                    _manufacturingDBContext.OutsourceOrder.Add(order);
                    await _manufacturingDBContext.SaveChangesAsync();

                    var detail = _mapper.Map<List<OutsourceOrderDetail>>(req.outsourceOrderDetail.Where(x => x.ProductionStepLinkDataRoleTypeId == EnumProductionStepLinkDataRoleType.Output));
                    detail.ForEach(x => x.OutsourceOrderId = order.OutsourceOrderId);

                    _manufacturingDBContext.OutsourceOrderDetail.AddRange(detail);
                    await _manufacturingDBContext.SaveChangesAsync();

                    if (string.IsNullOrWhiteSpace(req.OutsourceOrderCode))
                    {
                        await _customGenCodeHelperService.ConfirmCode(customGenCodeId);
                    }
                    await _manufacturingDBContext.SaveChangesAsync();
                    await trans.CommitAsync();
                    await _activityLogService.CreateLog(EnumObjectType.ProductionOrder, order.OutsourceOrderId, $"Thêm mới đơn hàng gia công công đoạn {order.OutsourceOrderId}", req.JsonSerialize());
                    return order.OutsourceOrderId;
                }
                catch (Exception ex)
                {
                    await trans.RollbackAsync();
                    _logger.LogError("CreateOutsourceStepOrderPart");
                    throw ex;
                }
            }
        }

        public async Task<PageData<OutsourceStepOrderSeach>> SearchOutsourceStepOrder(string keyword, int page, int size)
        {
            var outsourceStepOrders = (from o in _manufacturingDBContext.OutsourceOrder
                                       join d in _manufacturingDBContext.OutsourceOrderDetail on o.OutsourceOrderId equals d.OutsourceOrderId
                                       join rd in _manufacturingDBContext.OutsourceStepRequestData on d.ObjectId equals rd.ProductionStepLinkDataId
                                       join r in _manufacturingDBContext.OutsourceStepRequest on rd.OutsourceStepRequestId equals r.OutsourceStepRequestId
                                       where o.OutsourceTypeId == (int)EnumOutsourceOrderType.OutsourceStep
                                       group new { o, r, rd, d } by new { o.OutsourceOrderId, o.OutsourceOrderCode, r.OutsourceStepRequestId, r.OutsourceStepRequestCode, o.OutsourceOrderFinishDate } into g
                                       select new OutsourceStepOrderSeach
                                       {
                                           OutsourceOrderFinishDate = g.Key.OutsourceOrderFinishDate.GetUnix(),
                                           OutsourceOrderId = g.Key.OutsourceOrderId,
                                           OutsourceOrderCode = g.Key.OutsourceOrderCode,
                                           OutsourceStepRequestCode = g.Key.OutsourceStepRequestCode,
                                           OutsourceStepRequestId = g.Key.OutsourceStepRequestId,
                                       }).ToList();
            var outsourceStepRequests = (await _outsourceStepRequestService.GetListOutsourceStepRequest(string.Empty, 1, -1, string.Empty, true)).List;

            var data = from order in outsourceStepOrders
                       join request in outsourceStepRequests
                            on order.OutsourceStepRequestId equals request.OutsourceStepRequestId
                       select new OutsourceStepOrderSeach
                       {
                           OutsourceOrderId = order.OutsourceOrderId,
                           OutsourceStepRequestId = order.OutsourceStepRequestId,
                           OrderCode = request.OrderCode,
                           OutsourceOrderCode = order.OutsourceOrderCode,
                           OutsourceOrderFinishDate = order.OutsourceOrderFinishDate,
                           OutsourceStepRequestCode = order.OutsourceStepRequestCode,
                           ProductionOrderCode = request.ProductionOrderCode,
                           ProductionStepTitle = request.ProductionStepTitle
                       };

            if (!string.IsNullOrWhiteSpace(keyword))
                data = data.Where(x => x.ProductionOrderCode.Contains(keyword)
                                   || x.OutsourceOrderCode.Contains(keyword)
                                   || x.OutsourceStepRequestCode.Contains(keyword)
                                   || x.OrderCode.Contains(keyword));

            var total = data.Count();

            return (data.Skip((page - 1) * size).Take(size).ToList(), total);
        }

        public async Task<OutsourceStepOrderModel> GetOutsourceStepOrder(long outsourceStepOrderId)
        {
            var outsourceStepOrder = await _manufacturingDBContext.OutsourceOrder.AsNoTracking()
                .Include(x=>x.OutsourceOrderDetail)
                .ProjectTo<OutsourceStepOrderModel>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(x => x.OutsourceOrderId == outsourceStepOrderId);
            if (outsourceStepOrder == null)
                throw new BadRequestException(OutsourceErrorCode.NotFoundOutsourceOrder);
            var outsourceStepRequestIds = (from d in outsourceStepOrder.outsourceOrderDetail
                                           join rd in _manufacturingDBContext.OutsourceStepRequestData
                                           on d.ProductionStepLinkDataId equals rd.ProductionStepLinkDataId
                                           select rd.OutsourceStepRequestId).Distinct();

            foreach(var outsourceStepRequestId in outsourceStepRequestIds)
            {
                var a = await _outsourceStepRequestService.GetOutsourceStepRequestData(outsourceStepRequestId);
            }
            throw new NotImplementedException();
        }
    }
}
