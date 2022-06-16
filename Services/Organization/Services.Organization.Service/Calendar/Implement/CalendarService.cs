using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Verp.Resources.Organization.Calendar;
using VErp.Commons.Constants;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.Organization;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.OrganizationDB;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Organization.Model.Calendar;
using static Verp.Resources.Organization.Calendar.CalendarValidationMessage;
using CalendarEntity = VErp.Infrastructure.EF.OrganizationDB.Calendar;

namespace VErp.Services.Organization.Service.Calendar.Implement
{
    public class CalendarService : ICalendarService
    {
        private readonly OrganizationDBContext _organizationContext;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly IActivityLogService _activityLogService;
        private readonly IMapper _mapper;
        private readonly ICurrentContextService _currentContext;
        private readonly ObjectActivityLogFacade _calendarActivityLog;


        public CalendarService(OrganizationDBContext organizationContext
            , IOptions<AppSetting> appSetting
            , ILogger<CalendarService> logger
            , IActivityLogService activityLogService
            , IMapper mapper
            , ICurrentContextService currentContext
            )
        {
            _organizationContext = organizationContext;
            _appSetting = appSetting.Value;
            _logger = logger;
            _activityLogService = activityLogService;
            _mapper = mapper;
            _currentContext = currentContext;

            _calendarActivityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.Calendar);
        }


        public async Task<PageData<CalendarModel>> GetListCalendar(string keyword, int page, int size, Clause filter = null)
        {
            keyword = (keyword ?? "").Trim();
            var query = _organizationContext.Calendar.AsQueryable();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(c => c.CalendarCode.Contains(keyword) || c.CalendarName.Contains(keyword) || c.Guide.Contains(keyword) || c.Note.Contains(keyword));
            }
            query = query.InternalFilter(filter);
            if (size > 0)
            {
                query = query.Skip((page - 1) * size).Take(size);
            }
            var total = await query.CountAsync();
            var lst = await query.ProjectTo<CalendarModel>(_mapper.ConfigurationProvider).ToListAsync();
            return (lst, total);
        }
        public async Task<CalendarModel> AddCalendar(CalendarModel data)
        {
            if (string.IsNullOrEmpty(data.CalendarCode)) throw EmptyCalendarCode.BadRequest();

            var calendar = await _organizationContext.Calendar.FirstOrDefaultAsync(d => d.CalendarCode == data.CalendarCode || d.CalendarName == data.CalendarName);

            if (calendar != null)
            {
                if (string.Compare(calendar.CalendarCode, data.CalendarCode, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    throw CalendarNameAlreadyExists.BadRequest();
                }

                throw CalendarNameAlreadyExists.BadRequest();
            }

            calendar = _mapper.Map<CalendarEntity>(data);

            await _organizationContext.Calendar.AddAsync(calendar);
            await _organizationContext.SaveChangesAsync();

            data.CalendarId = calendar.CalendarId;

            await _calendarActivityLog.LogBuilder(() => CalendarActivityLogMessage.Create)
                .MessageResourceFormatDatas(calendar.CalendarCode)
                .ObjectId(calendar.CalendarId)
                .JsonData(data.JsonSerialize())
                .CreateLog();

            return data;
        }
        public async Task<CalendarModel> UpdateCalendar(int calendarId, CalendarModel data)
        {
            if (string.IsNullOrEmpty(data.CalendarCode)) throw new BadRequestException(GeneralCode.InvalidParams, "Mã lịch làm việc không được để trống");

            var calendar = await _organizationContext.Calendar.FirstOrDefaultAsync(d => d.CalendarId != calendarId && (d.CalendarCode == data.CalendarCode || d.CalendarName == data.CalendarName));

            if (calendar != null)
            {
                if (string.Compare(calendar.CalendarCode, data.CalendarCode, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    throw new BadRequestException(GeneralCode.InvalidParams, "Mã lịch làm việc đã tồn tại");
                }

                throw new BadRequestException(GeneralCode.InvalidParams, "Tên lịch làm việc đã tồn tại");
            }

            calendar = await _organizationContext.Calendar.FirstOrDefaultAsync(c => c.CalendarId == calendarId);
            if (calendar == null)
            {
                throw new BadRequestException(GeneralCode.InvalidParams, "Lịch làm việc không tồn tại");
            }
            data.CalendarId = calendarId;
            _mapper.Map(data, calendar);

            await _organizationContext.SaveChangesAsync();

            await _calendarActivityLog.LogBuilder(() => CalendarActivityLogMessage.Update)
                .MessageResourceFormatDatas(calendar.CalendarCode)
                .ObjectId(calendar.CalendarId)
                .JsonData(data.JsonSerialize())
                .CreateLog();

            return data;
        }
        public async Task<bool> DeleteCalendar(int calendarId)
        {
            using var trans = await _organizationContext.Database.BeginTransactionAsync();
            try
            {
                var calendar = await _organizationContext.Calendar.FirstOrDefaultAsync(c => c.CalendarId == calendarId);
                if (calendar == null)
                {
                    throw new BadRequestException(GeneralCode.InvalidParams, "Lịch làm việc không tồn tại");
                }

                if (_organizationContext.DepartmentCalendar.Any(dc => dc.CalendarId == calendarId))
                {
                    throw new BadRequestException(GeneralCode.InvalidParams, "Lịch làm việc đang có bộ phận sử dụng. Vui lòng kiểm tra lại");
                }

                // Lấy danh sách giờ làm / ngày
                var workingHourInfos = _organizationContext.WorkingHourInfo
                    .Where(wh => wh.CalendarId == calendarId)
                    .ToList();
                // Lấy danh sách ngày lv trong tuần
                var workingWeeks = _organizationContext.WorkingWeekInfo
                  .Where(ww => ww.CalendarId == calendarId)
                  .ToList();
                // Lấy danh sách nghỉ phép
                var dayOffs = _organizationContext.DayOffCalendar
                    .Where(dof => dof.CalendarId == calendarId)
                    .ToList();

                _organizationContext.WorkingHourInfo.RemoveRange(workingHourInfos);
                _organizationContext.WorkingWeekInfo.RemoveRange(workingWeeks);
                _organizationContext.DayOffCalendar.RemoveRange(dayOffs);
                _organizationContext.Calendar.Remove(calendar);
                _organizationContext.SaveChanges();

                trans.Commit();

                await _calendarActivityLog.LogBuilder(() => CalendarActivityLogMessage.Delete)
                    .MessageResourceFormatDatas(calendar.CalendarCode)
                    .ObjectId(calendar.CalendarId)
                    .JsonData(calendar.JsonSerialize())
                    .CreateLog();

                return true;
            }
            catch (Exception ex)
            {
                trans.Rollback();
                _logger.LogError(ex, "DeleteCalendar");
                throw;
            }
        }
        public string GetStringClone(string source, bool isCode, int suffix = 0)
        {
            string suffixText = suffix > 0 ? string.Format("({0})", suffix) : string.Empty;
            string des = string.Format("{0}_{1}{2}", source, "Copy", suffixText);
            var isDuplicate = isCode ? _organizationContext.Calendar.Any(c => c.CalendarCode == des) : _organizationContext.Calendar.Any(c => c.CalendarName == des);
            if (isDuplicate)
            {
                suffix++;
                des = GetStringClone(source, isCode, suffix);
            }
            return des;
        }

        public async Task<CalendarModel> CloneCalendar(int sourceCalendarId)
        {
            var sourceCalendar = await _organizationContext.Calendar.FirstOrDefaultAsync(c => c.CalendarId == sourceCalendarId);
            if (sourceCalendar == null)
            {
                throw CalendarDoesNotExists.BadRequest();
            }
            using var trans = await _organizationContext.Database.BeginTransactionAsync();
            try
            {
                var cloneCalendar = new CalendarEntity
                {
                    CalendarCode = GetStringClone(sourceCalendar.CalendarCode, true),
                    CalendarName = GetStringClone(sourceCalendar.CalendarName, false),
                    Guide = sourceCalendar.Guide,
                    Note = sourceCalendar.Note,
                    SubsidiaryId = sourceCalendar.SubsidiaryId
                };

                _organizationContext.Calendar.Add(cloneCalendar);
                _organizationContext.SaveChanges();

                // Clone giờ làm việc / ngày
                var sourceWorkingHourInfos = await _organizationContext.WorkingHourInfo
                    .Where(wh => wh.CalendarId == sourceCalendarId)
                    .ToListAsync();
                foreach (var sourceWorkingHourInfo in sourceWorkingHourInfos)
                {
                    var cloneWorkingHourInfo = new WorkingHourInfo
                    {
                        CalendarId = cloneCalendar.CalendarId,
                        StartDate = sourceWorkingHourInfo.StartDate,
                        SubsidiaryId = sourceWorkingHourInfo.SubsidiaryId,
                        WorkingHourPerDay = sourceWorkingHourInfo.WorkingHourPerDay
                    };
                    _organizationContext.WorkingHourInfo.Add(cloneWorkingHourInfo);
                }

                // Clone lịch lv tuần
                var sourceWorkingWeeks = await _organizationContext.WorkingWeekInfo
                    .Where(ww => ww.CalendarId == sourceCalendarId)
                    .ToListAsync();
                foreach (var sourceWorkingWeek in sourceWorkingWeeks)
                {
                    var cloneWorkingWeekInfo = new WorkingWeekInfo
                    {
                        CalendarId = cloneCalendar.CalendarId,
                        StartDate = sourceWorkingWeek.StartDate,
                        SubsidiaryId = sourceWorkingWeek.SubsidiaryId,
                        DayOfWeek = sourceWorkingWeek.DayOfWeek,
                        IsDayOff = sourceWorkingWeek.IsDayOff
                    };
                    _organizationContext.WorkingWeekInfo.Add(cloneWorkingWeekInfo);
                }

                // Clone ngày nghỉ
                var sourceDayOffs = await _organizationContext.DayOffCalendar
                    .Where(dof => dof.CalendarId == sourceCalendarId)
                    .ToListAsync();
                foreach (var sourceDayOff in sourceDayOffs)
                {
                    var cloneDayOffCalendar = new DayOffCalendar
                    {
                        CalendarId = cloneCalendar.CalendarId,
                        SubsidiaryId = sourceDayOff.SubsidiaryId,
                        Content = sourceDayOff.Content,
                        Day = sourceDayOff.Day
                    };
                    _organizationContext.DayOffCalendar.Add(cloneDayOffCalendar);
                }
                _organizationContext.SaveChanges();
                trans.Commit();
                var cloneCalendarModel = _mapper.Map<CalendarModel>(cloneCalendar);

                await _calendarActivityLog.LogBuilder(() => CalendarActivityLogMessage.Create)
                 .MessageResourceFormatDatas(cloneCalendar.CalendarCode, sourceCalendar.CalendarCode)
                 .ObjectId(cloneCalendar.CalendarId)
                 .JsonData(cloneCalendarModel.JsonSerialize())
                 .CreateLog();


                return cloneCalendarModel;
            }
            catch (Exception ex)
            {
                trans.TryRollbackTransaction();
                _logger.LogError(ex, "CloneCalendar");
                throw;
            }
        }

        public async Task<WeekCalendarModel> GetCurrentCalendar(int calendarId)
        {
            var now = DateTime.UtcNow.Date;
            var workingHourInfo = await _organizationContext.WorkingHourInfo
                .Where(wh => wh.StartDate <= now && wh.CalendarId == calendarId)
                .OrderByDescending(wh => wh.StartDate)
                .FirstOrDefaultAsync();

            var workingWeeks = await _organizationContext.WorkingWeekInfo
                .Where(ww => ww.StartDate <= now && ww.CalendarId == calendarId)
                .GroupBy(ww => ww.DayOfWeek)
                .Select(g => new
                {
                    DayOfWeek = g.Key,
                    StartDate = g.Max(ww => ww.StartDate)
                })
                .Join(_organizationContext.WorkingWeekInfo, w => new { w.StartDate, w.DayOfWeek, CalendarId = calendarId }, ww => new { ww.StartDate, ww.DayOfWeek, ww.CalendarId }, (w, ww) => ww)
                .ProjectTo<WorkingWeekInfoModel>(_mapper.ConfigurationProvider).ToListAsync();

            foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
            {
                if (!workingWeeks.Any(d => d.DayOfWeek == day))
                {
                    workingWeeks.Add(new WorkingWeekInfoModel
                    {
                        DayOfWeek = day,
                        IsDayOff = false
                    });
                }
            }

            WeekCalendarModel result = new WeekCalendarModel
            {
                WorkingHourPerDay = workingHourInfo?.WorkingHourPerDay ?? OrganizationConstants.WORKING_HOUR_PER_DAY,
                WorkingWeek = workingWeeks
            };

            return result;
        }
        public async Task<IList<WeekCalendarModel>> GetCalendar(int calendarId)
        {

            var workingHourInfos = await _organizationContext.WorkingHourInfo.Where(wh => wh.CalendarId == calendarId)
                .ToListAsync();

            var allWorkingWeeks = await _organizationContext.WorkingWeekInfo.Where(ww => ww.CalendarId == calendarId)
                .ToListAsync();

            var timePoints = workingHourInfos.Select(wh => wh.StartDate).Union(allWorkingWeeks.Select(ww => ww.StartDate)).Distinct().OrderBy(tp => tp).ToList();

            // Thêm thông tin mặc định
            var defaultWorkingWeeks = new List<WorkingWeekInfoModel>();
            foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
            {
                defaultWorkingWeeks.Add(new WorkingWeekInfoModel
                {
                    DayOfWeek = day,
                    IsDayOff = false
                });
            }
            var result = new List<WeekCalendarModel>()
            {
                new WeekCalendarModel
                {
                    StartDate = DateTime.MinValue.GetUnix(),
                    WorkingHourPerDay = OrganizationConstants.WORKING_HOUR_PER_DAY,
                    WorkingWeek = defaultWorkingWeeks
                }
            };

            foreach (var timePoint in timePoints)
            {
                var workingHourInfo = workingHourInfos
                    .Where(wh => wh.StartDate <= timePoint)
                    .OrderByDescending(wh => wh.StartDate)
                    .FirstOrDefault();

                var workingWeeks = allWorkingWeeks
                    .Where(ww => ww.StartDate <= timePoint)
                    .GroupBy(ww => ww.DayOfWeek)
                    .Select(g => new
                    {
                        DayOfWeek = g.Key,
                        StartDate = g.Max(ww => ww.StartDate)
                    })
                    .Join(allWorkingWeeks, w => new { w.StartDate, w.DayOfWeek, CalendarId = calendarId }, ww => new { ww.StartDate, ww.DayOfWeek, ww.CalendarId }, (w, ww) => ww)
                    .AsQueryable()
                    .ProjectTo<WorkingWeekInfoModel>(_mapper.ConfigurationProvider)
                    .ToList();

                foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
                {
                    if (!workingWeeks.Any(d => d.DayOfWeek == day))
                    {
                        workingWeeks.Add(new WorkingWeekInfoModel
                        {
                            DayOfWeek = day,
                            IsDayOff = false
                        });
                    }
                }

                result.Add(new WeekCalendarModel
                {
                    StartDate = timePoint.GetUnix(),
                    WorkingHourPerDay = workingHourInfo?.WorkingHourPerDay ?? OrganizationConstants.WORKING_HOUR_PER_DAY,
                    WorkingWeek = workingWeeks
                });
            }

            return result;
        }



        public async Task<IList<DayOffCalendarModel>> GetDayOffCalendar(int calendarId, long startDate, long endDate)
        {
            var start = startDate.UnixToDateTime().Value;
            var end = endDate.UnixToDateTime().Value;

            var dayOffCalendar = await _organizationContext.DayOffCalendar
                .Where(dof => dof.Day >= start && dof.Day <= end && dof.CalendarId == calendarId)
                .ToListAsync();

            // Lấy thông tin ngày làm việc trong tuần từ ngày bắt đầu
            var workingWeeks = await _organizationContext.WorkingWeekInfo
               .Where(ww => ww.StartDate <= start && ww.CalendarId == calendarId)
               .GroupBy(ww => ww.DayOfWeek)
               .Select(g => new
               {
                   DayOfWeek = g.Key,
                   StartDate = g.Max(ww => ww.StartDate)
               })
               .Join(_organizationContext.WorkingWeekInfo, w => new { w.StartDate, w.DayOfWeek, CalendarId = calendarId }, ww => new { ww.StartDate, ww.DayOfWeek, ww.CalendarId }, (w, ww) => ww)
               .ToListAsync();

            foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
            {
                if (!workingWeeks.Any(d => d.DayOfWeek == (int)day))
                {
                    workingWeeks.Add(new WorkingWeekInfo
                    {
                        DayOfWeek = (int)day,
                        IsDayOff = false,
                        StartDate = start
                    });
                }
            }

            // Lấy thông tin thay đổi trong khoảng thời gian
            var changeWorkingWeeks = await _organizationContext.WorkingWeekInfo
                .Where(ww => ww.StartDate > start && ww.StartDate <= end && ww.CalendarId == calendarId)
                .ToListAsync();
            var lstDayOff = new List<DayOffCalendarModel>();

            for (var day = start; day <= end; day = day.AddDays(1))
            {
                var clientDay = day.AddMinutes(-_currentContext.TimeZoneOffset.GetValueOrDefault());
                if (dayOffCalendar.Any(dof => dof.Day == day)) continue;
                var workingWeek = changeWorkingWeeks.Where(w => w.DayOfWeek == (int)clientDay.DayOfWeek && w.StartDate <= day).OrderByDescending(w => w.StartDate).FirstOrDefault();
                if (workingWeek == null) workingWeek = workingWeeks.Where(w => w.DayOfWeek == (int)clientDay.DayOfWeek).FirstOrDefault();
                if (workingWeek?.IsDayOff ?? false)
                {
                    lstDayOff.Add(new DayOffCalendarModel
                    {
                        Day = day.GetUnix(),
                        Content = CalendarTitle.OffDayOfWeek,
                        DayOffType = EnumDayOffType.Weekend
                    });
                }
            }

            foreach (var dayOff in dayOffCalendar)
            {
                var model = _mapper.Map<DayOffCalendarModel>(dayOff);
                model.DayOffType = EnumDayOffType.DayOff;
                lstDayOff.Add(model);
            }

            return lstDayOff.OrderBy(d => d.Day).ToList();
        }


        public async Task<DayOffCalendarModel> UpdateDayOff(int calendarId, DayOffCalendarModel data)
        {
            try
            {
                var calendar = _organizationContext.Calendar.FirstOrDefault(c => c.CalendarId == calendarId);
                if (calendar == null) throw CalendarDoesNotExists.BadRequest();
                var dayOff = await _organizationContext.DayOffCalendar
                .FirstOrDefaultAsync(dof => dof.Day == data.Day.UnixToDateTime() && dof.CalendarId == calendarId);
                if (dayOff == null)
                {
                    dayOff = _mapper.Map<DayOffCalendar>(data);
                    dayOff.CalendarId = calendarId;
                    _organizationContext.DayOffCalendar.Add(dayOff);
                }
                else if (dayOff.Content != data.Content)
                {
                    dayOff.Content = data.Content;
                }
                _organizationContext.SaveChanges();


                await _calendarActivityLog.LogBuilder(() => CalendarActivityLogMessage.UpdateDayOff)
                 .MessageResourceFormatDatas(data.Day.UnixToDateTime(), data.Content, calendar.CalendarCode)
                 .ObjectId(calendar.CalendarId)
                 .JsonData(data.JsonSerialize())
                 .CreateLog();

                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateDayOffCalendar");
                throw;
            }
        }

        public async Task<bool> DeleteDayOff(int calendarId, long day)
        {
            try
            {
                var time = day.UnixToDateTime().Value;
                var calendar = _organizationContext.Calendar.FirstOrDefault(c => c.CalendarId == calendarId);
                if (calendar == null) throw CalendarDoesNotExists.BadRequest();
                var dayOff = await _organizationContext.DayOffCalendar.FirstOrDefaultAsync(dof => dof.CalendarId == calendarId && dof.Day == time);
                if (dayOff == null) throw DayOffDoesNotExist.BadRequest();
                _organizationContext.DayOffCalendar.Remove(dayOff);
                _organizationContext.SaveChanges();

                await _calendarActivityLog.LogBuilder(() => CalendarActivityLogMessage.DeleteDayOff)
                .MessageResourceFormatDatas(time, dayOff.Content, calendar.CalendarCode)
                .ObjectId(calendar.CalendarId)
                .JsonData(dayOff.JsonSerialize())
                .CreateLog();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeleteDayOffCalendar");
                throw;
            }
        }

        public async Task<bool> DeleteWeekCalendar(int calendarId, long startDate)
        {
            using var trans = await _organizationContext.Database.BeginTransactionAsync();
            try
            {
                var calendar = _organizationContext.Calendar.FirstOrDefault(c => c.CalendarId == calendarId);
                if (calendar == null) throw CalendarDoesNotExists.BadRequest();

                DateTime time = startDate.UnixToDateTime().Value;
                var workingHourInfo = await _organizationContext.WorkingHourInfo.FirstOrDefaultAsync(wh => wh.CalendarId == calendarId && wh.StartDate == time);

                if (workingHourInfo != null)
                {
                    _organizationContext.WorkingHourInfo.Remove(workingHourInfo);
                }

                var workingWeeks = await _organizationContext.WorkingWeekInfo
                  .Where(ww => ww.CalendarId == calendarId && ww.StartDate == time)
                  .ToListAsync();

                if (workingWeeks.Count > 0)
                {
                    _organizationContext.WorkingWeekInfo.RemoveRange(workingWeeks);
                }

                await _organizationContext.SaveChangesAsync();
                trans.Commit();

                await _calendarActivityLog.LogBuilder(() => CalendarActivityLogMessage.WeekCalendarDelete)
                 .MessageResourceFormatDatas(time, calendar.CalendarCode)
                 .ObjectId(calendar.CalendarId)
                 .JsonData(workingWeeks.JsonSerialize())
                 .CreateLog();

                return true;
            }
            catch (Exception ex)
            {
                trans.TryRollbackTransaction();
                _logger.LogError(ex, "DeleteCalendar");
                throw;
            }
        }


        public async Task<WeekCalendarModel> CreateWeekCalendar(int calendarId, WeekCalendarModel data)
        {
            using var trans = await _organizationContext.Database.BeginTransactionAsync();
            try
            {

                var calendar = _organizationContext.Calendar.FirstOrDefault(c => c.CalendarId == calendarId);
                if (calendar == null) throw CalendarDoesNotExists.BadRequest();

                DateTime time = data.StartDate.HasValue ? data.StartDate.UnixToDateTime().Value : DateTime.UtcNow.Date;

                if (_organizationContext.WorkingHourInfo.Any(wh => wh.CalendarId == calendarId && wh.StartDate == time)
                    || _organizationContext.WorkingWeekInfo.Any(ww => ww.CalendarId == calendarId && ww.StartDate == time))
                {
                    throw CalendarStartDateAlreadyExists.BadRequestFormat(time.AddMinutes(-_currentContext.TimeZoneOffset.GetValueOrDefault()));
                }

                foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
                {
                    var newWorkingWeek = data.WorkingWeek.FirstOrDefault(w => w.DayOfWeek == day);
                    if (newWorkingWeek == null) throw MissingDaysOfWeek.BadRequest();
                }

                // Update workingHour per day
                var currentWorkingHourInfo = new WorkingHourInfo
                {
                    StartDate = time,
                    WorkingHourPerDay = data.WorkingHourPerDay,
                    CalendarId = calendarId
                };
                _organizationContext.WorkingHourInfo.Add(currentWorkingHourInfo);


                // Update working week
                foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
                {
                    var newWorkingWeek = data.WorkingWeek.FirstOrDefault(w => w.DayOfWeek == day);
                    var currentWorkingWeek = new WorkingWeekInfo
                    {
                        StartDate = time,
                        DayOfWeek = (int)day,
                        IsDayOff = newWorkingWeek.IsDayOff,
                        CalendarId = calendarId
                    };
                    _organizationContext.WorkingWeekInfo.Add(currentWorkingWeek);
                }

                _organizationContext.SaveChanges();
                trans.Commit();


                await _calendarActivityLog.LogBuilder(() => CalendarActivityLogMessage.WeekCalendarCreate)
                  .MessageResourceFormatDatas(time, calendar.CalendarCode)
                  .ObjectId(calendar.CalendarId)
                  .JsonData(data.JsonSerialize())
                  .CreateLog();


                return await GetCurrentCalendar(calendarId);
            }
            catch (Exception ex)
            {
                trans.TryRollbackTransaction();
                _logger.LogError(ex, "CreateCalendar");
                throw;
            }
        }

        public async Task<WeekCalendarModel> UpdateWeekCalendar(int calendarId, long oldDate, WeekCalendarModel data)
        {
            using var trans = await _organizationContext.Database.BeginTransactionAsync();
            try
            {
                var calendar = _organizationContext.Calendar.FirstOrDefault(c => c.CalendarId == calendarId);
                if (calendar == null) throw CalendarDoesNotExists.BadRequest();

                if (!data.StartDate.HasValue) throw MissingStartDate.BadRequest();
                DateTime oldTime = oldDate.UnixToDateTime().Value;
                DateTime time = data.StartDate.UnixToDateTime().Value;

                if (time != oldTime)
                {
                    if (_organizationContext.WorkingHourInfo.Any(wh => wh.CalendarId == calendarId && wh.StartDate == time)
                        || _organizationContext.WorkingWeekInfo.Any(ww => ww.CalendarId == calendarId && ww.StartDate == time))
                    {
                        throw CalendarStartDateAlreadyExists.BadRequestFormat(time.AddMinutes(-_currentContext.TimeZoneOffset.GetValueOrDefault()));
                    }
                }

                var currentWorkingHourInfo = await _organizationContext.WorkingHourInfo
                  .Where(wh => wh.CalendarId == calendarId && wh.StartDate == oldTime)
                  .FirstOrDefaultAsync();

                var currentWorkingWeeks = await _organizationContext.WorkingWeekInfo
                   .Where(ww => ww.CalendarId == calendarId && ww.StartDate == oldTime)
                   .ToListAsync();

                if (currentWorkingHourInfo == null || currentWorkingWeeks.Count == 0)
                {
                    throw CalendarStartDateInvalid.BadRequestFormat(oldTime.AddMinutes(-_currentContext.TimeZoneOffset.GetValueOrDefault()));
                }

                foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
                {
                    var newWorkingWeek = data.WorkingWeek.FirstOrDefault(w => w.DayOfWeek == day);
                    if (newWorkingWeek == null) throw MissingDaysOfWeek.BadRequest();
                }

                // Gỡ thông tin cũ
                _organizationContext.WorkingHourInfo.Remove(currentWorkingHourInfo);
                _organizationContext.WorkingWeekInfo.RemoveRange(currentWorkingWeeks);
                _organizationContext.SaveChanges();

                // Update workingHour per day
                var newWorkingHourInfo = new WorkingHourInfo
                {
                    StartDate = time,
                    WorkingHourPerDay = data.WorkingHourPerDay,
                    CalendarId = calendarId
                };
                _organizationContext.WorkingHourInfo.Add(newWorkingHourInfo);
                // Update working week
                foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
                {
                    var newWorkingWeek = data.WorkingWeek.FirstOrDefault(w => w.DayOfWeek == day);
                    var currentWorkingWeek = new WorkingWeekInfo
                    {
                        StartDate = time,
                        DayOfWeek = (int)day,
                        IsDayOff = newWorkingWeek.IsDayOff,
                        CalendarId = calendarId
                    };
                    _organizationContext.WorkingWeekInfo.Add(currentWorkingWeek);
                }

                _organizationContext.SaveChanges();
                trans.Commit();

                await _calendarActivityLog.LogBuilder(() => CalendarActivityLogMessage.WeekCalendarUpdate)
                   .MessageResourceFormatDatas(time, calendar.CalendarCode)
                   .ObjectId(calendar.CalendarId)
                   .JsonData(data.JsonSerialize())
                   .CreateLog();

                return await GetCurrentCalendar(calendarId);
            }
            catch (Exception ex)
            {
                trans.TryRollbackTransaction();
                _logger.LogError(ex, "UpdateCalendar");
                throw;
            }
        }
    }
}
