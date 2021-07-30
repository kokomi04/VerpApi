using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VErp.Commons.Enums.ErrorCodes;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.ManufacturingDB;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Manafacturing.Model.Outsource.Order;
using VErp.Services.Manafacturing.Model.Outsource.Track;
using VErp.Services.Manafacturing.Service.Resources;
using static VErp.Commons.Enums.Manafacturing.EnumOutsourceTrack;
using static VErp.Commons.Enums.Manafacturing.EnumProductionProcess;
using OutsourceOrderMaterialsEnity = VErp.Infrastructure.EF.ManufacturingDB.OutsourceOrderMaterials;

namespace VErp.Services.Manafacturing.Service.Outsource.Implement
{
    public class OutsourcePropertyService : IOutsourcePropertyService
    {
        private readonly ManufacturingDBContext _manufacturingDBContext;
        private readonly IActivityLogService _activityLogService;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly ICustomGenCodeHelperService _customGenCodeHelperService;
        private readonly IOutsourceStepRequestService _outsourceStepRequestService;
        private readonly ICurrentContextService _currentContextService;
        private readonly IProductHelperService _productHelperService;
        private readonly ObjectActivityLogFacade _objectActivityLog;

        public OutsourcePropertyService(ManufacturingDBContext manufacturingDB
            , IActivityLogService activityLogService
            , ILogger<OutsourceStepOrderService> logger
            , IMapper mapper
            , ICustomGenCodeHelperService customGenCodeHelperService
            , IOutsourceStepRequestService outsourceStepRequestService
            , ICurrentContextService currentContextService
            , IProductHelperService productHelperService)
        {
            _manufacturingDBContext = manufacturingDB;
            _activityLogService = activityLogService;
            _logger = logger;
            _mapper = mapper;
            _customGenCodeHelperService = customGenCodeHelperService;
            _outsourceStepRequestService = outsourceStepRequestService;
            _currentContextService = currentContextService;
            _productHelperService = productHelperService;
            _objectActivityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.OutsourceOrder);
        }

        public async Task<long> Create(OutsourcePropertyOrderInput req)
        {
            using (var trans = _manufacturingDBContext.Database.BeginTransaction())
            {
                try
                {
                    if (!req.OutsourceOrderDate.HasValue)
                        req.OutsourceOrderDate = DateTime.Now.Date.GetUnixUtc(_currentContextService.TimeZoneOffset);
                    var ctx = await GenerateCode(null, req);

                    var order = _mapper.Map<OutsourceOrder>(req);
                    order.OutsourceTypeId = (int)EnumOutsourceType.OutsourceMaterial;

                    _manufacturingDBContext.OutsourceOrder.Add(order);
                    await _manufacturingDBContext.SaveChangesAsync();

                    // Danh sách chi tiết gia công
                    var detail = _mapper.Map<List<OutsourceOrderDetail>>(req.OutsourceOrderDetail);
                    detail.ForEach(x => x.OutsourceOrderId = order.OutsourceOrderId);

                    _manufacturingDBContext.OutsourceOrderDetail.AddRange(detail);
                    await _manufacturingDBContext.SaveChangesAsync();

                    // Danh sách các nguyên vật liệu
                    var materials = _mapper.Map<List<OutsourceOrderMaterialsEnity>>(req.OutsourceOrderMaterials);
                    materials.ForEach(x => x.OutsourceOrderId = order.OutsourceOrderId);

                    _manufacturingDBContext.OutsourceOrderMaterials.AddRange(materials);
                    await _manufacturingDBContext.SaveChangesAsync();

                    // Danh sách vật tư dư thừa
                    var excess = _mapper.Map<List<OutsourceOrderExcess>>(req.OutsourceOrderExcesses);
                    excess.ForEach(x => x.OutsourceOrderId = order.OutsourceOrderId);

                    _manufacturingDBContext.OutsourceOrderExcess.AddRange(excess);
                    await _manufacturingDBContext.SaveChangesAsync();


                    // Tạo lịch sử theo dõi lần đầu
                    await CreateOutsourceTrackFirst(order.OutsourceOrderId);

                    await _manufacturingDBContext.SaveChangesAsync();

                    await trans.CommitAsync();

                    await _objectActivityLog.LogBuilder(() => OutsourceMaterialActivityMessage.Create)
                        .MessageResourceFormatDatas(req.OutsourceOrderCode)
                        .ObjectId(order.OutsourceOrderId)
                        .JsonData(req.JsonSerialize())
                        .CreateLog();

                    return order.OutsourceOrderId;
                }
                catch (Exception)
                {
                    await trans.TryRollbackTransactionAsync();
                    throw;
                }
            }
        }
        private async Task<GenerateCodeContext> GenerateCode(long? outsourceOrderId, OutsourcePropertyOrderInput model)
        {
            model.OutsourceOrderCode = (model.OutsourceOrderCode ?? "").Trim();

            var ctx = _customGenCodeHelperService.CreateGenerateCodeContext();

            var code = await ctx
                .SetConfig(EnumObjectType.OutsourceOrder)
                .SetConfigData(outsourceOrderId ?? 0, model.OutsourceOrderDate, null)
                .TryValidateAndGenerateCode(_manufacturingDBContext.OutsourceOrder, model.OutsourceOrderCode, (s, code) => s.OutsourceOrderId != outsourceOrderId && s.OutsourceOrderCode == code);

            model.OutsourceOrderCode = code;

            return ctx;
        }

        private async Task<bool> CreateOutsourceTrackFirst(long outsourceOrderId)
        {
            var track = new OutsourceTrackModel
            {
                OutsourceTrackDate = DateTime.Now.GetUnix(),
                OutsourceTrackDescription = "Tạo đơn gia công",
                OutsourceTrackStatusId = EnumOutsourceTrackStatus.Created,
                OutsourceTrackTypeId = EnumOutsourceTrackType.All,
                OutsourceOrderId = outsourceOrderId
            };

            await _manufacturingDBContext.OutsourceTrack.AddAsync(_mapper.Map<OutsourceTrack>(track));
            await _manufacturingDBContext.SaveChangesAsync();

            return true;
        }

        public async Task<PageData<OutsourcePropertyOrderList>> GetList(string keyword, int page, int size, string orderByFieldName, bool asc, long fromDate, long toDate, Clause filters = null)
        {
            keyword = (keyword ?? "").Trim();

            var outSourceOrder = _manufacturingDBContext.OutsourceOrder.AsQueryable();
            if (fromDate > 0)
            {
                outSourceOrder = outSourceOrder.Where(o => o.OutsourceOrderDate >= fromDate.UnixToDateTime());
            }

            if (toDate > 0)
            {
                outSourceOrder = outSourceOrder.Where(o => o.OutsourceOrderDate <= toDate.UnixToDateTime());
            }

            var query = from o in outSourceOrder
                        join d in _manufacturingDBContext.OutsourceOrderDetail on o.OutsourceOrderId equals d.OutsourceOrderId
                        join r in _manufacturingDBContext.RefPropertyCalc on o.PropertyCalcId equals r.PropertyCalcId into rs
                        from r in rs.DefaultIfEmpty()
                        join c in _manufacturingDBContext.RefCustomer on o.CustomerId equals c.CustomerId into cs
                        from c in cs.DefaultIfEmpty()
                        join p in _manufacturingDBContext.RefProduct on d.ObjectId equals p.ProductId into ps
                        from p in ps.DefaultIfEmpty()
                        where o.OutsourceTypeId == (int)EnumOutsourceType.OutsourceMaterial

                        select new
                        {
                            o.OutsourceOrderId,
                            o.OutsourceOrderCode,
                            o.OutsourceOrderDate,
                            o.OutsourceOrderFinishDate,
                            PropertyCalcCode = r == null ? null : r.PropertyCalcCode,
                            o.PropertyCalcId,
                            d.ObjectId,
                            d.Quantity,
                            CustomerId = c == null ? (int?)null : c.CustomerId,
                            CustomerCode = c == null ? null : c.CustomerCode,
                            CustomerName = c == null ? null : c.CustomerName,
                            ProductId = p == null ? (int?)null : p.ProductId,
                            ProductCode = p == null ? null : p.ProductCode,
                            ProductName = p == null ? null : p.ProductName,
                            UnitId = p == null ? (int?)null : p.UnitId,
                        };



            if (!string.IsNullOrWhiteSpace(keyword))
                query = query.Where(x => x.OutsourceOrderCode.Contains(keyword)
                                   || x.PropertyCalcCode.Contains(keyword)
                                   || x.ProductCode.Contains(keyword)
                                   || x.ProductName.Contains(keyword)
                                   || x.CustomerCode.Contains(keyword)
                                   || x.CustomerName.Contains(keyword));

            if (filters != null)
                query = query.InternalFilter(filters);

            if (!string.IsNullOrWhiteSpace(orderByFieldName))
                query = query.InternalOrderBy(orderByFieldName, asc);

            var total = await query.CountAsync();
            var lst = await (size > 0 ? query.Skip((page - 1) * size).Take(size) : query).ToListAsync();

            return (lst.Select(o => new OutsourcePropertyOrderList()
            {
                OutsourceOrderId = o.OutsourceOrderId,
                OutsourceOrderCode = o.OutsourceOrderCode,
                OutsourceOrderDate = o.OutsourceOrderDate.GetUnix(),
                OutsourceOrderFinishDate = o.OutsourceOrderFinishDate.GetUnix(),
                PropertyCalcCode = o.PropertyCalcCode,
                PropertyCalcId = o.PropertyCalcId,
                ObjectId = o.ObjectId,
                Quantity = o.Quantity,
                CustomerId = o.CustomerId,
                CustomerCode = o.CustomerCode,
                CustomerName = o.CustomerName,
                ProductId = o.ProductId,
                ProductCode = o.ProductCode,
                ProductName = o.ProductName,
                unitId = o.UnitId
            }).ToList(), total);
        }

        public async Task<OutsourcePropertyOrderInput> Info(long outsourceOrderId)
        {
            var outsourceStepOrder = await _manufacturingDBContext.OutsourceOrder.AsNoTracking()
                .ProjectTo<OutsourcePropertyOrderInput>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(x => x.OutsourceOrderId == outsourceOrderId);
            if (outsourceStepOrder == null)
                throw new BadRequestException(OutsourceErrorCode.NotFoundOutsourceOrder);

            var materials = await _manufacturingDBContext.OutsourceOrderMaterials.AsNoTracking()
                .Where(x => x.OutsourceOrderId == outsourceOrderId)
                .ProjectTo<OutsourceOrderMaterialsModel>(_mapper.ConfigurationProvider)
                .ToListAsync();


            var details = await _manufacturingDBContext.OutsourceOrderDetail.AsNoTracking()
                .Where(x => x.OutsourceOrderId == outsourceOrderId)
                .ProjectTo<OutsourcePropertyOrderDetail>(_mapper.ConfigurationProvider)
                .ToListAsync();

            var excesses = await _manufacturingDBContext.OutsourceOrderExcess.AsNoTracking()
             .Where(x => x.OutsourceOrderId == outsourceOrderId)
             .ProjectTo<OutsourceOrderExcessModel>(_mapper.ConfigurationProvider)
             .ToListAsync();

            var productIds = materials.Select(x => (int)x.ProductId).ToList();

            outsourceStepOrder.OutsourceOrderDetail = details;
            outsourceStepOrder.OutsourceOrderMaterials = materials;
            outsourceStepOrder.OutsourceOrderExcesses = excesses;

            return outsourceStepOrder;
        }

        public async Task<OutsourcePropertyOrderInput> GetInfoByPropertyCalcId(long propertyCalcId)
        {
            var info = await _manufacturingDBContext.OutsourceOrder.FirstOrDefaultAsync(o => o.PropertyCalcId == propertyCalcId);
            if (info == null)
            {
                throw new BadRequestException(OutsourceErrorCode.NotFoundOutsourceOrder);
            }
            return await Info(info.OutsourceOrderId);
        }
        public async Task<bool> Update(long outsourceOrderId, OutsourcePropertyOrderInput req)
        {
            var outsourceStepOrder = await _manufacturingDBContext.OutsourceOrder
              .FirstOrDefaultAsync(x => x.OutsourceOrderId == outsourceOrderId);
            if (outsourceStepOrder == null)
                throw new BadRequestException(OutsourceErrorCode.NotFoundOutsourceOrder);

            if (await _manufacturingDBContext.OutsourceOrder.AnyAsync(o => o.OutsourceOrderId != outsourceOrderId && o.OutsourceOrderCode == req.OutsourceOrderCode))
            {
                throw OutsourceErrorCode.OutsoureOrderCodeAlreadyExisted.BadRequest();
            }

            var trans = await _manufacturingDBContext.Database.BeginTransactionAsync();
            try
            {
                //order
                _mapper.Map(req, outsourceStepOrder);
                await _manufacturingDBContext.SaveChangesAsync();

                //detail
                var details = await _manufacturingDBContext.OutsourceOrderDetail
                                                        .Where(x => x.OutsourceOrderId == outsourceStepOrder.OutsourceOrderId)
                                                        .ToListAsync();
                foreach (var detail in details)
                {
                    var uDetail = req.OutsourceOrderDetail.FirstOrDefault(x => x.OutsourceOrderDetailId == detail.OutsourceOrderDetailId);
                    if (uDetail != null)
                        _mapper.Map(uDetail, detail);
                    else detail.IsDeleted = true;
                }

                var newOutsourceStepOrderDetail = req.OutsourceOrderDetail
                    .AsQueryable()
                    .Where(x => x.OutsourceOrderDetailId <= 0)
                    .ProjectTo<OutsourceOrderDetail>(_mapper.ConfigurationProvider)
                    .ToList();
                newOutsourceStepOrderDetail.ForEach(x => x.OutsourceOrderId = outsourceStepOrder.OutsourceOrderId);

                await _manufacturingDBContext.OutsourceOrderDetail.AddRangeAsync(newOutsourceStepOrderDetail);
                await _manufacturingDBContext.SaveChangesAsync();

                //materials
                var materials = await _manufacturingDBContext.OutsourceOrderMaterials
                                                        .Where(x => x.OutsourceOrderId == outsourceStepOrder.OutsourceOrderId)
                                                        .ToListAsync();

                foreach (var m in materials)
                {
                    var s = req.OutsourceOrderMaterials.FirstOrDefault(x => x.OutsourceOrderMaterialsId == m.OutsourceOrderMaterialsId);
                    if (s != null)
                        _mapper.Map(s, m);
                    else m.IsDeleted = true;
                }

                var newMaterials = req.OutsourceOrderMaterials
                    .AsQueryable()
                    .Where(x => x.OutsourceOrderMaterialsId <= 0)
                    .ProjectTo<OutsourceOrderMaterialsEnity>(_mapper.ConfigurationProvider)
                    .ToList();
                newMaterials.ForEach(x => x.OutsourceOrderId = outsourceStepOrder.OutsourceOrderId);

                await _manufacturingDBContext.OutsourceOrderMaterials.AddRangeAsync(newMaterials);
                await _manufacturingDBContext.SaveChangesAsync();

                //excesses
                var excesses = await _manufacturingDBContext.OutsourceOrderExcess.Where(x => x.OutsourceOrderId == outsourceStepOrder.OutsourceOrderId)
                    .ToListAsync();

                foreach (var m in excesses)
                {
                    var s = req.OutsourceOrderExcesses.FirstOrDefault(x => x.OutsourceOrderExcessId == m.OutsourceOrderExcessId);
                    if (s != null)
                        _mapper.Map(s, m);
                    else m.IsDeleted = true;
                }

                var newExcesses = req.OutsourceOrderExcesses
                    .AsQueryable()
                    .Where(x => x.OutsourceOrderExcessId <= 0)
                    .ProjectTo<OutsourceOrderExcess>(_mapper.ConfigurationProvider)
                    .ToList();
                newExcesses.ForEach(x => x.OutsourceOrderId = outsourceStepOrder.OutsourceOrderId);

                await _manufacturingDBContext.OutsourceOrderExcess.AddRangeAsync(newExcesses);

                await _manufacturingDBContext.SaveChangesAsync();

                await trans.CommitAsync();


                await _objectActivityLog.LogBuilder(() => OutsourceMaterialActivityMessage.Update)
                     .MessageResourceFormatDatas(req.OutsourceOrderCode)
                     .ObjectId(outsourceOrderId)
                     .JsonData(req.JsonSerialize())
                     .CreateLog();

                return true;
            }
            catch (Exception)
            {
                await trans.TryRollbackTransactionAsync();
                throw;
            }
        }

        public async Task<bool> Delete(long outsourceOrderId)
        {
            var order = await _manufacturingDBContext.OutsourceOrder
              .FirstOrDefaultAsync(x => x.OutsourceOrderId == outsourceOrderId);
            if (order == null)
                throw new BadRequestException(OutsourceErrorCode.NotFoundOutsourceOrder);

            var trans = await _manufacturingDBContext.Database.BeginTransactionAsync();
            try
            {
                order.IsDeleted = true;

                var detail = await _manufacturingDBContext.OutsourceOrderDetail
                    .Where(x => x.OutsourceOrderId == order.OutsourceOrderId)
                    .ToListAsync();
                detail.ForEach(x => x.IsDeleted = true);

                var materials = await _manufacturingDBContext.OutsourceOrderMaterials
                    .Where(x => x.OutsourceOrderId == order.OutsourceOrderId)
                    .ToListAsync();
                materials.ForEach(x => x.IsDeleted = true);

                await _manufacturingDBContext.SaveChangesAsync();

                await trans.CommitAsync();


                await _objectActivityLog.LogBuilder(() => OutsourceMaterialActivityMessage.Delete)
                     .MessageResourceFormatDatas(order.OutsourceOrderCode)
                     .ObjectId(outsourceOrderId)
                     .JsonData(order.JsonSerialize())
                     .CreateLog();


                return true;
            }
            catch (Exception)
            {
                await trans.TryRollbackTransactionAsync();
                throw;
            }
        }

    }
}
