using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Services.Organization.Model.TimeKeeping;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.OrganizationDB;

namespace VErp.Services.Organization.Service.TimeKeeping
{
    public interface ITimeSortConfigurationService
    {
        Task<int> AddTimeSortConfiguration(TimeSortConfigurationModel model);
        Task<bool> DeleteTimeSortConfiguration(int timeSortConfigurationId);
        Task<IList<TimeSortConfigurationModel>> GetListTimeSortConfiguration();
        Task<TimeSortConfigurationModel> GetTimeSortConfiguration(int timeSortConfigurationId);
        Task<bool> UpdateTimeSortConfiguration(int timeSortConfigurationId, TimeSortConfigurationModel model);
    }

    public class TimeSortConfigurationService : ITimeSortConfigurationService
    {
        private readonly OrganizationDBContext _organizationDBContext;
        private readonly IMapper _mapper;

        public TimeSortConfigurationService(OrganizationDBContext organizationDBContext, IMapper mapper)
        {
            _organizationDBContext = organizationDBContext;
            _mapper = mapper;
        }

        public async Task<int> AddTimeSortConfiguration(TimeSortConfigurationModel model)
        {
            var entity = _mapper.Map<TimeSortConfiguration>(model);

            await _organizationDBContext.TimeSortConfiguration.AddAsync(entity);
            await _organizationDBContext.SaveChangesAsync();

            if (model.SplitHour != null)
            {
                var arrSplitHourEntity = _mapper.Map<IList<SplitHour>>(model.SplitHour);

                foreach (var element in arrSplitHourEntity)
                {
                    element.TimeSortConfigurationId = entity.TimeSortConfigurationId;
                }

                await _organizationDBContext.SplitHour.AddRangeAsync(arrSplitHourEntity);
                await _organizationDBContext.SaveChangesAsync();
            }

            return entity.TimeSortConfigurationId;
        }

        public async Task<bool> UpdateTimeSortConfiguration(int timeSortConfigurationId, TimeSortConfigurationModel model)
        {
            var timeSortConfiguration = await _organizationDBContext.TimeSortConfiguration.FirstOrDefaultAsync(x => x.TimeSortConfigurationId == timeSortConfigurationId);
            if (timeSortConfiguration == null)
                throw new BadRequestException(GeneralCode.ItemNotFound);
            var arrSplitHour = await _organizationDBContext.SplitHour.Where(x => x.TimeSortConfigurationId == timeSortConfiguration.TimeSortConfigurationId).ToListAsync();

            foreach (var splitHour in arrSplitHour)
            {
                var m = model.SplitHour.FirstOrDefault(x => x.SplitHourId == splitHour.SplitHourId);
                if (m != null)
                    _mapper.Map(m, splitHour);
                else splitHour.IsDeleted = true;
            }

            var arrSplitHourEntity = _mapper.Map<IList<SplitHour>>(model.SplitHour.Where(x => x.SplitHourId <= 0));
            foreach (var element in arrSplitHourEntity)
            {
                element.TimeSortConfigurationId = timeSortConfiguration.TimeSortConfigurationId;
            }

            await _organizationDBContext.SplitHour.AddRangeAsync(arrSplitHourEntity);
            await _organizationDBContext.SaveChangesAsync();

            model.TimeSortConfigurationId = timeSortConfigurationId;

            _mapper.Map(model, timeSortConfiguration);

            await _organizationDBContext.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteTimeSortConfiguration(int timeSortConfigurationId)
        {
            var timeSortConfiguration = await _organizationDBContext.TimeSortConfiguration.FirstOrDefaultAsync(x => x.TimeSortConfigurationId == timeSortConfigurationId);
            if (timeSortConfiguration == null)
                throw new BadRequestException(GeneralCode.ItemNotFound);

            var arrSplitHour = await _organizationDBContext.SplitHour
                                .Where(x => x.TimeSortConfigurationId == timeSortConfiguration.TimeSortConfigurationId)
                                .ToListAsync();

            timeSortConfiguration.IsDeleted = true;

            arrSplitHour.ForEach(x => x.IsDeleted = true);

            await _organizationDBContext.SaveChangesAsync();

            return true;
        }

        public async Task<TimeSortConfigurationModel> GetTimeSortConfiguration(int timeSortConfigurationId)
        {
            var timeSortConfiguration = await _organizationDBContext.TimeSortConfiguration.FirstOrDefaultAsync(x => x.TimeSortConfigurationId == timeSortConfigurationId);
            if (timeSortConfiguration == null)
                throw new BadRequestException(GeneralCode.ItemNotFound);

            return _mapper.Map<TimeSortConfigurationModel>(timeSortConfiguration);
        }

        public async Task<IList<TimeSortConfigurationModel>> GetListTimeSortConfiguration()
        {
            var query = _organizationDBContext.TimeSortConfiguration.AsNoTracking();

            return await query
            .ProjectTo<TimeSortConfigurationModel>(_mapper.ConfigurationProvider)
            .ToListAsync();
        }
    }
}