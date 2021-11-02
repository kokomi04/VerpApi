using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Verp.Resources.Organization.Department;
using VErp.Commons.Constants;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.OrganizationDB;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Organization.Model.Calendar;
using VErp.Services.Organization.Model.DepartmentCalendar;
using DepartmentCalendarEntity = VErp.Infrastructure.EF.OrganizationDB.DepartmentCalendar;
using static Verp.Resources.Organization.Department.DepartmentCalendarValidationMessage;
using Verp.Resources.Organization.Calendar;

namespace VErp.Services.Organization.Service.DepartmentCalendar.Implement
{
    public class DepartmentCalendarService : IDepartmentCalendarService
    {
        private readonly OrganizationDBContext _organizationContext;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly ICurrentContextService _currentContext;
        private readonly ObjectActivityLogFacade _departmentActivityLog;


        public DepartmentCalendarService(OrganizationDBContext organizationContext
            , IOptions<AppSetting> appSetting
            , ILogger<DepartmentCalendarService> logger
            , IActivityLogService activityLogService
            , IMapper mapper
            , ICurrentContextService currentContext
            )
        {
            _organizationContext = organizationContext;
            _appSetting = appSetting.Value;
            _logger = logger;
            _mapper = mapper;
            _currentContext = currentContext;
            _departmentActivityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.Department);
        }
        public async Task<IList<DepartmentCalendarModel>> GetDepartmentCalendars(int departmentId)
        {
            var lstDepartmentCalendar = await (from dc in _organizationContext.DepartmentCalendar
                                               join c in _organizationContext.Calendar on dc.CalendarId equals c.CalendarId
                                               where dc.DepartmentId == departmentId
                                               orderby dc.StartDate descending
                                               select new DepartmentCalendarModel
                                               {
                                                   CalendarCode = c.CalendarCode,
                                                   CalendarId = c.CalendarId,
                                                   CalendarName = c.CalendarName,
                                                   StartDate = dc.StartDate.GetUnix()
                                               })
                                               .ToListAsync();
            return lstDepartmentCalendar;
        }

        public async Task<DepartmentCalendarModel> CreateDepartmentCalendar(int departmentId, DepartmentCalendarModel data)
        {
            using var trans = await _organizationContext.Database.BeginTransactionAsync();
            try
            {
                var department = _organizationContext.Department.FirstOrDefault(d => d.DepartmentId == departmentId);
                if (department == null) throw DepartmentNotFound.BadRequest();

                var calendar = _organizationContext.Calendar.FirstOrDefault(c => c.CalendarId == data.CalendarId);
                if (calendar == null) throw CalendarDoesNotExists.BadRequest();

                DateTime time = data.StartDate.HasValue ? data.StartDate.UnixToDateTime().Value : DateTime.UtcNow.Date;

                if (_organizationContext.DepartmentCalendar.Any(dc => dc.DepartmentId == departmentId && dc.StartDate == time))
                {
                    throw CalendarWithStartDateAlreadyExists.BadRequestFormat(department.DepartmentName, time.AddMinutes(-_currentContext.TimeZoneOffset.GetValueOrDefault()));
                }

                // Update Calendar
                var deparmentCalendar = new DepartmentCalendarEntity
                {
                    StartDate = time,
                    CalendarId = data.CalendarId,
                    DepartmentId = departmentId
                };
                _organizationContext.DepartmentCalendar.Add(deparmentCalendar);

                _organizationContext.SaveChanges();
                trans.Commit();

                await _departmentActivityLog.LogBuilder(() => DepartmentActivityLogMessage.DepartmentCalendarCreate)
                .MessageResourceFormatDatas(calendar.CalendarCode, time, department.DepartmentCode)
                .ObjectId(department.DepartmentId)
                .JsonData(data.JsonSerialize())
                .CreateLog();

                return data;
            }
            catch (Exception ex)
            {
                trans.TryRollbackTransaction();
                _logger.LogError(ex, "CreateDepartmentCalendar");
                throw;
            }
        }

        public async Task<DepartmentCalendarModel> UpdateDepartmentCalendar(int departmentId, long oldDate, DepartmentCalendarModel data)
        {
            using var trans = await _organizationContext.Database.BeginTransactionAsync();
            try
            {
                var department = _organizationContext.Department.FirstOrDefault(d => d.DepartmentId == departmentId);
                if (department == null) throw DepartmentNotFound.BadRequest();

                var calendar = _organizationContext.Calendar.FirstOrDefault(c => c.CalendarId == data.CalendarId);
                if (calendar == null) throw CalendarDoesNotExists.BadRequest();

                if (!data.StartDate.HasValue) throw StartDateMustHaveValue.BadRequest();

                DateTime oldTime = oldDate.UnixToDateTime().Value;
                DateTime time = data.StartDate.UnixToDateTime().Value;

                if (time != oldTime)
                {
                    if (_organizationContext.DepartmentCalendar.Any(dc => dc.DepartmentId == departmentId && dc.StartDate == time))
                    {
                        throw CalendarWithStartDateAlreadyExists.BadRequestFormat(department.DepartmentName, time.AddMinutes(-_currentContext.TimeZoneOffset.GetValueOrDefault()));
                    }
                }

                var currentDepartmentCalendar = await _organizationContext.DepartmentCalendar
                  .Where(dc => dc.DepartmentId == departmentId && dc.StartDate == oldTime)
                  .FirstOrDefaultAsync();

                if (currentDepartmentCalendar == null)
                {
                    throw CalendarWithStartDateDoesNotExists.BadRequestFormat(department.DepartmentName, oldTime.AddMinutes(-_currentContext.TimeZoneOffset.GetValueOrDefault()));
                }

                // Gỡ thông tin cũ
                _organizationContext.DepartmentCalendar.Remove(currentDepartmentCalendar);
                _organizationContext.SaveChanges();

                // Update lịch mới
                var newDepartmentCalendar = new DepartmentCalendarEntity
                {
                    StartDate = time,
                    DepartmentId = departmentId,
                    CalendarId = data.CalendarId
                };
                _organizationContext.DepartmentCalendar.Add(newDepartmentCalendar);
                _organizationContext.SaveChanges();
                trans.Commit();

                await _departmentActivityLog.LogBuilder(() => DepartmentActivityLogMessage.DepartmentCalendarUpdate)
                   .MessageResourceFormatDatas(calendar.CalendarCode, time, department.DepartmentCode)
                   .ObjectId(department.DepartmentId)
                   .JsonData(data.JsonSerialize())
                   .CreateLog();
                return data;
            }
            catch (Exception ex)
            {
                trans.TryRollbackTransaction();
                _logger.LogError(ex, "UpdateDepartmentCalendar");
                throw;
            }

        }
        public async Task<bool> DeleteDepartmentCalendar(int departmentId, long startDate)
        {
            using var trans = await _organizationContext.Database.BeginTransactionAsync();
            try
            {
                var department = _organizationContext.Department.FirstOrDefault(d => d.DepartmentId == departmentId);
                if (department == null) throw DepartmentNotFound.BadRequest();



                DateTime time = startDate.UnixToDateTime().Value;
                var departmentCalendar = await _organizationContext.DepartmentCalendar.FirstOrDefaultAsync(dc => dc.DepartmentId == departmentId && dc.StartDate == time);

                if (departmentCalendar != null)
                {
                    _organizationContext.DepartmentCalendar.Remove(departmentCalendar);
                }
                else
                {
                    throw CalendarWithStartDateDoesNotExists.BadRequestFormat(department.DepartmentName, time.AddMinutes(-_currentContext.TimeZoneOffset.GetValueOrDefault()));

                }

                var calendar = _organizationContext.Calendar.FirstOrDefault(c => c.CalendarId == departmentCalendar.CalendarId);

                await _organizationContext.SaveChangesAsync();
                trans.Commit();

                await _departmentActivityLog.LogBuilder(() => DepartmentActivityLogMessage.DepartmentCalendarDelete)
                 .MessageResourceFormatDatas(calendar?.CalendarCode, time, department.DepartmentCode)
                 .ObjectId(department.DepartmentId)
                 .JsonData(departmentCalendar.JsonSerialize())
                 .CreateLog();
                return true;
            }
            catch (Exception ex)
            {
                trans.TryRollbackTransaction();
                _logger.LogError(ex, "DeleteDepartmentCalendar");
                throw;
            }
        }

        public async Task<IList<DepartmentCalendarListModel>> GetListDepartmentCalendar(int[] departmentIds, long startDate, long endDate)
        {
            var start = startDate.UnixToDateTime().Value;
            var end = endDate.UnixToDateTime().Value;

            // Lấy thông tin lịch làm việc ban đầu
            var allDepartmentCalendars = _organizationContext.DepartmentCalendar
                .Where(dc => departmentIds.Contains(dc.DepartmentId) && dc.StartDate <= start)
                .ToList()
                .GroupBy(dc => new { dc.DepartmentId, dc.CalendarId })
                .Select(g => new
                {
                    CalendarId = g.Key.CalendarId,
                    DepartmentId = g.Key.DepartmentId,
                    StartDate = g.Max(wh => wh.StartDate)
                })
                .Join(_organizationContext.DepartmentCalendar, gdc => new { gdc.StartDate, gdc.DepartmentId, gdc.CalendarId }, dc => new { dc.StartDate, dc.DepartmentId, dc.CalendarId }, (gdc, dc) => dc)
                .ToList();

            // Lấy thông tin lịch sử đổi lịch làm việc trong khoảng thời gian
            var changeAllDepartmentCalendars = _organizationContext.DepartmentCalendar.Where(dc => departmentIds.Contains(dc.DepartmentId) && dc.StartDate <= end).OrderBy(dc => dc.StartDate).ToList();

            // Lấy danh sách lịch làm việc
            var calendarIds = allDepartmentCalendars.Select(dc => dc.CalendarId).Concat(changeAllDepartmentCalendars.Select(dc => dc.CalendarId)).Distinct().ToList();

            // Thông tin giờ làm việc / ngày ban đầu
            var departmentWorkingHourInfo = await _organizationContext.WorkingHourInfo
                .Where(wh => wh.StartDate <= start && calendarIds.Contains(wh.CalendarId))
                .GroupBy(wh => wh.CalendarId)
                .Select(g => new
                {
                    CalendarId = g.Key,
                    StartDate = g.Max(wh => wh.StartDate)
                })
                .Join(_organizationContext.WorkingHourInfo, w => new { w.StartDate, w.CalendarId }, wh => new { wh.StartDate, wh.CalendarId }, (w, wh) => wh)
                .ToListAsync();

            // Lấy thông tin giờ làm việc / ngày thay đổi trong khoảng thời gian
            var departmentChangeWorkingHours = await _organizationContext.WorkingHourInfo
                .Where(wh => wh.StartDate > start && wh.StartDate <= end && calendarIds.Contains(wh.CalendarId))
                .OrderBy(wh => wh.StartDate)
                .ToListAsync();

            // Lấy thông tin ngày làm việc trong tuần từ ngày bắt đầu
            var departmentWorkingWeeks = await _organizationContext.WorkingWeekInfo
              .Where(ww => ww.StartDate <= start && calendarIds.Contains(ww.CalendarId))
              .GroupBy(ww => new { ww.DayOfWeek, ww.CalendarId })
              .Select(g => new
              {
                  g.Key.DayOfWeek,
                  g.Key.CalendarId,
                  StartDate = g.Max(ww => ww.StartDate)
              })
              .Join(_organizationContext.WorkingWeekInfo, w => new { w.StartDate, w.DayOfWeek, w.CalendarId }, ww => new { ww.StartDate, ww.DayOfWeek, ww.CalendarId }, (w, ww) => ww)
              .ToListAsync();

            // Lấy thông tin ngày làm việc trong tuần thay đổi trong khoảng thời gian
            var departmentChangeWorkingWeeks = await _organizationContext.WorkingWeekInfo
                .Where(ww => ww.StartDate > start && ww.StartDate <= end && calendarIds.Contains(ww.CalendarId))
                .ToListAsync();

            // Thông tin ngày nghỉ
            var departmentDayOffCalendars = await _organizationContext.DayOffCalendar
               .Where(dof => dof.Day >= start && dof.Day <= end && calendarIds.Contains(dof.CalendarId))
               .ToListAsync();

            // Lấy thông tin làm thêm giờ
            var deparmentOverHourInfos = await _organizationContext.DepartmentOverHourInfo
               .Where(oh => departmentIds.Contains(oh.DepartmentId))
               .ProjectTo<DepartmentOverHourInfoModel>(_mapper.ConfigurationProvider)
               .ToListAsync();

            // Lấy thông tin nhân sự tăng cường
            var departmentIncreaseInfos = await _organizationContext.DepartmentIncreaseInfo
               .Where(oh => departmentIds.Contains(oh.DepartmentId))
               .ProjectTo<DepartmentIncreaseInfoModel>(_mapper.ConfigurationProvider)
               .ToListAsync();

            var result = new List<DepartmentCalendarListModel>();
            foreach (var departmentId in departmentIds)
            {
                var defaultWorkingHour = new WorkingHourInfoModel
                {
                    StartDate = DateTime.MinValue.GetUnix(),
                    WorkingHourPerDay = OrganizationConstants.WORKING_HOUR_PER_DAY
                };

                // Lấy thông tin lịch làm việc ban đầu
                var departmentCalendar = allDepartmentCalendars.FirstOrDefault(dc => dc.DepartmentId == departmentId);
                // Lấy thông tin thay đổi lịch
                var changeDepartmentCalendars = changeAllDepartmentCalendars.Where(dc => dc.DepartmentId == departmentId).ToList();

                // Danh sách thông tin số giờ làm / ngày
                var workingHourInfos = new List<WorkingHourInfoModel>();
                // Danh sách thông tin này nghỉ
                var lstDayOff = new List<DayOffCalendarModel>();

                if (departmentCalendar == null)
                {
                    workingHourInfos.Add(defaultWorkingHour);
                }
                else
                {
                    var workingHourInfo = departmentWorkingHourInfo.FirstOrDefault(wh => wh.CalendarId == departmentCalendar.CalendarId);
                    if (workingHourInfo == null)
                    {
                        workingHourInfos.Add(defaultWorkingHour);
                    }
                    else
                    {
                        workingHourInfos.Add(new WorkingHourInfoModel
                        {
                            StartDate = workingHourInfo.StartDate.GetUnix(),
                            WorkingHourPerDay = workingHourInfo.WorkingHourPerDay
                        });
                    }
                }

                // Duyệt thay đổi
                if (changeDepartmentCalendars.Count == 0 && departmentCalendar != null)
                {
                    // Thêm thông tin giờ làm / ngày
                    workingHourInfos.AddRange(departmentChangeWorkingHours.Where(wh => wh.CalendarId == departmentCalendar.CalendarId).Select(wh => new WorkingHourInfoModel
                    {
                        StartDate = wh.StartDate.GetUnix(),
                        WorkingHourPerDay = wh.WorkingHourPerDay
                    }));

                    // Thêm thông tin ngày nghỉ
                    for (var day = start; day <= end; day = day.AddDays(1))
                    {
                        var clientDay = day.AddMinutes(-_currentContext.TimeZoneOffset.GetValueOrDefault());

                        if (departmentDayOffCalendars.Any(dof => dof.Day == day && dof.CalendarId == departmentCalendar.CalendarId)) continue;

                        var workingWeek = departmentChangeWorkingWeeks
                            .Where(w => w.DayOfWeek == (int)clientDay.DayOfWeek && w.StartDate <= day && w.CalendarId == departmentCalendar.CalendarId)
                            .OrderByDescending(w => w.StartDate)
                            .FirstOrDefault();

                        if (workingWeek == null) workingWeek = departmentWorkingWeeks
                                .Where(w => w.DayOfWeek == (int)clientDay.DayOfWeek && w.CalendarId == departmentCalendar.CalendarId)
                                .FirstOrDefault();

                        if (workingWeek?.IsDayOff ?? false)
                        {
                            lstDayOff.Add(new DayOffCalendarModel
                            {
                                Day = day.GetUnix(),
                                Content = CalendarTitle.OffDayOfWeek
                            });
                        }
                    }

                    lstDayOff.AddRange(departmentDayOffCalendars.Where(dof => dof.CalendarId == departmentCalendar.CalendarId).Select(dof => new DayOffCalendarModel
                    {
                        Day = dof.Day.GetUnix(),
                        Content = dof.Content
                    }));

                }
                else if (changeDepartmentCalendars.Count > 0)
                {
                    var prevCalendar = departmentCalendar;
                    var prevTimePoint = start;
                    foreach (var changeDepartmentCalendar in changeDepartmentCalendars)
                    {
                        if (prevCalendar != null)
                        {
                            // Thêm thông tin giờ làm / ngày
                            workingHourInfos.AddRange(departmentChangeWorkingHours
                                .Where(wh => wh.CalendarId == prevCalendar.CalendarId
                                && wh.StartDate > prevTimePoint
                                && wh.StartDate <= changeDepartmentCalendar.StartDate)
                                .Select(wh => new WorkingHourInfoModel
                                {
                                    StartDate = wh.StartDate.GetUnix(),
                                    WorkingHourPerDay = wh.WorkingHourPerDay
                                }));

                            // Thêm thông tin ngày nghỉ
                            for (var day = prevTimePoint; day < changeDepartmentCalendar.StartDate; day = day.AddDays(1))
                            {
                                var clientDay = day.AddMinutes(-_currentContext.TimeZoneOffset.GetValueOrDefault());

                                if (departmentDayOffCalendars.Any(dof => dof.Day == day && dof.CalendarId == prevCalendar.CalendarId)) continue;

                                var workingWeek = departmentChangeWorkingWeeks
                                    .Where(w => w.DayOfWeek == (int)clientDay.DayOfWeek && w.StartDate <= day && w.CalendarId == prevCalendar.CalendarId)
                                    .OrderByDescending(w => w.StartDate)
                                    .FirstOrDefault();

                                if (workingWeek == null) workingWeek = departmentWorkingWeeks
                                        .Where(w => w.DayOfWeek == (int)clientDay.DayOfWeek && w.CalendarId == prevCalendar.CalendarId)
                                        .FirstOrDefault();

                                if (workingWeek?.IsDayOff ?? false)
                                {
                                    lstDayOff.Add(new DayOffCalendarModel
                                    {
                                        Day = day.GetUnix(),
                                        Content = CalendarTitle.OffDayOfWeek
                                    });
                                }
                            }

                            lstDayOff.AddRange(departmentDayOffCalendars.Where(dof => dof.Day >= prevTimePoint
                                && dof.Day < changeDepartmentCalendar.StartDate
                                && dof.CalendarId == prevCalendar.CalendarId).Select(dof => new DayOffCalendarModel
                                {
                                    Day = dof.Day.GetUnix(),
                                    Content = dof.Content
                                }));
                        }
                        prevTimePoint = changeDepartmentCalendar.StartDate;
                        prevCalendar = changeDepartmentCalendar;
                    }

                    // Thêm thông tin giờ làm / ngày khoảng cuối
                    workingHourInfos.AddRange(departmentChangeWorkingHours
                                .Where(wh => wh.CalendarId == prevCalendar.CalendarId
                                && wh.StartDate > prevTimePoint
                                && wh.StartDate <= end)
                                .Select(wh => new WorkingHourInfoModel
                                {
                                    StartDate = wh.StartDate.GetUnix(),
                                    WorkingHourPerDay = wh.WorkingHourPerDay
                                }));

                    // Thêm thông tin ngày nghỉ khoảng cuối
                    for (var day = prevTimePoint; day <= end; day = day.AddDays(1))
                    {
                        var clientDay = day.AddMinutes(-_currentContext.TimeZoneOffset.GetValueOrDefault());

                        if (departmentDayOffCalendars.Any(dof => dof.Day == day && dof.CalendarId == prevCalendar.CalendarId)) continue;

                        var workingWeek = departmentChangeWorkingWeeks
                            .Where(w => w.DayOfWeek == (int)clientDay.DayOfWeek && w.StartDate <= day && w.CalendarId == prevCalendar.CalendarId)
                            .OrderByDescending(w => w.StartDate)
                            .FirstOrDefault();

                        if (workingWeek == null) workingWeek = departmentWorkingWeeks
                                .Where(w => w.DayOfWeek == (int)clientDay.DayOfWeek && w.CalendarId == prevCalendar.CalendarId)
                                .FirstOrDefault();

                        if (workingWeek?.IsDayOff ?? false)
                        {
                            lstDayOff.Add(new DayOffCalendarModel
                            {
                                Day = day.GetUnix(),
                                Content = CalendarTitle.OffDayOfWeek
                            });
                        }
                    }

                    lstDayOff.AddRange(departmentDayOffCalendars.Where(dof => dof.Day >= prevTimePoint
                        && dof.Day <= end
                        && dof.CalendarId == prevCalendar.CalendarId).Select(dof => new DayOffCalendarModel
                        {
                            Day = dof.Day.GetUnix(),
                            Content = dof.Content
                        }));
                }

                var departmentCalender = new DepartmentCalendarListModel
                {
                    DepartmentId = departmentId,
                    DepartmentWorkingHourInfo = workingHourInfos,
                    DepartmentDayOffCalendar = lstDayOff,
                    DepartmentOverHourInfo = deparmentOverHourInfos.Where(oh => oh.DepartmentId == departmentId).ToList(),
                    DepartmentIncreaseInfo = departmentIncreaseInfos.Where(i => i.DepartmentId == departmentId).ToList()
                };
                result.Add(departmentCalender);
            }


            return result;
        }

        #region Làm thêm giờ

        public async Task<PageData<DepartmentOverHourInfoModel>> GetDepartmentOverHourInfo(int departmentId, int page, int size)
        {
            var query = _organizationContext.DepartmentOverHourInfo
             .Where(oh => oh.DepartmentId == departmentId)
             .AsQueryable();

            var total = query.Count();

            query = query.OrderByDescending(oh => oh.StartDate);

            if (size > 0) query = query.Skip(size * (page - 1)).Take(size);

            var result = await query.ProjectTo<DepartmentOverHourInfoModel>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return (result, total);
        }

        public async Task<IList<DepartmentOverHourInfoModel>> GetDepartmentOverHourInfo(int[] departmentIds)
        {
            var result = await _organizationContext.DepartmentOverHourInfo
                .Where(oh => departmentIds.Contains(oh.DepartmentId))
                .ProjectTo<DepartmentOverHourInfoModel>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return result;
        }

        public async Task<DepartmentOverHourInfoModel> CreateDepartmentOverHourInfo(int departmentId, DepartmentOverHourInfoModel data)
        {
            try
            {
                var department = _organizationContext.Department.FirstOrDefault(d => d.DepartmentId == departmentId);
                if (department == null) throw DepartmentNotFound.BadRequest();

                if (_organizationContext.DepartmentOverHourInfo.Any(oh => oh.StartDate <= data.EndDate.UnixToDateTime() && oh.EndDate >= data.StartDate.UnixToDateTime() && oh.DepartmentId == departmentId))
                    throw DuplicateDateRange.BadRequest();

                var overHour = _mapper.Map<DepartmentOverHourInfo>(data);
                overHour.DepartmentId = departmentId;
                _organizationContext.DepartmentOverHourInfo.Add(overHour);
                _organizationContext.SaveChanges();

                await _departmentActivityLog.LogBuilder(() => DepartmentActivityLogMessage.DepartmentOverHourCreate)
                   .MessageResourceFormatDatas(overHour.StartDate, overHour.EndDate, department.DepartmentCode)
                   .ObjectId(department.DepartmentId)
                   .JsonData(data.JsonSerialize())
                   .CreateLog();

                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreateDepartmentOverHour");
                throw;
            }
        }

        public async Task<DepartmentOverHourInfoModel> UpdateDepartmentOverHourInfo(int departmentId, long departmentOverHourInfoId, DepartmentOverHourInfoModel data)
        {
            try
            {
                var department = _organizationContext.Department.FirstOrDefault(d => d.DepartmentId == departmentId);
                if (department == null) throw DepartmentNotFound.BadRequest();

                var overHour = _organizationContext.DepartmentOverHourInfo.FirstOrDefault(oh => oh.DepartmentOverHourInfoId == departmentOverHourInfoId && oh.DepartmentId == departmentId);
                if (overHour == null) throw DepartmentOverHourInfoNotFound.BadRequest();

                if (_organizationContext.DepartmentOverHourInfo.Any(oh => oh.DepartmentOverHourInfoId != departmentOverHourInfoId && oh.StartDate <= data.EndDate.UnixToDateTime() && oh.EndDate >= data.StartDate.UnixToDateTime() && oh.DepartmentId == departmentId))
                    throw DuplicateDateRange.BadRequest();

                _mapper.Map(data, overHour);

                overHour.DepartmentId = departmentId;

                _organizationContext.SaveChanges();

                await _departmentActivityLog.LogBuilder(() => DepartmentActivityLogMessage.DepartmentOverHourUpdate)
                 .MessageResourceFormatDatas(overHour.StartDate, overHour.EndDate, department.DepartmentCode)
                 .ObjectId(department.DepartmentId)
                 .JsonData(data.JsonSerialize())
                 .CreateLog();

                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateDepartmentOverHour");
                throw;
            }
        }

        public async Task<IList<DepartmentOverHourInfoModel>> UpdateDepartmentOverHourInfoMultiple(IList<DepartmentOverHourInfoModel> data)
        {
            try
            {
                var departmentIds = data.Select(oh => oh.DepartmentId).Distinct().ToList();

                var departments = _organizationContext.Department.Where(d => departmentIds.Contains(d.DepartmentId)).ToList();

                if (departmentIds.Count != departments.Count) throw DepartmentNotFound.BadRequest();

                if (data.Any(oh => data.Any(ooh => ooh != oh && oh.StartDate <= oh.EndDate && oh.EndDate >= oh.StartDate && ooh.DepartmentId == oh.DepartmentId)))
                    throw DuplicateDateRange.BadRequest();

                var currentOverHours = _organizationContext.DepartmentOverHourInfo.Where(oh => departmentIds.Contains(oh.DepartmentId)).ToList();

                var batchLog = _departmentActivityLog.BeginBatchLog();
                foreach (var model in data)
                {
                    var departmentInfo = departments.FirstOrDefault(d => d.DepartmentId == model.DepartmentId);

                    var currentOverHour = currentOverHours.FirstOrDefault(oh => oh.DepartmentOverHourInfoId == model.DepartmentOverHourInfoId);
                    if (currentOverHour == null)
                    {
                        currentOverHour = _mapper.Map<DepartmentOverHourInfo>(model);
                        _organizationContext.DepartmentOverHourInfo.Add(currentOverHour);


                        await _departmentActivityLog.LogBuilder(() => DepartmentActivityLogMessage.DepartmentOverHourCreate)
                          .MessageResourceFormatDatas(currentOverHour.StartDate, currentOverHour.EndDate, departmentInfo.DepartmentCode)
                          .ObjectId(departmentInfo.DepartmentId)
                          .JsonData(model.JsonSerialize())
                          .CreateLog();

                    }
                    else
                    {
                        currentOverHours.Remove(currentOverHour);
                        _mapper.Map(model, currentOverHour);

                        await _departmentActivityLog.LogBuilder(() => DepartmentActivityLogMessage.DepartmentOverHourUpdate)
                          .MessageResourceFormatDatas(currentOverHour.StartDate, currentOverHour.EndDate, departmentInfo.DepartmentCode)
                          .ObjectId(departmentInfo.DepartmentId)
                          .JsonData(model.JsonSerialize())
                          .CreateLog();
                    }
                }

                _organizationContext.DepartmentOverHourInfo.RemoveRange(currentOverHours);
                _organizationContext.SaveChanges();


                var result = await _organizationContext.DepartmentOverHourInfo
                      .Where(oh => departmentIds.Contains(oh.DepartmentId))
                      .ProjectTo<DepartmentOverHourInfoModel>(_mapper.ConfigurationProvider)
                      .ToListAsync();

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateMultipleDepartmentOverHour");
                throw;
            }
        }

        public async Task<bool> DeleteDepartmentOverHourInfo(int departmentId, long departmentOverHourInfoId)
        {
            try
            {
                var department = _organizationContext.Department.FirstOrDefault(d => d.DepartmentId == departmentId);
                if (department == null) throw DepartmentNotFound.BadRequest();

                var overHour = _organizationContext.DepartmentOverHourInfo.FirstOrDefault(oh => oh.DepartmentOverHourInfoId == departmentOverHourInfoId && oh.DepartmentId == departmentId);
                if (overHour == null) throw DepartmentOverHourInfoNotFound.BadRequest();

                _organizationContext.DepartmentOverHourInfo.Remove(overHour);
                _organizationContext.SaveChanges();

                await _departmentActivityLog.LogBuilder(() => DepartmentActivityLogMessage.DepartmentOverHourDelete)
                  .MessageResourceFormatDatas(overHour.StartDate, overHour.EndDate, department.DepartmentCode)
                  .ObjectId(department.DepartmentId)
                  .JsonData(overHour.JsonSerialize())
                  .CreateLog();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeleteDepartmentOverHour");
                throw;
            }
        }


        #endregion

        #region Nhân sự tăng cường
        public async Task<PageData<DepartmentIncreaseInfoModel>> GetDepartmentIncreaseInfo(int departmentId, int page, int size)
        {
            var query = _organizationContext.DepartmentIncreaseInfo
                .Where(oh => oh.DepartmentId == departmentId)
                .AsQueryable();

            var total = query.Count();

            query = query.OrderByDescending(oh => oh.StartDate);

            if (size > 0) query = query.Skip(size * (page - 1)).Take(size);

            var result = await query.ProjectTo<DepartmentIncreaseInfoModel>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return (result, total);
        }

        public async Task<IList<DepartmentIncreaseInfoModel>> GetDepartmentIncreaseInfo(int[] departmentIds)
        {
            var result = await _organizationContext.DepartmentIncreaseInfo
                .Where(oh => departmentIds.Contains(oh.DepartmentId))
                .ProjectTo<DepartmentIncreaseInfoModel>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return result;
        }

        public async Task<DepartmentIncreaseInfoModel> CreateDepartmentIncreaseInfo(int departmentId, DepartmentIncreaseInfoModel data)
        {
            try
            {
                var department = _organizationContext.Department.FirstOrDefault(d => d.DepartmentId == departmentId);
                if (department == null) throw new BadRequestException(GeneralCode.ItemNotFound, "Bộ phận không tồn tại");

                if (data.NumberOfPerson <= 0) throw new BadRequestException(GeneralCode.InvalidParams, "Số lượng nhân sự tăng ca không hợp lệ");

                if (_organizationContext.DepartmentIncreaseInfo.Any(oh => oh.StartDate <= data.EndDate.UnixToDateTime() && oh.EndDate >= data.StartDate.UnixToDateTime() && oh.DepartmentId == departmentId))
                    throw new BadRequestException(GeneralCode.InvalidParams, "Trùng khoảng thời gian với giai đoạn đã tồn tại");

                var increase = _mapper.Map<DepartmentIncreaseInfo>(data);
                increase.DepartmentId = departmentId;
                _organizationContext.DepartmentIncreaseInfo.Add(increase);
                _organizationContext.SaveChanges();

                await _departmentActivityLog.LogBuilder(() => DepartmentActivityLogMessage.DepartmentIncreaseCreate)
                   .MessageResourceFormatDatas(increase.StartDate, increase.EndDate, department.DepartmentCode)
                   .ObjectId(department.DepartmentId)
                   .JsonData(data.JsonSerialize())
                   .CreateLog();

                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DepartmentIncreaseCreate");
                throw;
            }
        }

        public async Task<DepartmentIncreaseInfoModel> UpdateDepartmentIncreaseInfo(int departmentId, long departmentIncreaseInfoId, DepartmentIncreaseInfoModel data)
        {
            try
            {
                var department = _organizationContext.Department.FirstOrDefault(d => d.DepartmentId == departmentId);
                if (department == null) throw new BadRequestException(GeneralCode.ItemNotFound, "Bộ phận không tồn tại");

                if (data.NumberOfPerson <= 0) throw new BadRequestException(GeneralCode.InvalidParams, "Số lượng nhân sự tăng ca không hợp lệ");

                var increase = _organizationContext.DepartmentIncreaseInfo.FirstOrDefault(oh => oh.DepartmentIncreaseInfoId == departmentIncreaseInfoId && oh.DepartmentId == departmentId);
                if (increase == null) throw new BadRequestException(GeneralCode.ItemNotFound, "Thông tin nhân sự tăng cường không tồn tại");

                if (_organizationContext.DepartmentIncreaseInfo.Any(oh => oh.DepartmentIncreaseInfoId != departmentIncreaseInfoId && oh.StartDate <= data.EndDate.UnixToDateTime() && oh.EndDate >= data.StartDate.UnixToDateTime() && oh.DepartmentId == departmentId))
                    throw new BadRequestException(GeneralCode.InvalidParams, "Trùng khoảng thời gian với giai đoạn đã tồn tại");

                _mapper.Map(data, increase);

                increase.DepartmentId = departmentId;

                _organizationContext.SaveChanges();

                await _departmentActivityLog.LogBuilder(() => DepartmentActivityLogMessage.DepartmentIncreaseUpdate)
                 .MessageResourceFormatDatas(increase.StartDate, increase.EndDate, department.DepartmentCode)
                 .ObjectId(department.DepartmentId)
                 .JsonData(data.JsonSerialize())
                 .CreateLog();

                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DepartmentIncreaseUpdate");
                throw;
            }
        }

        public async Task<IList<DepartmentIncreaseInfoModel>> UpdateDepartmentIncreaseInfoMultiple(IList<DepartmentIncreaseInfoModel> data)
        {
            try
            {
                var departmentIds = data.Select(oh => oh.DepartmentId).Distinct().ToList();

                var departments = _organizationContext.Department.Where(d => departmentIds.Contains(d.DepartmentId)).ToList();

                if (departmentIds.Count != departments.Count) throw new BadRequestException(GeneralCode.ItemNotFound, "Bộ phận không tồn tại");
                if (data.Any(d => d.NumberOfPerson <= 0)) throw new BadRequestException(GeneralCode.InvalidParams, "Số lượng nhân sự tăng ca không hợp lệ");
                if (data.Any(oh => data.Any(ooh => ooh != oh && oh.StartDate <= oh.EndDate && oh.EndDate >= oh.StartDate && ooh.DepartmentId == oh.DepartmentId)))
                    throw new BadRequestException(GeneralCode.InvalidParams, "Trùng khoảng thời gian với giai đoạn đã tồn tại");

                var currentIncreases = _organizationContext.DepartmentIncreaseInfo.Where(oh => departmentIds.Contains(oh.DepartmentId)).ToList();

                var batchLog = _departmentActivityLog.BeginBatchLog();
                foreach (var model in data)
                {
                    var departmentInfo = departments.FirstOrDefault(d => d.DepartmentId == model.DepartmentId);

                    var currentIncrease = currentIncreases.FirstOrDefault(oh => oh.DepartmentIncreaseInfoId == model.DepartmentIncreaseInfoId);
                    if (currentIncrease == null)
                    {
                        currentIncrease = _mapper.Map<DepartmentIncreaseInfo>(model);
                        _organizationContext.DepartmentIncreaseInfo.Add(currentIncrease);


                        await _departmentActivityLog.LogBuilder(() => DepartmentActivityLogMessage.DepartmentIncreaseCreate)
                          .MessageResourceFormatDatas(currentIncrease.StartDate, currentIncrease.EndDate, departmentInfo.DepartmentCode)
                          .ObjectId(departmentInfo.DepartmentId)
                          .JsonData(model.JsonSerialize())
                          .CreateLog();

                    }
                    else
                    {
                        currentIncreases.Remove(currentIncrease);
                        _mapper.Map(model, currentIncrease);

                        await _departmentActivityLog.LogBuilder(() => DepartmentActivityLogMessage.DepartmentIncreaseUpdate)
                          .MessageResourceFormatDatas(currentIncrease.StartDate, currentIncrease.EndDate, departmentInfo.DepartmentCode)
                          .ObjectId(departmentInfo.DepartmentId)
                          .JsonData(model.JsonSerialize())
                          .CreateLog();
                    }
                }

                _organizationContext.DepartmentIncreaseInfo.RemoveRange(currentIncreases);
                _organizationContext.SaveChanges();


                var result = await _organizationContext.DepartmentIncreaseInfo
                      .Where(oh => departmentIds.Contains(oh.DepartmentId))
                      .ProjectTo<DepartmentIncreaseInfoModel>(_mapper.ConfigurationProvider)
                      .ToListAsync();

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateMultipleDepartmentIncrease");
                throw;
            }
        }

        public async Task<bool> DeleteDepartmentIncreaseInfo(int departmentId, long departmentIncreaseInfoId)
        {
            try
            {
                var department = _organizationContext.Department.FirstOrDefault(d => d.DepartmentId == departmentId);
                if (department == null) throw new BadRequestException(GeneralCode.ItemNotFound, "Bộ phận không tồn tại");

                var increase = _organizationContext.DepartmentIncreaseInfo.FirstOrDefault(oh => oh.DepartmentIncreaseInfoId == departmentIncreaseInfoId && oh.DepartmentId == departmentId);
                if (increase == null) throw new BadRequestException(GeneralCode.ItemNotFound, "Thông tin tăng ca không tồn tại");

                _organizationContext.DepartmentIncreaseInfo.Remove(increase);
                _organizationContext.SaveChanges();

                await _departmentActivityLog.LogBuilder(() => DepartmentActivityLogMessage.DepartmentIncreaseDelete)
                  .MessageResourceFormatDatas(increase.StartDate, increase.EndDate, department.DepartmentCode)
                  .ObjectId(department.DepartmentId)
                  .JsonData(increase.JsonSerialize())
                  .CreateLog();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DepartmentIncreaseDelete");
                throw;
            }
        }

        #endregion
    }
}
