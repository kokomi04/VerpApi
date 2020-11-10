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
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ManufacturingDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Manafacturing.Model.Outsource.Order;

namespace VErp.Services.Manafacturing.Service.Outsource.Implement
{
    public class OutsourceOrderService : IOutsourceOrderService
    {
        private readonly ManufacturingDBContext _manuDBContext;
        private readonly IActivityLogService _activityLogService;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        public OutsourceOrderService(ManufacturingDBContext manufacturingDB
            , IActivityLogService activityLogService
            , ILogger<OutsourceOrderService> logger
            , IMapper mapper)
        {
            _manuDBContext = manufacturingDB;
            _activityLogService = activityLogService;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<int> CreateOutsourceOrder(OutsoureOrderInfo req)
        {
            using (var trans = _manuDBContext.Database.BeginTransaction())
            {
                try
                {
                    var order = _mapper.Map<OutsourceOrder>(req as OutsourceOrderModel);
                    _manuDBContext.OutsourceOrder.Add(order);
                    await _manuDBContext.SaveChangesAsync();

                    var detail = _mapper.Map<List<OutsourceOrderDetail>>(req.OutsourceOrderDetail);
                    detail.ForEach(x => x.OutsoureOrderId = order.OutsoureOrderId);
                    _manuDBContext.OutsourceOrderDetail.AddRange(detail);
                    await _manuDBContext.SaveChangesAsync();

                    await trans.CommitAsync();

                    return order.OutsoureOrderId;
                }
                catch (Exception ex)
                {
                    await trans.RollbackAsync();
                    _logger.LogError("CreateOutsourceOrder");
                    throw ex;
                }
            }
        }

        public async Task<bool> DeleteOutsourceOrder(int outsourceOrderId)
        {
            var order = await _manuDBContext.OutsourceOrder.FirstOrDefaultAsync(x => x.OutsoureOrderId == outsourceOrderId);
            if (order == null)
                throw new BadRequestException(OutsourceErrorCode.NotFoundOutsourOrder);
            var details = await _manuDBContext.OutsourceOrderDetail.Where(x => x.OutsoureOrderId == outsourceOrderId).ToListAsync();

            details.ForEach(x => x.IsDeleted = true);
            order.IsDeleted = true;

            await _manuDBContext.SaveChangesAsync();
            return true;
        }

        public async Task<PageData<OutsoureOrderInfo>> GetListOutsourceOrder(int requestContainerTypeId, string keyWord, int page, int size)
        {
            var query = _manuDBContext.OutsourceOrder.Where(x => x.RequestObjectTypeId == requestContainerTypeId);
            if (!string.IsNullOrWhiteSpace(keyWord))
            {
                query = query.Where(x => x.OutsoureOrderCode.Contains(keyWord));
            }

            var total = await query.CountAsync();
            var data = query.Skip((page - 1) * size).Take(size)
                .ProjectTo<OutsoureOrderInfo>(_mapper.ConfigurationProvider)
                .ToList();

            return (data, total);
        }

        public async Task<bool> UpdateOutsourceOrder(int outsourceOrderId, OutsoureOrderInfo req)
        {
            var order = await _manuDBContext.OutsourceOrder.FirstOrDefaultAsync(x => x.OutsoureOrderId == outsourceOrderId);
            if (order == null)
                throw new BadRequestException(OutsourceErrorCode.NotFoundOutsourOrder);
            var details = await _manuDBContext.OutsourceOrderDetail.Where(x => x.OutsoureOrderId == outsourceOrderId).ToListAsync();

            using (var trans = _manuDBContext.Database.BeginTransaction())
            {
                try
                {
                    _mapper.Map(req as OutsourceOrderModel, order);

                    foreach (var d in details)
                    {
                        var s = req.OutsourceOrderDetail.FirstOrDefault(x => x.OutsourceOrderDetailId == d.OutsourceOrderDetailId);
                        if (s == null)
                            throw new BadRequestException(OutsourceErrorCode.NotFoundOutsourOrder);
                        _mapper.Map(s, d);
                    }

                    await _manuDBContext.SaveChangesAsync();
                    await trans.CommitAsync();

                    return true;
                }
                catch (Exception ex)
                {
                    await trans.RollbackAsync();
                    _logger.LogError("UpdateOutsourceOrder");
                    throw ex;
                }
            }
        }
    }
}
