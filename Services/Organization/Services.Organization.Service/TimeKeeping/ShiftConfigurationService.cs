using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Services.Organization.Model.TimeKeeping;
using System.Collections.Generic;
using System.Threading.Tasks;
using Verp.Resources.Organization.TimeKeeping;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
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
            var entity = _mapper.Map<ShiftConfiguration>(model);

            await _organizationDBContext.ShiftConfiguration.AddAsync(entity);
            await _organizationDBContext.SaveChangesAsync();

            if (model.OvertimeConfiguration != null)
            {
                var overtimeEntity = _mapper.Map<OvertimeConfiguration>(model.OvertimeConfiguration);
                await _organizationDBContext.OvertimeConfiguration.AddAsync(overtimeEntity);
                await _organizationDBContext.SaveChangesAsync();

                entity.OvertimeConfigurationId = overtimeEntity.OvertimeConfigurationId;
                await _organizationDBContext.SaveChangesAsync();
            }

            await _shiftActivityLog.LogBuilder(() => ShiftConfigurationActivityLogMessage.CreateShiftConfiguration)
                      .MessageResourceFormatDatas(entity.ShiftCode)
                      .ObjectId(entity.ShiftConfigurationId)
                      .JsonData(model)
                      .CreateLog();

            return entity.ShiftConfigurationId;
        }

        public async Task<bool> UpdateShiftConfiguration(int shiftConfigurationId, ShiftConfigurationModel model)
        {
            var shiftConfiguration = await _organizationDBContext.ShiftConfiguration.FirstOrDefaultAsync(x => x.ShiftConfigurationId == shiftConfigurationId);
            if (shiftConfiguration == null)
                throw new BadRequestException(GeneralCode.ItemNotFound);
            var overtimeConfiguration = await _organizationDBContext.OvertimeConfiguration.FirstOrDefaultAsync(x => x.OvertimeConfigurationId == shiftConfiguration.OvertimeConfigurationId);

            if (overtimeConfiguration != null)
            {
                model.OvertimeConfigurationId = overtimeConfiguration.OvertimeConfigurationId;
                model.OvertimeConfiguration.OvertimeConfigurationId = overtimeConfiguration.OvertimeConfigurationId;
                _mapper.Map(model.OvertimeConfiguration, overtimeConfiguration);
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

            if (overtimeConfiguration != null) overtimeConfiguration.IsDeleted = true;

            await _organizationDBContext.SaveChangesAsync();

            await _shiftActivityLog.LogBuilder(() => ShiftConfigurationActivityLogMessage.DeleteShiftConfiguration)
                     .MessageResourceFormatDatas(shiftConfiguration.ShiftCode)
                     .ObjectId(shiftConfiguration.ShiftConfigurationId)
                     .CreateLog();

            return true;
        }

        public async Task<ShiftConfigurationModel> GetShiftConfiguration(int shiftConfigurationId)
        {
            var shiftConfiguration = await _organizationDBContext.ShiftConfiguration.FirstOrDefaultAsync(x => x.ShiftConfigurationId == shiftConfigurationId);
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
    }
}