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

        public async Task<int> CreateRequestOutsourcePart(RequestOutsourcePartModel req)
        {
            using (var trans = _manufacturingDBContext.Database.BeginTransaction())
            {
                try
                {
                    var quest = _mapper.Map<RequestOutsourcePart>(req);
                    await _manufacturingDBContext.RequestOutsourcePart.AddAsync(quest);
                    _manufacturingDBContext.SaveChanges();

                    var details = _mapper.Map<List<RequestOutsourcePartDetail>>(req.RequestOutsourcePartDetail);
                    details.ForEach(x => x.RequestOutsourcePartId = quest.RequestOutsourcePartId);
                    await _manufacturingDBContext.RequestOutsourcePartDetail.AddRangeAsync(details);
                    _manufacturingDBContext.SaveChanges();

                    trans.Commit();
                    _activityLogService.CreateLog(EnumObjectType.RequestOutsourcePart, quest.RequestOutsourcePartId,
                        $"Tạo YCGC chi tiết {quest.RequestOutsourcePartId}", quest.JsonSerialize());
                    return quest.RequestOutsourcePartId;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    _logger.LogError("CreateRequestOutsourcePart");
                    throw;
                }
            }
        }

        public async Task<bool> DeleteRequestOutsourcePart(int requestId)
        {
            var outsInfo = await _manufacturingDBContext.RequestOutsourcePart
                .Where(x => x.RequestOutsourcePartId == requestId)
                .FirstOrDefaultAsync();
            if (outsInfo == null)
                throw new BadRequestException(OutsourceErrorCode.NotFoundRquest);
            var tracks = await _manufacturingDBContext.TrackOutsource
                .Where(t => t.OutsourceId == requestId && t.OutsourceType == TRACK_OUTSOURCE_TYPE)
                .ToListAsync();
            var details = await _manufacturingDBContext.RequestOutsourcePartDetail
                .Where(x => x.RequestOutsourcePartId == requestId)
                .ToListAsync();

            using (var trans = _manufacturingDBContext.Database.BeginTransaction())
            {
                try
                {
                    outsInfo.IsDeleted = true;
                    tracks.ForEach(t => t.IsDeleted = true);
                    details.ForEach(d => d.IsDeleted = true);
                    _manufacturingDBContext.SaveChanges();

                    trans.Commit();
                    _activityLogService.CreateLog(EnumObjectType.RequestOutsourcePart, outsInfo.RequestOutsourcePartId,
                        $"Xóa YCGC chi tiết {outsInfo.RequestOutsourcePartId}", outsInfo.JsonSerialize());
                    return true;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    _logger.LogError("DeleteRequestOutsourcePart");
                    throw;
                }
            }
        }

        public async Task<PageData<RequestOutsourcePartModel>> GetListRequestOutsourcePart(string keyWord, int page, int size)
        {
            var query = _manufacturingDBContext.RequestOutsourcePart.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(keyWord))
                query = query.Where(x => x.RequestOutsourcePartCode.Contains(keyWord));

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

        public async Task<bool> UpdateRequestOutsourcePart(int requestId, RequestOutsourcePartModel req)
        {
            var request = await _manufacturingDBContext.RequestOutsourcePart
                .Where(x => x.RequestOutsourcePartId == requestId)
                .FirstOrDefaultAsync();
            if (request == null)
                throw new BadRequestException(OutsourceErrorCode.NotFoundRquest);

            var details = await _manufacturingDBContext.RequestOutsourcePartDetail
                .Where(x => x.RequestOutsourcePartId == requestId)
                .ToListAsync();
            using(var trans = _manufacturingDBContext.Database.BeginTransaction())
            {
                try
                {
                    _mapper.Map(req, request);

                    foreach (var dest in details)
                    {
                        var source = req.RequestOutsourcePartDetail.FirstOrDefault(x => x.RequestOutsourcePartDetailId == dest.RequestOutsourcePartDetailId);
                        if (source != null)
                            _mapper.Map(source, dest);
                    }

                    _manufacturingDBContext.SaveChanges();

                    _activityLogService.CreateLog(EnumObjectType.RequestOutsourcePart, request.RequestOutsourcePartId,
                                $"Cập nhật YCGC chi tiết {request.RequestOutsourcePartId}", request.JsonSerialize());
                    return true;
                }
                catch(Exception ex)
                {
                    trans.Rollback();
                    _logger.LogError("UpdateRequestOutsourcePart");
                    throw;
                }
            }
            
        }
    }
}
