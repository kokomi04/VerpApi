using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verp.Resources.Organization.Leave;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.OrganizationDB;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Organization.Model.Leave;

namespace VErp.Services.Organization.Service.Leave
{
    public interface ILeaveConfigService
    {
        Task<IList<LeaveConfigListModel>> Get();

        Task<LeaveConfigModel> Info(int leaveConfigId);

        Task<LeaveConfigModel> Default();

        Task<int> Create(LeaveConfigModel model);

        Task<bool> Update(int leaveConfigId, LeaveConfigModel model);

        Task<bool> Delete(int leaveConfigId);
    }

    public class LeaveConfigService : ILeaveConfigService
    {

        private readonly OrganizationDBContext _organizationDBContext;
        private readonly IMapper _mapper;
        private readonly ObjectActivityLogFacade _leaveConfigActivityLog;

        public LeaveConfigService(OrganizationDBContext organizationDBContext, IMapper mapper, IActivityLogService activityLogService)
        {
            _organizationDBContext = organizationDBContext;
            _mapper = mapper;
            _leaveConfigActivityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.LeaveConfig);
        }


        public async Task<int> Create(LeaveConfigModel model)
        {
            var info = _mapper.Map<LeaveConfig>(model);

            var roles = _mapper.Map<List<LeaveConfigRole>>(model.Roles);

            var seniorities = _mapper.Map<List<LeaveConfigSeniority>>(model.Seniorities);

            var valications = _mapper.Map<List<LeaveConfigValidation>>(model.Validations);

            using var trans = await _organizationDBContext.Database.BeginTransactionAsync();


            await _organizationDBContext.LeaveConfig.AddAsync(info);

            await _organizationDBContext.SaveChangesAsync();
            if (roles != null)
            {
                foreach (var r in roles)
                {
                    r.LeaveConfigId = info.LeaveConfigId;
                }

                await _organizationDBContext.LeaveConfigRole.AddRangeAsync(roles);
            }

            if (seniorities != null)
            {
                foreach (var s in seniorities)
                {
                    s.LeaveConfigId = info.LeaveConfigId;
                }
                await _organizationDBContext.LeaveConfigSeniority.AddRangeAsync(seniorities);

            }

            if (valications != null)
            {
                foreach (var v in valications)
                {
                    v.LeaveConfigId = info.LeaveConfigId;
                }

                await _organizationDBContext.LeaveConfigValidation.AddRangeAsync(valications);
            }


            await trans.CommitAsync();

            await _leaveConfigActivityLog.LogBuilder(() => LeaveConfigActivityLogMessage.Create)
                .MessageResourceFormatDatas(info.Title)
                .ObjectId(info.LeaveConfigId)
                .JsonData(model.JsonSerialize())
                .CreateLog();

            return info.LeaveConfigId;

        }

        public async Task<bool> Delete(int leaveConfigId)
        {
            using var trans = await _organizationDBContext.Database.BeginTransactionAsync();

            var model = await Info(leaveConfigId);

            var info = await _organizationDBContext.LeaveConfig.FirstOrDefaultAsync(c => c.LeaveConfigId == leaveConfigId);
            if (info == null)
            {
                throw GeneralCode.ItemNotFound.BadRequest();
            }

            info.IsDeleted = true;


            await trans.CommitAsync();

            await _leaveConfigActivityLog.LogBuilder(() => LeaveConfigActivityLogMessage.Delete)
                .MessageResourceFormatDatas(info.Title)
                .ObjectId(info.LeaveConfigId)
                .JsonData(model.JsonSerialize())
                .CreateLog();

            return true;
        }

        public async Task<IList<LeaveConfigListModel>> Get()
        {
            return _mapper.Map<List<LeaveConfigListModel>>(await _organizationDBContext.LeaveConfig.ToListAsync());
        }

        public async Task<LeaveConfigModel> Info(int leaveConfigId)
        {
            var info = await _organizationDBContext.LeaveConfig.FirstOrDefaultAsync(c => c.LeaveConfigId == leaveConfigId);
            if (info == null)
            {
                throw GeneralCode.ItemNotFound.BadRequest();
            }
            var model = _mapper.Map<LeaveConfigModel>(info);

            var roles = await _organizationDBContext.LeaveConfigRole.Where(c => c.LeaveConfigId == leaveConfigId).ToListAsync();
            var seniorities = await _organizationDBContext.LeaveConfigSeniority.Where(c => c.LeaveConfigId == leaveConfigId).ToListAsync();
            var validation = await _organizationDBContext.LeaveConfigValidation.Where(c => c.LeaveConfigId == leaveConfigId).ToListAsync();

            model.Roles = _mapper.Map<List<LeaveConfigRoleModel>>(roles);
            model.Seniorities = _mapper.Map<List<LeaveConfigSeniorityModel>>(seniorities);
            model.Validations = _mapper.Map<List<LeaveConfigValidationModel>>(validation);
            return model;
        }

        public async Task<LeaveConfigModel> Default()
        {
            var info = await _organizationDBContext.LeaveConfig.FirstOrDefaultAsync(c => c.IsDefault);
            if (info == null)
            {
                throw GeneralCode.ItemNotFound.BadRequest();
            }
            return await Info(info.LeaveConfigId);
        }

        public async Task<bool> Update(int leaveConfigId, LeaveConfigModel model)
        {
            model.LeaveConfigId = leaveConfigId;


            var roles = _mapper.Map<List<LeaveConfigRole>>(model.Roles);

            var seniorities = _mapper.Map<List<LeaveConfigSeniority>>(model.Seniorities);

            var valications = _mapper.Map<List<LeaveConfigValidation>>(model.Validations);

            using var trans = await _organizationDBContext.Database.BeginTransactionAsync();


            var info = await _organizationDBContext.LeaveConfig.FirstOrDefaultAsync(c => c.LeaveConfigId == leaveConfigId);

            if (info == null)
            {
                throw GeneralCode.ItemNotFound.BadRequest();
            }

            _mapper.Map(model, info);


            await _organizationDBContext.LeaveConfigRole.Where(r => r.LeaveConfigId == leaveConfigId).DeleteByBatch();

            await _organizationDBContext.LeaveConfigSeniority.Where(r => r.LeaveConfigId == leaveConfigId).DeleteByBatch();

            await _organizationDBContext.LeaveConfigValidation.Where(r => r.LeaveConfigId == leaveConfigId).DeleteByBatch();


            if (roles != null)
            {              
                foreach (var r in roles)
                {
                    r.LeaveConfigId = info.LeaveConfigId;
                }

                await _organizationDBContext.LeaveConfigRole.AddRangeAsync(roles);
            }

            if (seniorities != null)
            {
                foreach (var s in seniorities)
                {
                    s.LeaveConfigId = info.LeaveConfigId;
                }
                await _organizationDBContext.LeaveConfigSeniority.AddRangeAsync(seniorities);

            }

            if (valications != null)
            {
                foreach (var v in valications)
                {
                    v.LeaveConfigId = info.LeaveConfigId;
                }

                await _organizationDBContext.LeaveConfigValidation.AddRangeAsync(valications);
            }


            await trans.CommitAsync();

            await _leaveConfigActivityLog.LogBuilder(() => LeaveConfigActivityLogMessage.Update)
                .MessageResourceFormatDatas(info.Title)
                .ObjectId(info.LeaveConfigId)
                .JsonData(model.JsonSerialize())
                .CreateLog();
            return true;
        }
    }
}
