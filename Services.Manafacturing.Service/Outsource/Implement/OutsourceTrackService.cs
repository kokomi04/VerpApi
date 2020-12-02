using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.ManufacturingDB;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Manafacturing.Model.Outsource.Track;

namespace VErp.Services.Manafacturing.Service.Outsource.Implement
{
    public class OutsourceTrackService: IOutsourceTrackService
    {
        private readonly ManufacturingDBContext _manufacturingDBContext;
        private readonly IActivityLogService _activityLogService;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly ICustomGenCodeHelperService _customGenCodeHelperService;

        public OutsourceTrackService(ManufacturingDBContext manufacturingDB
            , IActivityLogService activityLogService
            , ILogger<OutsourceTrackService> logger
            , IMapper mapper
            , ICustomGenCodeHelperService customGenCodeHelperService)
        {
            _manufacturingDBContext = manufacturingDB;
            _activityLogService = activityLogService;
            _logger = logger;
            _mapper = mapper;
            _customGenCodeHelperService = customGenCodeHelperService;
        }

        public async Task<IList<OutsourceTrackModel>> SearchOutsourceTrackByOutsourceOrder(long outsourceOrderId)
        {
            var lst = await _manufacturingDBContext.OutsourceTrack
                            .AsNoTracking()
                            .Where(x => x.OutsourceOrderId == outsourceOrderId)
                            .ProjectTo<OutsourceTrackModel>(_mapper.ConfigurationProvider)
                            .ToListAsync();
            return lst;
        }

        public async Task<bool> UpdateOutsourceTrackByOutsourceOrder(long outsourceOrderId, IList<OutsourceTrackModel> req)
        {
            var trans = await _manufacturingDBContext.Database.BeginTransactionAsync();
            try
            {
                var outsourceTracks = await _manufacturingDBContext.OutsourceTrack
                                            .Where(x => x.OutsourceOrderId == outsourceOrderId)
                                            .ToListAsync();
                foreach(var track in outsourceTracks)
                {
                    var rTrack = req.FirstOrDefault(x => x.OutsourceTrackId == track.OutsourceTrackId);
                    if (rTrack != null)
                        _mapper.Map(rTrack, track);
                    else track.IsDeleted = true;
                }

                var newOutsourceTracks = req.AsQueryable()
                                            .ProjectTo<OutsourceTrack>(_mapper.ConfigurationProvider)
                                            .Where(t => outsourceTracks.Select(x => x.OutsourceTrackId).Contains(t.OutsourceTrackId));
                await _manufacturingDBContext.OutsourceTrack.AddRangeAsync(newOutsourceTracks);
                await _manufacturingDBContext.SaveChangesAsync();

                await trans.CommitAsync();
                await _activityLogService.CreateLog(EnumObjectType.OutsourceTrack, outsourceOrderId, "Cập nhật và tạo mới theo dõi gia công", req.JsonSerialize());
                return true;
            }catch(Exception ex)
            {
                await trans.TryRollbackTransactionAsync();
                _logger.LogError(ex, "UpdateOutsourceTrackByOutsourceOrder");
                throw;
            }
        }
    }
}
