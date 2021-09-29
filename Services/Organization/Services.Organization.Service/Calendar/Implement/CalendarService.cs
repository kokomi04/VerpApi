﻿using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VErp.Commons.Constants;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.OrganizationDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Organization.Model.Calendar;

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
        }


        public async Task<WeekCalendarModel> GetCurrentCalendar()
        {
            var now = DateTime.UtcNow.Date;
            var workingHourInfo = await _organizationContext.WorkingHourInfo
                .Where(wh => wh.StartDate <= now)
                .OrderByDescending(wh => wh.StartDate)
                .FirstOrDefaultAsync();

            var workingWeeks = await _organizationContext.WorkingWeekInfo
                .Where(ww => ww.StartDate <= now)
                .GroupBy(ww => ww.DayOfWeek)
                .Select(g => new
                {
                    DayOfWeek = g.Key,
                    StartDate = g.Max(ww => ww.StartDate)
                })
                .Join(_organizationContext.WorkingWeekInfo, w => new { w.StartDate, w.DayOfWeek }, ww => new { ww.StartDate, ww.DayOfWeek }, (w, ww) => ww)
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

        public async Task<IList<WeekCalendarModel>> GetCalendar()
        {

            var workingHourInfos = await _organizationContext.WorkingHourInfo
                .ToListAsync();

            var allWorkingWeeks = await _organizationContext.WorkingWeekInfo
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
                    .Join(allWorkingWeeks, w => new { w.StartDate, w.DayOfWeek }, ww => new { ww.StartDate, ww.DayOfWeek }, (w, ww) => ww)
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

        public async Task<IList<DayOffCalendarModel>> GetDayOffCalendar(long startDate, long endDate)
        {
            var start = startDate.UnixToDateTime().Value;
            var end = endDate.UnixToDateTime().Value;
            var dayOffCalendar = await _organizationContext.DayOffCalendar
                .Where(dof => dof.Day >= start && dof.Day <= end)
                .ToListAsync();

            // Lấy thông tin ngày làm việc trong tuần từ ngày bắt đầu
            var workingWeeks = await _organizationContext.WorkingWeekInfo
               .Where(ww => ww.StartDate <= start)
               .GroupBy(ww => ww.DayOfWeek)
               .Select(g => new
               {
                   DayOfWeek = g.Key,
                   StartDate = g.Max(ww => ww.StartDate)
               })
               .Join(_organizationContext.WorkingWeekInfo, w => new { w.StartDate, w.DayOfWeek }, ww => new { ww.StartDate, ww.DayOfWeek }, (w, ww) => ww)
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
                .Where(ww => ww.StartDate > start && ww.StartDate <= end)
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
                        Content = "Nghỉ làm cố định trong tuần"
                    });
                }
            }

            foreach (var dayOff in dayOffCalendar)
            {
                lstDayOff.Add(_mapper.Map<DayOffCalendarModel>(dayOff));
            }

            return lstDayOff.OrderBy(d => d.Day).ToList();
        }

        public async Task<DayOffCalendarModel> UpdateDayOff(DayOffCalendarModel data)
        {
            try
            {
                var dayOff = await _organizationContext.DayOffCalendar
                .FirstOrDefaultAsync(dof => dof.Day == data.Day.UnixToDateTime());
                if (dayOff == null)
                {
                    dayOff = _mapper.Map<DayOffCalendar>(data);
                    _organizationContext.DayOffCalendar.Add(dayOff);
                }
                else if (dayOff.Content != data.Content)
                {
                    dayOff.Content = data.Content;
                }
                _organizationContext.SaveChanges();

                await _activityLogService.CreateLog(EnumObjectType.DayOffCalendar, data.Day, $"Cập nhật ngày nghỉ {data.Day.UnixToDateTime()}", data.JsonSerialize());
                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateDayOffCalendar");
                throw;
            }
        }

        public async Task<bool> DeleteDayOff(long day)
        {
            try
            {
                var dayOff = await _organizationContext.DayOffCalendar
                .FirstOrDefaultAsync(dof => dof.Day == day.UnixToDateTime());
                if (dayOff == null) throw new BadRequestException(GeneralCode.ItemNotFound, "Ngày nghỉ không tồn tại");
                _organizationContext.DayOffCalendar.Remove(dayOff);
                _organizationContext.SaveChanges();
                await _activityLogService.CreateLog(EnumObjectType.DayOffCalendar, day, $"Xóa ngày nghỉ {day.UnixToDateTime()}", dayOff.JsonSerialize());
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeleteDayOffCalendar");
                throw;
            }
        }

        public async Task<bool> DeleteWeekCalendar(long startDate)
        {
            using var trans = await _organizationContext.Database.BeginTransactionAsync();
            try
            {
                DateTime time = startDate.UnixToDateTime().Value;
                var workingHourInfo = await _organizationContext.WorkingHourInfo.FirstOrDefaultAsync(wh => wh.StartDate == time);

                if (workingHourInfo != null)
                {
                    _organizationContext.WorkingHourInfo.Remove(workingHourInfo);
                }

                var workingWeeks = await _organizationContext.WorkingWeekInfo
                  .Where(ww => ww.StartDate == time)
                  .ToListAsync();

                if(workingWeeks.Count > 0)
                {
                    _organizationContext.WorkingWeekInfo.RemoveRange(workingWeeks);
                }

                await _organizationContext.SaveChangesAsync();
                trans.Commit();

                await _activityLogService.CreateLog(EnumObjectType.Calendar, time.GetUnix(), $"Xóa thay đổi lịch làm việc ngày {time.ToString()}", string.Empty);

                return true;
            }
            catch (Exception ex)
            {
                trans.TryRollbackTransaction();
                _logger.LogError(ex, "DeleteCalendar");
                throw;
            }
        }


        public async Task<WeekCalendarModel> CreateWeekCalendar(WeekCalendarModel data)
        {
            using var trans = await _organizationContext.Database.BeginTransactionAsync();
            try
            {
                DateTime time = data.StartDate.HasValue ? data.StartDate.UnixToDateTime().Value : DateTime.UtcNow.Date;
               
               
                var currentWorkingHourInfo = await _organizationContext.WorkingHourInfo
                  .Where(wh => wh.StartDate == time)
                  .FirstOrDefaultAsync();

                var currentWorkingWeeks = await _organizationContext.WorkingWeekInfo
                   .Where(ww => ww.StartDate == time)
                   .ToListAsync();

                if (currentWorkingHourInfo != null || currentWorkingWeeks.Count > 0)
                {
                    throw new BadRequestException(GeneralCode.InvalidParams, $"Đã tồn tại thay đổi lịch làm việc vào ngày {time.AddMinutes(-_currentContext.TimeZoneOffset.GetValueOrDefault()).ToString("dd/MM/yyyy")}");
                }

                foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
                {
                    var newWorkingWeek = data.WorkingWeek.FirstOrDefault(w => w.DayOfWeek == day);
                    if (newWorkingWeek == null) throw new BadRequestException(GeneralCode.InvalidParams, "Thông tin làm việc trong tuần chưa đủ");
                }

                // Update workingHour per day
                currentWorkingHourInfo = new WorkingHourInfo
                {
                    StartDate = DateTime.MinValue,
                    WorkingHourPerDay = data.WorkingHourPerDay
                };
                _organizationContext.WorkingHourInfo.Add(currentWorkingHourInfo);


                // Update working week
                foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
                {
                    var newWorkingWeek = data.WorkingWeek.FirstOrDefault(w => w.DayOfWeek == day);
                    var currentWorkingWeek = new WorkingWeekInfo
                    {
                        StartDate = DateTime.MinValue,
                        DayOfWeek = (int)day,
                        IsDayOff = newWorkingWeek.IsDayOff
                    };
                    _organizationContext.WorkingWeekInfo.Add(currentWorkingWeek);
                }
               
                _organizationContext.SaveChanges();
                trans.Commit();

                await _activityLogService.CreateLog(EnumObjectType.Calendar, time.GetUnix(), $"Thêm mới thay đổi lịch làm việc ngày {time.ToString()}", data.JsonSerialize());

                return await GetCurrentCalendar();
            }
            catch (Exception ex)
            {
                trans.TryRollbackTransaction();
                _logger.LogError(ex, "CreateCalendar");
                throw;
            }
        }

        public async Task<WeekCalendarModel> UpdateWeekCalendar(long oldDate, WeekCalendarModel data)
        {
            using var trans = await _organizationContext.Database.BeginTransactionAsync();
            try
            {
                if(!data.StartDate.HasValue) throw new BadRequestException(GeneralCode.InvalidParams, "Vui lòng chọn ngày hiệu lực");
                DateTime oldTime = oldDate.UnixToDateTime().Value;
                DateTime time = data.StartDate.UnixToDateTime().Value;

                var newWorkingHourInfo = await _organizationContext.WorkingHourInfo
                 .Where(wh => wh.StartDate == time)
                 .FirstOrDefaultAsync();

                var newWorkingWeeks = await _organizationContext.WorkingWeekInfo
                   .Where(ww => ww.StartDate == time)
                   .ToListAsync();

                if (newWorkingHourInfo != null || newWorkingWeeks.Count > 0)
                {
                    throw new BadRequestException(GeneralCode.InvalidParams, $"Thay đổi lịch làm việc vào ngày {time.AddMinutes(-_currentContext.TimeZoneOffset.GetValueOrDefault()).ToString("dd/MM/yyyy")} đã tồn tại");
                }


                var currentWorkingHourInfo = await _organizationContext.WorkingHourInfo
                  .Where(wh => wh.StartDate == oldTime)
                  .FirstOrDefaultAsync();

                var currentWorkingWeeks = await _organizationContext.WorkingWeekInfo
                   .Where(ww => ww.StartDate == oldTime)
                   .ToListAsync();

                if (currentWorkingHourInfo == null || currentWorkingWeeks.Count == 0)
                {
                    throw new BadRequestException(GeneralCode.InvalidParams, $"Không tồn tại thay đổi lịch làm việc vào ngày {oldTime.AddMinutes(-_currentContext.TimeZoneOffset.GetValueOrDefault()).ToString("dd/MM/yyyy")}");
                }

                foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
                {
                    var newWorkingWeek = data.WorkingWeek.FirstOrDefault(w => w.DayOfWeek == day);
                    if (newWorkingWeek == null) throw new BadRequestException(GeneralCode.InvalidParams, "Thông tin làm việc trong tuần chưa đủ");
                }

                // Update workingHour per day
                currentWorkingHourInfo.WorkingHourPerDay = data.WorkingHourPerDay;
                currentWorkingHourInfo.StartDate = time;

                // Update working week
                foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
                {
                    var newWorkingWeek = data.WorkingWeek.FirstOrDefault(w => w.DayOfWeek == day);
                    var currentWorkingWeek = currentWorkingWeeks.FirstOrDefault(ww => ww.DayOfWeek == (int)day);
                    currentWorkingWeek.IsDayOff = newWorkingWeek.IsDayOff;
                    currentWorkingWeek.StartDate = time;
                }

                _organizationContext.SaveChanges();
                trans.Commit();

                await _activityLogService.CreateLog(EnumObjectType.Calendar, time.GetUnix(), $"Cập nhật thay đổi lịch làm việc ngày {oldTime.ToString()}", data.JsonSerialize());

                return await GetCurrentCalendar();
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
