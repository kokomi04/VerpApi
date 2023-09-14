using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Services.Organization.Model.TimeKeeping;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Verp.Resources.Organization.TimeKeeping;
using Verp.Resources.Organization.TimeKeeping.Validation;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.OrganizationDB;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Service;

namespace VErp.Services.Organization.Service.TimeKeeping
{
    public interface IShiftConfigurationService
    {
        Task<int> AddShiftConfiguration(ShiftConfigurationModel model);
        Task<bool> DeleteShiftConfiguration(int shiftConfigurationId);
        Task<IList<ShiftConfigurationModel>> GetListShiftConfiguration();
        Task<ShiftConfigurationModel> GetShiftConfiguration(int shiftConfigurationId);
        Task<bool> UpdateShiftConfiguration(int shiftConfigurationId, ShiftConfigurationModel model);
    }

    public class ShiftConfigurationService : IShiftConfigurationService
    {
        private readonly OrganizationDBContext _organizationDBContext;
        private readonly IMapper _mapper;
        private readonly ObjectActivityLogFacade _shiftActivityLog;

        public ShiftConfigurationService(OrganizationDBContext organizationDBContext, IMapper mapper, IActivityLogService activityLogService)
        {
            _organizationDBContext = organizationDBContext;
            _mapper = mapper;
            _shiftActivityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.ShiftConfiguration);
        }

        public async Task<int> AddShiftConfiguration(ShiftConfigurationModel model)
        {
            if (await _organizationDBContext.ShiftConfiguration.AnyAsync(s => s.ShiftCode == model.ShiftCode))
                throw ShiftConfigurationValidationMessage.ShiftCodeIsUnique.BadRequest();

            await ValidateModel(model);
            var entity = _mapper.Map<ShiftConfiguration>(model);
            await _organizationDBContext.ShiftConfiguration.AddAsync(entity);
            await _organizationDBContext.SaveChangesAsync();

            await _shiftActivityLog.LogBuilder(() => ShiftConfigurationActivityLogMessage.CreateShiftConfiguration)
                      .MessageResourceFormatDatas(entity.ShiftCode)
                      .ObjectId(entity.ShiftConfigurationId)
                      .JsonData(model)
                      .CreateLog();

            return entity.ShiftConfigurationId;
        }

        public async Task<bool> UpdateShiftConfiguration(int shiftConfigurationId, ShiftConfigurationModel model)
        {
            await ValidateModel(model);

            var shiftConfiguration = await _organizationDBContext.ShiftConfiguration.FirstOrDefaultAsync(x => x.ShiftConfigurationId == shiftConfigurationId);
            if (shiftConfiguration == null)
                throw new BadRequestException(GeneralCode.ItemNotFound);

            if (model.ShiftCode != shiftConfiguration.ShiftCode && await _organizationDBContext.ShiftConfiguration.AnyAsync(s => s.ShiftCode == model.ShiftCode))
                throw ShiftConfigurationValidationMessage.ShiftCodeIsUnique.BadRequest();

            var overtimeConfiguration = await _organizationDBContext.OvertimeConfiguration
                .FirstOrDefaultAsync(x => x.OvertimeConfigurationId == shiftConfiguration.OvertimeConfigurationId);

            if (overtimeConfiguration != null)
            {
                model.OvertimeConfigurationId = overtimeConfiguration.OvertimeConfigurationId;
                model.OvertimeConfiguration.OvertimeConfigurationId = overtimeConfiguration.OvertimeConfigurationId;

                _mapper.Map(model.OvertimeConfiguration, overtimeConfiguration);

                await RemoveOvertimeConfigurationMapping(overtimeConfiguration.OvertimeConfigurationId);

                if (model.OvertimeConfiguration.OvertimeConfigurationMapping != null)
                {
                    var mappings = new List<OvertimeConfigurationMapping>();

                    foreach (var item in model.OvertimeConfiguration.OvertimeConfigurationMapping)
                    {
                        var mappingEntity = _mapper.Map<OvertimeConfigurationMapping>(item);
                        mappings.Add(mappingEntity);
                    }
                    await _organizationDBContext.InsertByBatch(mappings, true, false);
                    await _organizationDBContext.SaveChangesAsync();
                }
            }

            model.ShiftConfigurationId = shiftConfigurationId;

            _mapper.Map(model, shiftConfiguration);

            await _organizationDBContext.SaveChangesAsync();

            await _shiftActivityLog.LogBuilder(() => ShiftConfigurationActivityLogMessage.UpdateShiftConfiguration)
                      .MessageResourceFormatDatas(shiftConfiguration.ShiftCode)
                      .ObjectId(shiftConfiguration.ShiftConfigurationId)
                      .JsonData(model)
                      .CreateLog();

            return true;
        }

        public async Task<bool> DeleteShiftConfiguration(int shiftConfigurationId)
        {
            var shiftConfiguration = await _organizationDBContext.ShiftConfiguration.FirstOrDefaultAsync(x => x.ShiftConfigurationId == shiftConfigurationId);
            if (shiftConfiguration == null)
                throw new BadRequestException(GeneralCode.ItemNotFound);

            var overtimeConfiguration = await _organizationDBContext.OvertimeConfiguration.FirstOrDefaultAsync(x => x.OvertimeConfigurationId == shiftConfiguration.OvertimeConfigurationId);

            shiftConfiguration.IsDeleted = true;

            if (overtimeConfiguration != null)
            {
                overtimeConfiguration.IsDeleted = true;

                await RemoveOvertimeConfigurationMapping(overtimeConfiguration.OvertimeConfigurationId);
            }

            await _organizationDBContext.SaveChangesAsync();

            await _shiftActivityLog.LogBuilder(() => ShiftConfigurationActivityLogMessage.DeleteShiftConfiguration)
                     .MessageResourceFormatDatas(shiftConfiguration.ShiftCode)
                     .ObjectId(shiftConfiguration.ShiftConfigurationId)
                     .CreateLog();

            return true;
        }

        public async Task<ShiftConfigurationModel> GetShiftConfiguration(int shiftConfigurationId)
        {
            var shiftConfiguration = await _organizationDBContext.ShiftConfiguration
                .Include(c => c.OvertimeConfiguration)
                .ThenInclude(m => m.OvertimeConfigurationMapping)
                .FirstOrDefaultAsync(x => x.ShiftConfigurationId == shiftConfigurationId);

            if (shiftConfiguration == null)
                throw new BadRequestException(GeneralCode.ItemNotFound);

            return _mapper.Map<ShiftConfigurationModel>(shiftConfiguration);
        }

        public async Task<IList<ShiftConfigurationModel>> GetListShiftConfiguration()
        {
            var query = _organizationDBContext.ShiftConfiguration.AsNoTracking();

            return await query
            .ProjectTo<ShiftConfigurationModel>(_mapper.ConfigurationProvider)
            .ToListAsync();
        }

        private async Task RemoveOvertimeConfigurationMapping(int overtimeConfigurationId)
        {
            var overtimeConfigurationMappings = _organizationDBContext.OvertimeConfigurationMapping
                    .Where(m => m.OvertimeConfigurationId == overtimeConfigurationId).AsNoTracking();

            _organizationDBContext.OvertimeConfigurationMapping.RemoveRange(overtimeConfigurationMappings);
            await _organizationDBContext.SaveChangesAsync();
        }

        private async Task ValidateModel(ShiftConfigurationModel model)
        {
            if (string.IsNullOrWhiteSpace(model.ShiftCode))
                throw ShiftConfigurationValidationMessage.ShiftCodeIsRequired.BadRequest();

            if (model.LunchTimeStart != 0 && model.LunchTimeFinish != 0 && model.LunchTimeStart >= model.LunchTimeFinish)
                throw ShiftConfigurationValidationMessage.InvalidLunchTime.BadRequest();

            if (model.StartTimeOnRecord >= model.EndTimeOnRecord)
                throw ShiftConfigurationValidationMessage.InvalidTimeOnRecord.BadRequest();

            if (model.StartTimeOutRecord >= model.EndTimeOutRecord)
                throw ShiftConfigurationValidationMessage.InvalidTimeOutRecord.BadRequest();

            if (model.OvertimeConfigurationId.HasValue)
            {
                if (!_organizationDBContext.OvertimeConfiguration.Any(c => c.OvertimeConfigurationId == model.OvertimeConfigurationId))
                    throw OvertimeConfigurationValidationMessage.OvertimeConfigurationNotFound.BadRequest();

                HashSet<int> uniqueOvertimeLevelIds = new HashSet<int>();

                if (model.OvertimeConfiguration.OvertimeConfigurationMapping != null)
                {
                    foreach (var item in model.OvertimeConfiguration.OvertimeConfigurationMapping)
                    {
                        var overtimeLevel = await _organizationDBContext.OvertimeLevel.FindAsync(item.OvertimeLevelId);

                        if (overtimeLevel == null)
                            throw OvertimeConfigurationValidationMessage.OvertimeLevelNotExist.BadRequestFormat(item.OvertimeLevelId);

                        if (uniqueOvertimeLevelIds.Contains(item.OvertimeLevelId))
                            throw OvertimeConfigurationValidationMessage.DuplicateOvertimeLevel.BadRequestFormat(overtimeLevel.OvertimeCode, overtimeLevel.Description);

                        uniqueOvertimeLevelIds.Add(item.OvertimeLevelId);
                    }
                }

            }
        }
    }
}