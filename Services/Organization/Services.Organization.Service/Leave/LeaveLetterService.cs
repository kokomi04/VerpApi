using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verp.Resources.Organization.Leave;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.Organization;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.OrganizationDB;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Organization.Model.Leave;
using LeaveLetter = VErp.Infrastructure.EF.OrganizationDB.Leave;

namespace VErp.Services.Organization.Service.Leave
{
    public interface ILeaveLetterService
    {
        Task<PageData<LeaveModel>> Get(int? userId, int? roleUserId, string keyword, int? leaveConfigId, int? absenceTypeSymbolId, EnumLeaveStatus? leaveStatusId, long? fromDate, long? toDate, int page, int size, string sortBy, bool asc);

        Task<LeaveModel> Info(long leaveId);

        Task<long> Create(LeaveModel model);

        Task<bool> Update(long leaveId, LeaveModel model);

        Task<bool> Delete(long leaveId);

        Task<bool> CheckAccept(long leaveId);

        Task<bool> CheckReject(long leaveId);

        Task<bool> CensorAccept(long leaveId);

        Task<bool> CensorReject(long leaveId);
    }

    public class LeaveLetterService : ILeaveLetterService
    {
        private readonly OrganizationDBContext _organizationDBContext;
        private readonly IMapper _mapper;
        private readonly ObjectActivityLogFacade _leaveConfigActivityLog;
        private readonly ICurrentContextService _currentContextService;
        private readonly IUserHelperService _userHelperService;
        private readonly ILeaveConfigService _leaveConfigService;

        public LeaveLetterService(
            OrganizationDBContext organizationDBContext,
            IMapper mapper,
            IActivityLogService activityLogService,
            ICurrentContextService currentContextService,
            IUserHelperService userHelperService,
            ILeaveConfigService leaveConfigService

            )
        {
            _organizationDBContext = organizationDBContext;
            _mapper = mapper;
            _leaveConfigActivityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.LeaveBill);
            _currentContextService = currentContextService;
            _userHelperService = userHelperService;
            _leaveConfigService = leaveConfigService;
        }


        private async Task<LeaveConfigModel> GetLeaveConfig(UserInfoOutput user)
        {
            if (user.LeaveConfigId.HasValue)
            {
                return await _leaveConfigService.Info(user.LeaveConfigId.Value);
            }
            else
            {
                return await _leaveConfigService.Default();
            }
        }

        private void Validate(LeaveConfigModel cfg, LeaveModel model)
        {
            decimal sDate = (int)model.DateEnd.UnixToDateTime().Value.Subtract(model.DateStart.UnixToDateTime().Value).TotalDays;
            sDate += 1;
            if (model.DateStartIsHalf)
            {
                sDate = -0.5M;
            }
            if (model.DateEndIsHalf)
            {
                sDate = -0.5M;
            }
            if (sDate <= 0)
            {
                throw GeneralCode.InvalidParams.BadRequest();
            }

            var validation = cfg.Validations?.Where(v => v.TotalDays <= sDate)?.OrderByDescending(v => v.TotalDays)?.FirstOrDefault();
            if (validation != null)
            {
                if (validation.IsWarning)
                {
                    //TODO send notification
                }

                var daysFromCreateToStart = (int)DateTime.UtcNow.Subtract(model.DateStart.UnixToDateTime().Value).TotalDays;
                if (daysFromCreateToStart < validation.MinDaysFromCreateToStart)
                {
                    //TODO validation messsage
                    throw GeneralCode.InvalidParams.BadRequest();
                }
            }
        }

        public async Task<long> Create(LeaveModel model)
        {

            var info = _mapper.Map<LeaveLetter>(model);
            info.UserId = _currentContextService.UserId;


            var userInfo = (await _userHelperService.GetByIds(new[] { info.UserId ?? 0 })).FirstOrDefault();
            if (userInfo == null) throw GeneralCode.ItemNotFound.BadRequest();

            var cfg = await GetLeaveConfig(userInfo);

            Validate(cfg, model);

            info.LeaveConfigId = cfg.LeaveConfigId.Value;
            info.LeaveStatusId = (int)EnumLeaveStatus.New;
            await _organizationDBContext.Leave.AddAsync(info);
            await _organizationDBContext.SaveChangesAsync();


            await _leaveConfigActivityLog.LogBuilder(() => LeaveActivityLogMessage.Create)
                .MessageResourceFormatDatas(info.DateStart, userInfo.EmployeeCode + " " + userInfo.FullName + " - " + info.Title)
                .ObjectId(info.LeaveId)
                .JsonData(model.JsonSerialize())
                .CreateLog();

            return info.LeaveId;
        }

        public async Task<bool> Delete(long leaveId)
        {
            var info = await _organizationDBContext.Leave.FirstOrDefaultAsync(l => l.LeaveId == leaveId);
            if (info == null) throw GeneralCode.ItemNotFound.BadRequest();

            var userInfo = (await _userHelperService.GetByIds(new[] { info.UserId ?? 0 })).FirstOrDefault();
            if (userInfo == null) throw GeneralCode.ItemNotFound.BadRequest();

            info.IsDeleted = true;

            await _organizationDBContext.SaveChangesAsync();


            await _leaveConfigActivityLog.LogBuilder(() => LeaveActivityLogMessage.Create)
                .MessageResourceFormatDatas(info.DateStart, userInfo.EmployeeCode + " " + userInfo.FullName + " - " + info.Title)
                .ObjectId(info.LeaveId)
                .JsonData(info.JsonSerialize())
                .CreateLog();

            return true;
        }

        public async Task<PageData<LeaveModel>> Get(int? userId, int? roleUserId, string keyword, int? leaveConfigId, int? absenceTypeSymbolId, EnumLeaveStatus? leaveStatusId, long? fromDate, long? toDate, int page, int size, string sortBy, bool asc)
        {
            var query = _organizationDBContext.Leave.AsQueryable();

            if (roleUserId.HasValue)
            {
                var roleLeaveConfigIds = _organizationDBContext.LeaveConfigRole.Where(r => r.UserId == roleUserId.Value).Select(r => r.LeaveConfigId).Distinct();

                query = query.Where(l => roleLeaveConfigIds.Contains(l.LeaveConfigId));
            }

            if (leaveConfigId.HasValue)
            {
                query = query.Where(l => l.LeaveConfigId == leaveConfigId);
            }

            if (absenceTypeSymbolId.HasValue)
            {
                query = query.Where(l => l.AbsenceTypeSymbolId == absenceTypeSymbolId);
            }

            if (leaveStatusId.HasValue)
            {
                query = query.Where(l => l.LeaveStatusId == (int)leaveStatusId);
            }

            if (fromDate.HasValue)
            {
                var fDate = fromDate.Value.UnixToDateTime();
                query = query.Where(l => l.DateStart <= fDate);
            }
            if (toDate.HasValue)
            {
                var tDate = fromDate.Value.UnixToDateTime();
                query = query.Where(l => l.DateStart >= tDate);
            }

            if (userId.HasValue)
            {
                query = query.Where(l => l.UserId == userId);
            }
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(l => l.Title.Contains(keyword));
            }

            var total = await query.CountAsync();
            if (size > 0)
            {
                if (string.IsNullOrWhiteSpace(sortBy))
                {
                    sortBy = nameof(LeaveLetter.CreatedDatetimeUtc);
                }

                query = query.SortByFieldName(sortBy, asc);
                query = query.Skip((page - 1) * size).Take(size);
            }

            var lst = await query.ToListAsync();

            return (_mapper.Map<List<LeaveModel>>(lst), total);
        }

        public async Task<LeaveModel> Info(long leaveId)
        {
            var info = await _organizationDBContext.Leave.FirstOrDefaultAsync(l => l.LeaveId == leaveId);
            if (info == null) throw GeneralCode.ItemNotFound.BadRequest();

            return _mapper.Map<LeaveModel>(info);
        }

        public async Task<bool> Update(long leaveId, LeaveModel model)
        {
            var info = await _organizationDBContext.Leave.FirstOrDefaultAsync(l => l.LeaveId == leaveId);
            if (info == null) throw GeneralCode.ItemNotFound.BadRequest();

            var userInfo = (await _userHelperService.GetByIds(new[] { info.UserId ?? 0 })).FirstOrDefault();
            if (userInfo == null) throw GeneralCode.ItemNotFound.BadRequest();

            var cfg = await GetLeaveConfig(userInfo);

            Validate(cfg, model);

            info.LeaveConfigId = cfg.LeaveConfigId.Value;

            _mapper.Map(model, info);
            if (info.UserId != _currentContextService.UserId)
            {
                throw GeneralCode.InvalidParams.BadRequest();
            }

            info.LeaveStatusId = (int)EnumLeaveStatus.New;
            await _organizationDBContext.SaveChangesAsync();

            await _leaveConfigActivityLog.LogBuilder(() => LeaveActivityLogMessage.Update)
                .MessageResourceFormatDatas(info.DateStart, userInfo.EmployeeCode + " " + userInfo.FullName + " - " + info.Title)
                .ObjectId(info.LeaveId)
                .JsonData(model.JsonSerialize())
                .CreateLog();

            return true;
        }



        public async Task<bool> CheckAccept(long leaveId)
        {
            var info = await _organizationDBContext.Leave.FirstOrDefaultAsync(l => l.LeaveId == leaveId);
            if (info == null) throw GeneralCode.ItemNotFound.BadRequest();

            var userInfo = (await _userHelperService.GetByIds(new[] { info.UserId ?? 0 })).FirstOrDefault();
            if (userInfo == null) throw GeneralCode.ItemNotFound.BadRequest();

            if (info.LeaveStatusId != (int)EnumLeaveStatus.New || info.LeaveStatusId != (int)EnumLeaveStatus.CheckRejected)
            {
                throw GeneralCode.InvalidParams.BadRequest();

            }

            var cfg = await GetLeaveConfig(userInfo);
            if (cfg.Roles?.Any(r => r.LeaveRoleTypeId == EnumLeaveRoleType.Check && r.UserIds.Contains(_currentContextService.UserId)) != true)
            {
                //TODO Validation message
                throw GeneralCode.Forbidden.BadRequest();
            }
            info.LeaveConfigId = cfg.LeaveConfigId.Value;
            info.CheckedByUserId = _currentContextService.UserId;
            info.LeaveStatusId = (int)EnumLeaveStatus.CheckAccepted;

            await _organizationDBContext.SaveChangesAsync();

            await _leaveConfigActivityLog.LogBuilder(() => LeaveActivityLogMessage.Check)
                .MessageResourceFormatDatas(info.DateStart, userInfo.EmployeeCode + " " + userInfo.FullName + " - " + info.Title)
                .ObjectId(info.LeaveId)
                .JsonData(info.JsonSerialize())
                .CreateLog();

            return true;
        }

        public async Task<bool> CheckReject(long leaveId)
        {
            var info = await _organizationDBContext.Leave.FirstOrDefaultAsync(l => l.LeaveId == leaveId);
            if (info == null) throw GeneralCode.ItemNotFound.BadRequest();

            var userInfo = (await _userHelperService.GetByIds(new[] { info.UserId ?? 0 })).FirstOrDefault();
            if (userInfo == null) throw GeneralCode.ItemNotFound.BadRequest();

            if (info.LeaveStatusId != (int)EnumLeaveStatus.New)
            {
                throw GeneralCode.InvalidParams.BadRequest();

            }

            var cfg = await GetLeaveConfig(userInfo);
            if (cfg.Roles?.Any(r => r.LeaveRoleTypeId == EnumLeaveRoleType.Check && r.UserIds.Contains(_currentContextService.UserId)) != true)
            {
                //TODO Validation message
                throw GeneralCode.Forbidden.BadRequest();
            }
            info.LeaveConfigId = cfg.LeaveConfigId.Value;
            info.CheckedByUserId = _currentContextService.UserId;
            info.LeaveStatusId = (int)EnumLeaveStatus.CheckRejected;

            await _organizationDBContext.SaveChangesAsync();

            await _leaveConfigActivityLog.LogBuilder(() => LeaveActivityLogMessage.CheckReject)
                .MessageResourceFormatDatas(info.DateStart, userInfo.EmployeeCode + " " + userInfo.FullName + " - " + info.Title)
                .ObjectId(info.LeaveId)
                .JsonData(info.JsonSerialize())
                .CreateLog();

            return true;
        }

        public async Task<bool> CensorAccept(long leaveId)
        {
            var info = await _organizationDBContext.Leave.FirstOrDefaultAsync(l => l.LeaveId == leaveId);
            if (info == null) throw GeneralCode.ItemNotFound.BadRequest();

            var userInfo = (await _userHelperService.GetByIds(new[] { info.UserId ?? 0 })).FirstOrDefault();
            if (userInfo == null) throw GeneralCode.ItemNotFound.BadRequest();

            if (info.LeaveStatusId != (int)EnumLeaveStatus.CheckAccepted || info.LeaveStatusId != (int)EnumLeaveStatus.CensorRejected)
            {
                throw GeneralCode.InvalidParams.BadRequest();

            }

            var cfg = await GetLeaveConfig(userInfo);
            if (cfg.Roles?.Any(r => r.LeaveRoleTypeId == EnumLeaveRoleType.Censor && r.UserIds.Contains(_currentContextService.UserId)) != true)
            {
                //TODO Validation message
                throw GeneralCode.Forbidden.BadRequest();
            }
            info.LeaveConfigId = cfg.LeaveConfigId.Value;
            info.CheckedByUserId = _currentContextService.UserId;
            info.LeaveStatusId = (int)EnumLeaveStatus.CensorApproved;

            await _organizationDBContext.SaveChangesAsync();

            await _leaveConfigActivityLog.LogBuilder(() => LeaveActivityLogMessage.Approve)
                .MessageResourceFormatDatas(info.DateStart, userInfo.EmployeeCode + " " + userInfo.FullName + " - " + info.Title)
                .ObjectId(info.LeaveId)
                .JsonData(info.JsonSerialize())
                .CreateLog();

            return true;
        }

        public async Task<bool> CensorReject(long leaveId)
        {
            var info = await _organizationDBContext.Leave.FirstOrDefaultAsync(l => l.LeaveId == leaveId);
            if (info == null) throw GeneralCode.ItemNotFound.BadRequest();

            var userInfo = (await _userHelperService.GetByIds(new[] { info.UserId ?? 0 })).FirstOrDefault();
            if (userInfo == null) throw GeneralCode.ItemNotFound.BadRequest();

            if (info.LeaveStatusId != (int)EnumLeaveStatus.CheckAccepted)
            {
                throw GeneralCode.InvalidParams.BadRequest();

            }

            var cfg = await GetLeaveConfig(userInfo);
            if (cfg.Roles?.Any(r => r.LeaveRoleTypeId == EnumLeaveRoleType.Censor && r.UserIds.Contains(_currentContextService.UserId)) != true)
            {
                //TODO Validation message
                throw GeneralCode.Forbidden.BadRequest();
            }
            info.LeaveConfigId = cfg.LeaveConfigId.Value;
            info.CheckedByUserId = _currentContextService.UserId;
            info.LeaveStatusId = (int)EnumLeaveStatus.CensorRejected;

            await _organizationDBContext.SaveChangesAsync();

            await _leaveConfigActivityLog.LogBuilder(() => LeaveActivityLogMessage.Reject)
                .MessageResourceFormatDatas(info.DateStart, userInfo.EmployeeCode + " " + userInfo.FullName + " - " + info.Title)
                .ObjectId(info.LeaveId)
                .JsonData(info.JsonSerialize())
                .CreateLog();

            return true;
        }

    }
}
