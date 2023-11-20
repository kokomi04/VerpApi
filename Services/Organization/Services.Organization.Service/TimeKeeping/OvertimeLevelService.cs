using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using OpenXmlPowerTools;
using Services.Organization.Model.TimeKeeping;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.OrganizationDB;

namespace VErp.Services.Organization.Service.TimeKeeping
{
    public interface IOvertimeLevelService
    {
        Task<int> AddOvertimeLevel(OvertimeLevelModel model);
        Task<bool> DeleteOvertimeLevel(int overtimeLevelId);
        Task<IList<OvertimeLevelModel>> GetListOvertimeLevel();
        Task<OvertimeLevelModel> GetOvertimeLevel(int overtimeLevelId);
        Task<bool> UpdateOvertimeLevel(int overtimeLevelId, OvertimeLevelModel model);
        Task<bool> UpdateOvertimeLevelSortOrder(IList<OvertimeLevelModel> model);
    }

    public class OvertimeLevelService : IOvertimeLevelService
    {
        private readonly OrganizationDBContext _organizationDBContext;
        private readonly IMapper _mapper;

        public OvertimeLevelService(OrganizationDBContext organizationDBContext, IMapper mapper)
        {
            _organizationDBContext = organizationDBContext;
            _mapper = mapper;
        }

        public async Task<int> AddOvertimeLevel(OvertimeLevelModel model)
        {
            if (await _organizationDBContext.OvertimeLevel.AnyAsync(a => a.OvertimeCode == model.OvertimeCode))
                throw new BadRequestException(GeneralCode.InvalidParams, "Ký hiệu mức tăng ca đã tồn tại");

            var maxSortOrder = await _organizationDBContext.OvertimeLevel.MaxAsync(m => m.SortOrder);

            await UpdateSortOrder(maxSortOrder + 1, model.SortOrder);

            var entity = _mapper.Map<OvertimeLevel>(model);

            await _organizationDBContext.OvertimeLevel.AddAsync(entity);
            await _organizationDBContext.SaveChangesAsync();

            return entity.OvertimeLevelId;
        }

        public async Task<bool> UpdateOvertimeLevel(int overtimeLevelId, OvertimeLevelModel model)
        {
            var overtimeLevel = await _organizationDBContext.OvertimeLevel.FirstOrDefaultAsync(x => x.OvertimeLevelId == overtimeLevelId);
            if (overtimeLevel == null)
                throw new BadRequestException(GeneralCode.ItemNotFound);

            if (overtimeLevel.OvertimeCode != model.OvertimeCode && await _organizationDBContext.OvertimeLevel.AnyAsync(a => a.OvertimeCode == model.OvertimeCode))
                throw new BadRequestException(GeneralCode.InvalidParams, "Ký hiệu mức tăng ca đã tồn tại");

            await UpdateSortOrder(overtimeLevel.SortOrder, model.SortOrder);

            model.OvertimeLevelId = overtimeLevelId;
            _mapper.Map(model, overtimeLevel);

            await _organizationDBContext.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteOvertimeLevel(int overtimeLevelId)
        {
            var overtimeLevel = await _organizationDBContext.OvertimeLevel.FirstOrDefaultAsync(x => x.OvertimeLevelId == overtimeLevelId);
            if (overtimeLevel == null)
                throw new BadRequestException(GeneralCode.ItemNotFound);

            await ValidateWithShiftConfig(overtimeLevelId);

            await UpdateSortOrder(overtimeLevel.SortOrder, null);

            overtimeLevel.IsDeleted = true;
            await _organizationDBContext.SaveChangesAsync();

            return true;
        }

        public async Task<OvertimeLevelModel> GetOvertimeLevel(int overtimeLevelId)
        {
            var overtimeLevel = await _organizationDBContext.OvertimeLevel.FirstOrDefaultAsync(x => x.OvertimeLevelId == overtimeLevelId);
            if (overtimeLevel == null)
                throw new BadRequestException(GeneralCode.ItemNotFound);

            return _mapper.Map<OvertimeLevelModel>(overtimeLevel);
        }

        public async Task<IList<OvertimeLevelModel>> GetListOvertimeLevel()
        {
            return await _organizationDBContext.OvertimeLevel
                .OrderBy(o => o.SortOrder)
                .ProjectTo<OvertimeLevelModel>(_mapper.ConfigurationProvider)
                .ToArrayAsync();
        }

        public async Task<bool> UpdateOvertimeLevelSortOrder(IList<OvertimeLevelModel> model)
        {
            var overtimeLevelIds = model.Select(model => model.OvertimeLevelId).ToList();

            var entities = await _organizationDBContext.OvertimeLevel.Where(x => overtimeLevelIds.Contains(x.OvertimeLevelId)).ToListAsync();
            if(!entities.Any())
                throw new BadRequestException(GeneralCode.ItemNotFound);

            foreach (var ov in model)
            {
                entities.ForEach(e =>
                {
                    if(e.OvertimeLevelId == ov.OvertimeLevelId)
                        e.SortOrder = ov.SortOrder;
                });
            }

            await _organizationDBContext.SaveChangesAsync();
            return true;
        }

        private async Task UpdateSortOrder(int entitySortOrder, int? modelSortOrder)
        {
            if (entitySortOrder > modelSortOrder)
            {
                var behindOvertimeLevels = await _organizationDBContext.OvertimeLevel.Where(x => x.SortOrder >= modelSortOrder && x.SortOrder < entitySortOrder).ToListAsync();
                if (behindOvertimeLevels.Any())
                    behindOvertimeLevels.ForEach(x => x.SortOrder++);
            }

            if (entitySortOrder < modelSortOrder)
            {
                var behindOvertimeLevels = await _organizationDBContext.OvertimeLevel.Where(x => x.SortOrder <= modelSortOrder && x.SortOrder > entitySortOrder).ToListAsync();
                if (behindOvertimeLevels.Any())
                    behindOvertimeLevels.ForEach(x => x.SortOrder--);
            }

            if (!modelSortOrder.HasValue)
            {
                var behindOvertimeLevels = await _organizationDBContext.OvertimeLevel.Where(x => x.SortOrder > entitySortOrder).ToListAsync();
                if (behindOvertimeLevels.Any())
                    behindOvertimeLevels.ForEach(x => x.SortOrder--);
            }
        }

        private async Task ValidateWithShiftConfig(int overtimeLevelId)
        {
            var overtimeConfig = await _organizationDBContext.OvertimeConfiguration.Include(o => o.OvertimeConfigurationMapping).FirstOrDefaultAsync(s => (s.IsWeekdayLevel && s.WeekdayLevel == overtimeLevelId)
                    || (s.IsWeekendLevel && s.WeekendLevel == overtimeLevelId)
                    || (s.IsHolidayLevel && s.HolidayLevel == overtimeLevelId)
                    || (s.IsWeekdayOvertimeLevel && s.WeekdayOvertimeLevel == overtimeLevelId)
                    || (s.IsWeekendOvertimeLevel && s.WeekendOvertimeLevel == overtimeLevelId)
                    || (s.IsHolidayOvertimeLevel && s.HolidayOvertimeLevel == overtimeLevelId)
                    || (s.OvertimeConfigurationMapping.Any(m => m.OvertimeLevelId == overtimeLevelId)));


            if (overtimeConfig != null)
            {
                var shift = await _organizationDBContext.ShiftConfiguration.FirstOrDefaultAsync(s => s.OvertimeConfigurationId == overtimeConfig.OvertimeConfigurationId);
                throw new BadRequestException(GeneralCode.ItemInUsed, $"Ký hiệu này đang được sử dụng ở ca làm việc {shift.ShiftCode}");
            }
        }
    }
}