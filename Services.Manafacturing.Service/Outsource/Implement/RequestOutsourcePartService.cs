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
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.ManufacturingDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Manafacturing.Model.Outsource.RequestPart;

namespace VErp.Services.Manafacturing.Service.Outsource.Implement
{
    public class RequestOutsourcePartService : IRequestOutsourcePartService
    {
        private const int TRACK_OUTSOURCE_TYPE = (int)EnumTrackOutsourceType.OutsourceComposition;

        private readonly ManufacturingDBContext _manufacturingDBContext;
        private readonly IActivityLogService _activityLogService;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        public RequestOutsourcePartService(ManufacturingDBContext manufacturingDB
            , IActivityLogService activityLogService
            , ILogger<RequestOutsourcePartService> logger
            , IMapper mapper)
        {
            _manufacturingDBContext = manufacturingDB;
            _activityLogService = activityLogService;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<int> CreateRequest(RequestOutsourcePartModel req)
        {
            using (var trans = _manufacturingDBContext.Database.BeginTransaction())
            {
                try
                {
                    var o = _mapper.Map<RequestOutsourcePart>(req);
                    await _manufacturingDBContext.RequestOutsourcePart.AddAsync(o);
                    _manufacturingDBContext.SaveChanges();

                    trans.Commit();
                    _activityLogService.CreateLog(EnumObjectType.RequestOutsourcePart, o.RequestOutsourcePartId,
                        $"Tạo YCGC {o.RequestOutsourcePartId} của chi tiết {o.ProductInStepId} trong LSXDetail {o.ProductionOrderDetailId}", o.JsonSerialize());
                    return o.RequestOutsourcePartId;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    _logger.LogError("CreateOutsource");
                    throw;
                }
            }
        }

        public async Task<bool> DeleteRequest(int requestId)
        {
            var outsInfo = await _manufacturingDBContext.RequestOutsourcePart
                .Where(x => x.RequestOutsourcePartId == requestId)
                .FirstOrDefaultAsync();
            if (outsInfo == null)
                throw new BadRequestException(OutsourceErrorCode.NotFoundRquest);
            var tracks = await _manufacturingDBContext.TrackOutsource
                .Where(t => t.OutsourceId == requestId && t.OutsourceType == TRACK_OUTSOURCE_TYPE)
                .ToListAsync();

            using (var trans = _manufacturingDBContext.Database.BeginTransaction())
            {
                try
                {
                    outsInfo.IsDeleted = true;
                    tracks.ForEach(t => t.IsDeleted = true);
                    _manufacturingDBContext.SaveChanges();

                    trans.Commit();
                    _activityLogService.CreateLog(EnumObjectType.RequestOutsourcePart, outsInfo.RequestOutsourcePartId,
                        $"Xóa YCGC {outsInfo.RequestOutsourcePartId} của chi tiết {outsInfo.ProductInStepId} trong LSXDetail {outsInfo.ProductionOrderDetailId}", outsInfo.JsonSerialize());
                    return true;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    _logger.LogError("CreateOutsource");
                    throw;
                }
            }
        }

        public async Task<PageData<RequestOutsourcePartModel>> GetListRequest(string keyWord, int page, int size)
        {
            var query = _manufacturingDBContext.RequestOutsourcePart.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(keyWord))
                query = query.Where(x => x.RequestOrder.Contains(keyWord));

            var total = await query.CountAsync();

            var data = query.Skip((page - 1) * size).Take(size)
                        .ProjectTo<RequestOutsourcePartModel>(_mapper.ConfigurationProvider)
                        .ToList();

            return (data, total);
        }

        public async Task<RequestOutsourcePartModel> GetRequestById(int requestId)
        {
            var info = await _manufacturingDBContext.RequestOutsourcePart
                .FirstOrDefaultAsync(X => X.RequestOutsourcePartId == requestId);
            if (info == null)
                throw new BadRequestException(OutsourceErrorCode.NotFoundRquest);
            return _mapper.Map<RequestOutsourcePartModel>(info);
        }

        public async Task<bool> UpdateRequest(int requestId, RequestOutsourcePartModel req)
        {
            var outsInfo = await _manufacturingDBContext.RequestOutsourcePart
                .Where(x => x.RequestOutsourcePartId == requestId)
                .FirstOrDefaultAsync();
            if (outsInfo == null)
                throw new BadRequestException(OutsourceErrorCode.NotFoundRquest);

            _mapper.Map(req, outsInfo);
            _manufacturingDBContext.SaveChanges();

            _activityLogService.CreateLog(EnumObjectType.RequestOutsourcePart, outsInfo.RequestOutsourcePartId,
                        $"Cập nhật YCGC {outsInfo.RequestOutsourcePartId} của chi tiết {outsInfo.ProductInStepId} trong LSXDetail {outsInfo.ProductionOrderDetailId}", outsInfo.JsonSerialize());
            return true;
        }
    }
}
