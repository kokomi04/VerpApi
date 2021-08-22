using AutoMapper;
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
using VErp.Services.Organization.Model.DepartmentCalendar;

namespace VErp.Services.Organization.Service.DepartmentCalendar.Implement
{
    public class DepartmentCalendarService : IDepartmentCalendarService
    {
        private readonly OrganizationDBContext _organizationContext;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly IActivityLogService _activityLogService;
        private readonly IMapper _mapper;
        private readonly ICurrentContextService _currentContext;
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
            _activityLogService = activityLogService;
            _mapper = mapper;
            _currentContext = currentContext;
        }

        public async Task<DepartmentWeekCalendarModel> GetCurrentDepartmentCalendar(int departmentId)
        {
            var now = DateTime.UtcNow.Date;
            double workingHourPerDay = 0;
            var departmentWorkingHourInfo = await _organizationContext.DepartmentWorkingHourInfo
                .Where(wh => wh.StartDate <= now && wh.DepartmentId == departmentId)
                .OrderByDescending(wh => wh.StartDate)
                .FirstOrDefaultAsync();

            if (departmentWorkingHourInfo == null)
            {
                var workingHourInfo = await _organizationContext.WorkingHourInfo
                       .Where(wh => wh.StartDate <= now)
                       .OrderByDescending(wh => wh.StartDate)
                       .FirstOrDefaultAsync();
                workingHourPerDay = workingHourInfo?.WorkingHourPerDay ?? OrganizationConstants.WORKING_HOUR_PER_DAY;
            }
            else
            {
                workingHourPerDay = departmentWorkingHourInfo.WorkingHourPerDay;
            }

            var departmentWorkingWeeks = await _organizationContext.DepartmentWorkingWeekInfo
               .Where(ww => ww.StartDate <= now && ww.DepartmentId == departmentId)
               .GroupBy(ww => ww.DayOfWeek)
               .Select(g => new
               {
                   DayOfWeek = g.Key,
                   StartDate = g.Max(ww => ww.StartDate)
               })
               .Join(_organizationContext.DepartmentWorkingWeekInfo, w => new { w.StartDate, w.DayOfWeek, DepartmentId = departmentId }, ww => new { ww.StartDate, ww.DayOfWeek, ww.DepartmentId }, (w, ww) => ww)
               .ProjectTo<DepartmentWorkingWeekInfoModel>(_mapper.ConfigurationProvider)
               .ToListAsync();

            if (departmentWorkingWeeks.Count == 0)
            {
                var workingWeeks = await _organizationContext.WorkingWeekInfo
                    .Where(ww => ww.StartDate <= now)
                    .GroupBy(ww => ww.DayOfWeek)
                    .Select(g => new
                    {
                        DayOfWeek = g.Key,
                        StartDate = g.Max(ww => ww.StartDate)
                    })
                    .Join(_organizationContext.WorkingWeekInfo, w => new { w.StartDate, w.DayOfWeek }, ww => new { ww.StartDate, ww.DayOfWeek }, (w, ww) => ww)
                    .ToListAsync();

                departmentWorkingWeeks = workingWeeks
                    .Select(ww => new DepartmentWorkingWeekInfoModel
                    {
                        DepartmentId = departmentId,
                        DayOfWeek = (DayOfWeek)ww.DayOfWeek,
                        IsDayOff = ww.IsDayOff
                    })
                    .ToList();
            }


            foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
            {
                if (!departmentWorkingWeeks.Any(d => d.DayOfWeek == day))
                {
                    departmentWorkingWeeks.Add(new DepartmentWorkingWeekInfoModel
                    {
                        DepartmentId = departmentId,
                        DayOfWeek = day,
                        IsDayOff = false
                    });
                }
            }

            DepartmentWeekCalendarModel result = new DepartmentWeekCalendarModel
            {
                DepartmentId = departmentId,
                WorkingHourPerDay = workingHourPerDay,
                DepartmentWorkingWeek = departmentWorkingWeeks
            };

            return result;
        }

        public async Task<IList<DepartmentCalendarListModel>> GetListDepartmentCalendar(int[] departmentIds, long startDate, long endDate)
        {
            var start = startDate.UnixToDateTime().Value;
            var end = endDate.UnixToDateTime().Value;

            // Thông tin giờ làm việc / ngày
            var departmentWorkingHourInfo = await _organizationContext.DepartmentWorkingHourInfo
                .Where(wh => wh.StartDate <= start && departmentIds.Contains(wh.DepartmentId))
                .GroupBy(wh => wh.DepartmentId)
                .Select(g => new
                {
                    DepartmentId = g.Key,
                    StartDate = g.Max(wh => wh.StartDate)
                })
                .Join(_organizationContext.DepartmentWorkingHourInfo, w => new { w.StartDate, w.DepartmentId }, wh => new { wh.StartDate, wh.DepartmentId }, (w, wh) => wh)
                .ProjectTo<DepartmentWorkingHourInfoModel>(_mapper.ConfigurationProvider)
                .ToListAsync();

            var notExitWorkingHourInfoIds = departmentIds.Where(departmentId => !departmentWorkingHourInfo.Any(wh => wh.DepartmentId == departmentId)).ToList();

            if (notExitWorkingHourInfoIds.Count > 0)
            {
                departmentWorkingHourInfo.AddRange(_organizationContext.WorkingHourInfo
                       .Where(wh => wh.StartDate <= start)
                       .OrderByDescending(wh => wh.StartDate)
                       .ToList()
                       .SelectMany(wh => notExitWorkingHourInfoIds
                           .Select(departmentId => new DepartmentWorkingHourInfoModel
                           {
                               DepartmentId = departmentId,
                               StartDate = wh.StartDate.GetUnix(),
                               WorkingHourPerDay = wh.WorkingHourPerDay
                           })
                           .ToList())
                       .ToList());
            }

            departmentWorkingHourInfo.AddRange(departmentIds
                .Where(departmentId => !departmentWorkingHourInfo.Any(wh => wh.DepartmentId == departmentId))
                .Select(departmentId => new DepartmentWorkingHourInfoModel
                {
                    DepartmentId = departmentId,
                    StartDate = DateTime.MinValue.GetUnix(),
                    WorkingHourPerDay = OrganizationConstants.WORKING_HOUR_PER_DAY
                }).ToList());

            // Lấy thông tin thay đổi trong khoảng thời gian
            var departmentChangeWorkingHours = await _organizationContext.DepartmentWorkingHourInfo
                .Where(ww => ww.StartDate > start && ww.StartDate <= end && departmentIds.Contains(ww.DepartmentId))
                .ProjectTo<DepartmentWorkingHourInfoModel>(_mapper.ConfigurationProvider)
                .ToListAsync();

            var notExitChangeWorkingHourIds = notExitWorkingHourInfoIds.Where(departmentId => !departmentChangeWorkingHours.Any(wh => wh.DepartmentId == departmentId)).ToList();

            if (notExitChangeWorkingHourIds.Count > 0)
            {
                departmentChangeWorkingHours.AddRange(_organizationContext.WorkingHourInfo
                       .Where(wh => wh.StartDate > start && wh.StartDate <= end)
                       .ToList()
                       .SelectMany(wh => notExitChangeWorkingHourIds
                           .Select(departmentId => new DepartmentWorkingHourInfoModel
                           {
                               DepartmentId = departmentId,
                               StartDate = wh.StartDate.GetUnix(),
                               WorkingHourPerDay = wh.WorkingHourPerDay
                           }))
                       .ToList());
            }


            // Thông tin ngày nghỉ
            var departmentDayOffCalendar = await _organizationContext.DepartmentDayOffCalendar
               .Where(dof => dof.Day >= start && dof.Day <= end && departmentIds.Contains(dof.DepartmentId))
               .ToListAsync();

            var lstDay = departmentDayOffCalendar.Select(dof => dof.Day).ToList();
            var dayOffCalendar = _organizationContext.DayOffCalendar
                .Where(dof => dof.Day >= start && dof.Day <= end && !lstDay.Contains(dof.Day))
                .ToList()
                .SelectMany(dof => departmentIds
                    .Select(departmentId => new DepartmentDayOffCalendar
                    {
                        DepartmentId = departmentId,
                        Day = dof.Day,
                        Content = dof.Content,
                        SubsidiaryId = dof.SubsidiaryId
                    })
                    .ToList())
                .ToList();

            // Lấy thông tin ngày làm việc trong tuần từ ngày bắt đầu
            var departmentWorkingWeeks = await _organizationContext.DepartmentWorkingWeekInfo
              .Where(ww => ww.StartDate <= start && departmentIds.Contains(ww.DepartmentId))
              .GroupBy(ww => new { ww.DayOfWeek, ww.DepartmentId })
              .Select(g => new
              {
                  g.Key.DayOfWeek,
                  g.Key.DepartmentId,
                  StartDate = g.Max(ww => ww.StartDate)
              })
              .Join(_organizationContext.DepartmentWorkingWeekInfo, w => new { w.StartDate, w.DayOfWeek, w.DepartmentId }, ww => new { ww.StartDate, ww.DayOfWeek, ww.DepartmentId }, (w, ww) => ww)
              .ToListAsync();

            var notExitWorkingWeekIds = departmentIds.Where(departmentId => !departmentWorkingWeeks.Any(ww => ww.DepartmentId == departmentId)).ToList();
            if (notExitWorkingWeekIds.Count > 0)
            {
                departmentWorkingWeeks.AddRange(_organizationContext.WorkingWeekInfo
               .Where(ww => ww.StartDate <= start)
               .GroupBy(ww => ww.DayOfWeek)
               .Select(g => new
               {
                   DayOfWeek = g.Key,
                   StartDate = g.Max(ww => ww.StartDate)
               })
               .Join(_organizationContext.WorkingWeekInfo, w => new { w.StartDate, w.DayOfWeek }, ww => new { ww.StartDate, ww.DayOfWeek }, (w, ww) => ww)
               .ToList()
               .SelectMany(ww => notExitWorkingWeekIds
                   .Select(departmentId => new DepartmentWorkingWeekInfo
                   {
                       DepartmentId = departmentId,
                       DayOfWeek = ww.DayOfWeek,
                       IsDayOff = ww.IsDayOff,
                       StartDate = ww.StartDate,
                       SubsidiaryId = ww.SubsidiaryId
                   })
                   .ToList())
               .ToList());
            }


            foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
            {
                foreach (var departmentId in departmentIds)
                {
                    if (!departmentWorkingWeeks.Any(d => d.DayOfWeek == (int)day && d.DepartmentId == departmentId))
                    {
                        departmentWorkingWeeks.Add(new DepartmentWorkingWeekInfo
                        {
                            DepartmentId = departmentId,
                            DayOfWeek = (int)day,
                            IsDayOff = false,
                            StartDate = start,
                            SubsidiaryId = _currentContext.SubsidiaryId
                        });
                    }
                }
            }

            // Lấy thông tin thay đổi trong khoảng thời gian
            var departmentChangeWorkingWeeks = await _organizationContext.DepartmentWorkingWeekInfo
                .Where(ww => ww.StartDate > start && ww.StartDate <= end && departmentIds.Contains(ww.DepartmentId))
                .ToListAsync();

            var notExitChangeWorkingWeekIds = notExitWorkingWeekIds.Where(departmentId => !departmentChangeWorkingWeeks.Any(ww => ww.DepartmentId == departmentId)).ToList();


            if (notExitChangeWorkingWeekIds.Count > 0)
            {
                departmentChangeWorkingWeeks.AddRange(_organizationContext.WorkingWeekInfo
                       .Where(ww => ww.StartDate > start && ww.StartDate <= end)
                       .ToList()
                       .SelectMany(ww => notExitChangeWorkingWeekIds
                           .Select(departmentId => new DepartmentWorkingWeekInfo
                           {
                               DepartmentId = departmentId,
                               DayOfWeek = ww.DayOfWeek,
                               IsDayOff = ww.IsDayOff,
                               StartDate = ww.StartDate,
                               SubsidiaryId = ww.SubsidiaryId
                           }))
                       .ToList());
            }

            var lstDayOff = new List<DepartmentDayOffCalendarModel>();
            for (var day = start; day <= end; day = day.AddDays(1))
            {
                foreach (var departmentId in departmentIds)
                {
                    var clientDay = day.AddMinutes(-_currentContext.TimeZoneOffset.GetValueOrDefault());
                    if (departmentDayOffCalendar.Any(dof => dof.Day == day && dof.DepartmentId == departmentId)) continue;
                    var workingWeek = departmentChangeWorkingWeeks
                        .Where(w => w.DayOfWeek == (int)clientDay.DayOfWeek && w.StartDate <= day && w.DepartmentId == departmentId)
                        .OrderByDescending(w => w.StartDate)
                        .FirstOrDefault();
                    if (workingWeek == null) workingWeek = departmentWorkingWeeks
                            .Where(w => w.DayOfWeek == (int)clientDay.DayOfWeek && w.DepartmentId == departmentId)
                            .FirstOrDefault();
                    if (workingWeek?.IsDayOff ?? false)
                    {
                        lstDayOff.Add(new DepartmentDayOffCalendarModel
                        {
                            DepartmentId = departmentId,
                            Day = day.GetUnix(),
                            Content = "Nghỉ làm cố định trong tuần"
                        });
                    }
                }

            }

            foreach (var dayOff in departmentDayOffCalendar)
            {
                lstDayOff.Add(_mapper.Map<DepartmentDayOffCalendarModel>(dayOff));
            }

            // Lấy thông tin làm thêm giờ
            var deparmentOverHourInfos = await _organizationContext.DepartmentOverHourInfo
               .Where(oh => departmentIds.Contains(oh.DepartmentId))
               .ProjectTo<DepartmentOverHourInfoModel>(_mapper.ConfigurationProvider)
               .ToListAsync();


            var result = new List<DepartmentCalendarListModel>();
            foreach (var departmentId in departmentIds)
            {
                var workingHourInfo = departmentWorkingHourInfo.Where(wh => wh.DepartmentId == departmentId).ToList();
                workingHourInfo.AddRange(departmentChangeWorkingHours.Where(wh => wh.DepartmentId == departmentId).ToList());

                var departmentCalender = new DepartmentCalendarListModel
                {
                    DepartmentId = departmentId,
                    DepartmentWorkingHourInfo = workingHourInfo,
                    DepartmentDayOffCalendar = lstDayOff.Where(dof => dof.DepartmentId == departmentId).ToList(),
                    DepartmentOverHourInfo = deparmentOverHourInfos.Where(oh => oh.DepartmentId == departmentId).ToList()
                };
                result.Add(departmentCalender);
            }

            return result;
        }

        public async Task<IList<DepartmentDayOffCalendarModel>> GetDepartmentDayOffCalendar(int departmentId, long startDate, long endDate)
        {
            var start = startDate.UnixToDateTime().Value;
            var end = endDate.UnixToDateTime().Value;

            var departmentDayOffCalendar = await _organizationContext.DepartmentDayOffCalendar
                .Where(dof => dof.Day >= start && dof.Day <= end && dof.DepartmentId == departmentId)
                .ToListAsync();

            var lstDay = departmentDayOffCalendar.Select(dof => dof.Day).ToList();
            var dayOffCalendar = await _organizationContext.DayOffCalendar
                .Where(dof => dof.Day >= start && dof.Day <= end && !lstDay.Contains(dof.Day))
                .Select(dof => new DepartmentDayOffCalendar
                {
                    DepartmentId = departmentId,
                    Day = dof.Day,
                    Content = dof.Content,
                    SubsidiaryId = dof.SubsidiaryId
                })
                .ToListAsync();

            bool isWorkingWeekCompany = false;
            // Lấy thông tin ngày làm việc trong tuần từ ngày bắt đầu
            var departmentWorkingWeeks = await _organizationContext.DepartmentWorkingWeekInfo
              .Where(ww => ww.StartDate <= start && ww.DepartmentId == departmentId)
              .GroupBy(ww => ww.DayOfWeek)
              .Select(g => new
              {
                  DayOfWeek = g.Key,
                  StartDate = g.Max(ww => ww.StartDate)
              })
              .Join(_organizationContext.DepartmentWorkingWeekInfo, w => new { w.StartDate, w.DayOfWeek, DepartmentId = departmentId }, ww => new { ww.StartDate, ww.DayOfWeek, ww.DepartmentId }, (w, ww) => ww)
              .ToListAsync();

            if (departmentWorkingWeeks.Count == 0)
            {
                isWorkingWeekCompany = true;
                departmentWorkingWeeks = await _organizationContext.WorkingWeekInfo
                    .Where(ww => ww.StartDate <= start)
                    .GroupBy(ww => ww.DayOfWeek)
                    .Select(g => new
                    {
                        DayOfWeek = g.Key,
                        StartDate = g.Max(ww => ww.StartDate)
                    })
                    .Join(_organizationContext.WorkingWeekInfo, w => new { w.StartDate, w.DayOfWeek }, ww => new { ww.StartDate, ww.DayOfWeek }, (w, ww) => new DepartmentWorkingWeekInfo
                    {
                        DepartmentId = departmentId,
                        DayOfWeek = ww.DayOfWeek,
                        IsDayOff = ww.IsDayOff,
                        StartDate = ww.StartDate,
                        SubsidiaryId = ww.SubsidiaryId
                    })
                    .ToListAsync();
            }

            foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
            {
                if (!departmentWorkingWeeks.Any(d => d.DayOfWeek == (int)day))
                {
                    departmentWorkingWeeks.Add(new DepartmentWorkingWeekInfo
                    {
                        DepartmentId = departmentId,
                        DayOfWeek = (int)day,
                        IsDayOff = false,
                        StartDate = start,
                        SubsidiaryId = _currentContext.SubsidiaryId
                    });
                }
            }

            // Lấy thông tin thay đổi trong khoảng thời gian
            var departmentChangeWorkingWeeks = await _organizationContext.DepartmentWorkingWeekInfo
                .Where(ww => ww.StartDate > start && ww.StartDate <= end && ww.DepartmentId == departmentId)
                .ToListAsync();

            if (isWorkingWeekCompany && departmentChangeWorkingWeeks.Count == 0)
            {
                departmentChangeWorkingWeeks = await _organizationContext.WorkingWeekInfo
                    .Where(ww => ww.StartDate > start && ww.StartDate <= end)
                    .Select(ww => new DepartmentWorkingWeekInfo
                    {
                        DepartmentId = departmentId,
                        DayOfWeek = ww.DayOfWeek,
                        IsDayOff = ww.IsDayOff,
                        StartDate = ww.StartDate,
                        SubsidiaryId = ww.SubsidiaryId
                    })
                    .ToListAsync();
            }

            var lstDayOff = new List<DepartmentDayOffCalendarModel>();
            for (var day = start; day <= end; day = day.AddDays(1))
            {
                var clientDay = day.AddMinutes(-_currentContext.TimeZoneOffset.GetValueOrDefault());
                if (departmentDayOffCalendar.Any(dof => dof.Day == day)) continue;
                var workingWeek = departmentChangeWorkingWeeks.Where(w => w.DayOfWeek == (int)clientDay.DayOfWeek && w.StartDate <= day).OrderByDescending(w => w.StartDate).FirstOrDefault();
                if (workingWeek == null) workingWeek = departmentWorkingWeeks.Where(w => w.DayOfWeek == (int)clientDay.DayOfWeek).FirstOrDefault();
                if (workingWeek?.IsDayOff ?? false)
                {
                    lstDayOff.Add(new DepartmentDayOffCalendarModel
                    {
                        DepartmentId = departmentId,
                        Day = day.GetUnix(),
                        Content = "Nghỉ làm cố định trong tuần"
                    });
                }
            }

            foreach (var dayOff in departmentDayOffCalendar)
            {
                lstDayOff.Add(_mapper.Map<DepartmentDayOffCalendarModel>(dayOff));
            }

            return lstDayOff.OrderBy(d => d.Day).ToList();
        }

        public async Task<DepartmentDayOffCalendarModel> UpdateDepartmentDayOff(int departmentId, DepartmentDayOffCalendarModel data)
        {
            try
            {
                var department = _organizationContext.Department.FirstOrDefault(d => d.DepartmentId == departmentId);
                if (department == null) throw new BadRequestException(GeneralCode.ItemNotFound, "Bộ phận không tồn tại");
                var dayOff = await _organizationContext.DepartmentDayOffCalendar
                .FirstOrDefaultAsync(dof => dof.Day == data.Day.UnixToDateTime() && dof.DepartmentId == departmentId);
                if (dayOff == null)
                {
                    dayOff = _mapper.Map<DepartmentDayOffCalendar>(data);
                    _organizationContext.DepartmentDayOffCalendar.Add(dayOff);
                }
                else if (dayOff.Content != data.Content)
                {
                    dayOff.Content = data.Content;
                }
                _organizationContext.SaveChanges();

                await _activityLogService.CreateLog(EnumObjectType.DepartmentDayOffCalendar, data.Day, $"Cập nhật ngày nghỉ {data.Day.UnixToDateTime()} cho bộ phận {department.DepartmentName}", data.JsonSerialize());
                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateDepartmentDayOffCalendar");
                throw;
            }
        }

        public async Task<bool> DeleteDepartmentDayOff(int departmentId, long day)
        {
            try
            {
                var department = _organizationContext.Department.FirstOrDefault(d => d.DepartmentId == departmentId);
                if (department == null) throw new BadRequestException(GeneralCode.ItemNotFound, "Bộ phận không tồn tại");

                var dayOff = await _organizationContext.DepartmentDayOffCalendar
                .FirstOrDefaultAsync(dof => dof.Day == day.UnixToDateTime() && dof.DepartmentId == departmentId);

                if (dayOff == null) throw new BadRequestException(GeneralCode.ItemNotFound, "Ngày nghỉ không tồn tại hoặc là ngày nghỉ chung của công ty");

                _organizationContext.DepartmentDayOffCalendar.Remove(dayOff);
                _organizationContext.SaveChanges();
                await _activityLogService.CreateLog(EnumObjectType.DepartmentDayOffCalendar, day, $"Xóa ngày nghỉ {day.UnixToDateTime()} cho bộ phận {department.DepartmentName}", dayOff.JsonSerialize());
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeleteDepartmentDayOffCalendar");
                throw;
            }
        }

        public async Task<DepartmentWeekCalendarModel> UpdateDepartmentWeekCalendar(int departmentId, DepartmentWeekCalendarModel data)
        {
            var department = _organizationContext.Department.FirstOrDefault(d => d.DepartmentId == departmentId);
            if (department == null) throw new BadRequestException(GeneralCode.ItemNotFound, "Bộ phận không tồn tại");

            using var trans = await _organizationContext.Database.BeginTransactionAsync();
            try
            {
                DateTime now = DateTime.UtcNow.Date;

                // Update workingHour per day
                var currentWorkingHourInfo = await _organizationContext.DepartmentWorkingHourInfo
                  .Where(wh => wh.StartDate <= now && wh.DepartmentId == departmentId)
                  .OrderByDescending(wh => wh.StartDate)
                  .FirstOrDefaultAsync();

                if (currentWorkingHourInfo == null)
                {
                    currentWorkingHourInfo = new DepartmentWorkingHourInfo
                    {
                        DepartmentId = departmentId,
                        StartDate = now,
                        WorkingHourPerDay = data.WorkingHourPerDay
                    };
                    _organizationContext.DepartmentWorkingHourInfo.Add(currentWorkingHourInfo);
                }
                else if (currentWorkingHourInfo.WorkingHourPerDay != data.WorkingHourPerDay)
                {
                    if (currentWorkingHourInfo.StartDate < now)
                    {
                        currentWorkingHourInfo = new DepartmentWorkingHourInfo
                        {
                            DepartmentId = departmentId,
                            StartDate = now,
                            WorkingHourPerDay = data.WorkingHourPerDay
                        };
                        _organizationContext.DepartmentWorkingHourInfo.Add(currentWorkingHourInfo);
                    }
                    else
                    {
                        currentWorkingHourInfo.WorkingHourPerDay = data.WorkingHourPerDay;
                    }
                }

                // Update working week

                var workingWeeks = await _organizationContext.DepartmentWorkingWeekInfo
                    .Where(ww => ww.StartDate <= now && ww.DepartmentId == departmentId)
                    .GroupBy(ww => ww.DayOfWeek)
                    .Select(g => new
                    {
                        DayOfWeek = g.Key,
                        StartDate = g.Max(ww => ww.StartDate)
                    })
                    .Join(_organizationContext.DepartmentWorkingWeekInfo, w => new { w.StartDate, w.DayOfWeek, DepartmentId = departmentId }, ww => new { ww.StartDate, ww.DayOfWeek, ww.DepartmentId }, (w, ww) => ww)
                    .ToListAsync();

                foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
                {
                    var newWorkingWeek = data.DepartmentWorkingWeek.FirstOrDefault(w => w.DayOfWeek == day);
                    if (newWorkingWeek == null) continue;
                    var currentWorkingWeek = workingWeeks.FirstOrDefault(ww => ww.DayOfWeek == (int)day);
                    if (currentWorkingWeek == null)
                    {
                        currentWorkingWeek = new DepartmentWorkingWeekInfo
                        {
                            DepartmentId = departmentId,
                            StartDate = now,
                            DayOfWeek = (int)day,
                            IsDayOff = newWorkingWeek.IsDayOff
                        };
                        _organizationContext.DepartmentWorkingWeekInfo.Add(currentWorkingWeek);
                    }
                    else if (currentWorkingWeek.IsDayOff != newWorkingWeek.IsDayOff)
                    {
                        if (currentWorkingWeek.StartDate < now)
                        {
                            currentWorkingWeek = new DepartmentWorkingWeekInfo
                            {
                                DepartmentId = departmentId,
                                StartDate = now,
                                DayOfWeek = (int)day,
                                IsDayOff = newWorkingWeek.IsDayOff
                            };
                            _organizationContext.DepartmentWorkingWeekInfo.Add(currentWorkingWeek);
                        }
                        else
                        {
                            currentWorkingWeek.IsDayOff = newWorkingWeek.IsDayOff;
                        }
                    }
                }

                _organizationContext.SaveChanges();
                trans.Commit();

                await _activityLogService.CreateLog(EnumObjectType.DepartmentCalendar, now.GetUnix(), $"Cập nhật lịch làm việc ngày {now.ToString()} cho bộ phận {department.DepartmentName}", data.JsonSerialize());

                return await GetCurrentDepartmentCalendar(departmentId);
            }
            catch (Exception ex)
            {
                trans.TryRollbackTransaction();
                _logger.LogError(ex, "UpdateDepartmentCalendar");
                throw;
            }
        }

        public async Task<IList<DepartmentOverHourInfoModel>> GetDepartmentOverHourInfo(int departmentId)
        {
            var result = await _organizationContext.DepartmentOverHourInfo
                .Where(oh => oh.DepartmentId == departmentId)
                .ProjectTo<DepartmentOverHourInfoModel>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return result;
        }

        public async Task<DepartmentOverHourInfoModel> CreateDepartmentOverHourInfo(int departmentId, DepartmentOverHourInfoModel data)
        {
            try
            {
                var department = _organizationContext.Department.FirstOrDefault(d => d.DepartmentId == departmentId);
                if (department == null) throw new BadRequestException(GeneralCode.ItemNotFound, "Bộ phận không tồn tại");

                if (_organizationContext.DepartmentOverHourInfo.Any(oh => oh.StartDate <= data.EndDate.UnixToDateTime() && oh.EndDate <= data.StartDate.UnixToDateTime() && oh.DepartmentId == departmentId))
                    throw new BadRequestException(GeneralCode.InvalidParams, "Trùng khoảng thời gian với giai đoạn đã tồn tại");

                var overHour = _mapper.Map<DepartmentOverHourInfo>(data);
                overHour.DepartmentId = departmentId;
                _organizationContext.DepartmentOverHourInfo.Add(overHour);
                _organizationContext.SaveChanges();

                await _activityLogService.CreateLog(EnumObjectType.DepartmentOverHour, overHour.DepartmentOverHourInfoId, $"Thêm mới lịch tăng ca bộ phận {department.DepartmentName}", data.JsonSerialize());
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
                if (department == null) throw new BadRequestException(GeneralCode.ItemNotFound, "Bộ phận không tồn tại");

                var overHour = _organizationContext.DepartmentOverHourInfo.FirstOrDefault(oh => oh.DepartmentOverHourInfoId == departmentOverHourInfoId && oh.DepartmentId == departmentId);
                if (overHour == null) throw new BadRequestException(GeneralCode.ItemNotFound, "Thông tin tăng ca không tồn tại");

                if (_organizationContext.DepartmentOverHourInfo.Any(oh => oh.DepartmentOverHourInfoId != departmentOverHourInfoId && oh.StartDate <= data.EndDate.UnixToDateTime() && oh.EndDate <= data.StartDate.UnixToDateTime() && oh.DepartmentId == departmentId))
                    throw new BadRequestException(GeneralCode.InvalidParams, "Trùng khoảng thời gian với giai đoạn đã tồn tại");

                _mapper.Map(data, overHour);

                overHour.DepartmentId = departmentId;

                _organizationContext.SaveChanges();

                await _activityLogService.CreateLog(EnumObjectType.DepartmentOverHour, overHour.DepartmentOverHourInfoId, $"Cập nhật lịch tăng ca bộ phận {department.DepartmentName}", data.JsonSerialize());
                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateDepartmentOverHour");
                throw;
            }
        }

        public async Task<bool> DeleteDepartmentOverHourInfo(int departmentId, long departmentOverHourInfoId)
        {
            try
            {
                var department = _organizationContext.Department.FirstOrDefault(d => d.DepartmentId == departmentId);
                if (department == null) throw new BadRequestException(GeneralCode.ItemNotFound, "Bộ phận không tồn tại");

                var overHour = _organizationContext.DepartmentOverHourInfo.FirstOrDefault(oh => oh.DepartmentOverHourInfoId == departmentOverHourInfoId && oh.DepartmentId == departmentId);
                if (overHour == null) throw new BadRequestException(GeneralCode.ItemNotFound, "Thông tin tăng ca không tồn tại");

                _organizationContext.DepartmentOverHourInfo.Remove(overHour);
                _organizationContext.SaveChanges();
                await _activityLogService.CreateLog(EnumObjectType.DepartmentOverHour, departmentOverHourInfoId, $"Xóa thông tin tăng ca cho bộ phận {department.DepartmentName}", overHour.JsonSerialize());
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeleteDepartmentOverHour");
                throw;
            }
        }
    }
}
