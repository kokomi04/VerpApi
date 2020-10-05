using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Services.Master.Model.Guide;
using GuideEntity = VErp.Infrastructure.EF.MasterDB.Guide;

namespace VErp.Services.Master.Service.Guide.Implement
{
    public class GuideService : IGuideService
    {
        private readonly MasterDBContext _masterDBContext;
        private readonly ILogger<GuideService> _logger;
        private readonly IMapper _mapper;

        public GuideService(MasterDBContext masterDBContext, ILogger<GuideService> logger, IMapper mapper)
        {
            _masterDBContext = masterDBContext;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<int> Create(GuideModel model)
        {
            var entity = _mapper.Map<GuideEntity>(model);

            _masterDBContext.Guide.Add(entity);
            await _masterDBContext.SaveChangesAsync();

            return entity.GuideId;
        }

        public async Task<bool> Deleted(int guideId)
        {
            var g = await _masterDBContext.Guide.FirstOrDefaultAsync(g => g.GuideId == guideId);
            if (g == null)
                throw new BadRequestException(GeneralCode.ItemNotFound);

            g.IsDeleted = true;
            await _masterDBContext.SaveChangesAsync();

            return true;
        }

        public async Task<List<GuideModel>> GetList()
        {
            var ls = _masterDBContext.Guide.AsNoTracking()
                .ProjectTo<GuideModel>(_mapper.ConfigurationProvider)
                .ToList();
            return ls;
        }

        public async Task<bool> Update(int guideId, GuideModel model)
        {
            var g = await _masterDBContext.Guide.FirstOrDefaultAsync(g => g.GuideId == guideId);
            if (g == null)
                throw new BadRequestException(GeneralCode.ItemNotFound);

            _mapper.Map(model, g);

            await _masterDBContext.SaveChangesAsync();

            return true;
        }
    }
}
