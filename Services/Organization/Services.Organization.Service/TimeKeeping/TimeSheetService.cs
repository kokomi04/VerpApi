using AutoMapper;
using AutoMapper.QueryableExtensions;
using DocumentFormat.OpenXml.Spreadsheet;
using Elasticsearch.Net;
using Microsoft.EntityFrameworkCore;
using OpenXmlPowerTools;
using Services.Organization.Model.TimeKeeping;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Verp.Resources.GlobalObject;
using VErp.Commons.Constants;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.Organization.TimeKeeping;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.OrganizationDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Organization.Model.Calendar;
using VErp.Services.Organization.Service.Department;
using VErp.Services.Organization.Service.DepartmentCalendar;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static VErp.Commons.Library.ExcelReader;

namespace VErp.Services.Organization.Service.TimeKeeping
{
    public interface ITimeSheetService
    {
        Task<long> AddTimeSheet(TimeSheetModel model);
        Task<bool> DeleteTimeSheet(long timeSheetId);
        Task<PageData<TimeSheetModel>> GetListTimeSheet(TimeSheetFilterModel filter, int page, int size);
        Task<TimeSheetModel> GetTimeSheet(long timeSheetId);
        Task<TimeSheetModel> GetTimeSheetByEmployee(long timeSheetId, int employeeId);
        Task<bool> UpdateTimeSheet(long timeSheetId, TimeSheetModel model);
        Task<List<TimeSheetByEmployeeModel>> GenerateTimeSheet(long timeSheetId, int[] departmentIds, long beginDate, long endDate, bool ignoreOvertimePlan);
        Task<TimeSheetDetailModel> SingleTimeKeeping(TimeSheetDetailRequestModel model);

        //CategoryNameModel GetFieldDataForMapping(long beginDate, long endDate);

        //Task<bool> ImportTimeSheetFromMapping(int month, int year, long beginDate, long endDate, ImportExcelMapping mapping, Stream stream);

        Task<bool> ApproveTimeSheet(long timeSheetId);
    }

    public class TimeSheetService : ITimeSheetService
    {
        private readonly OrganizationDBContext _organizationDBContext;
        private readonly IDepartmentCalendarService _departmentCalendarService;
        private readonly IDepartmentService _departmentService;
        private readonly IShiftScheduleService _shiftScheduleService;
        private readonly ITimeSheetRawService _timeSheetRawService;
        private readonly IOvertimePlanService _overtimePlanService;
        private readonly IMapper _mapper;

        public TimeSheetService(OrganizationDBContext organizationDBContext
            , IMapper mapper
            , IDepartmentCalendarService departmentCalendarService
            , IShiftScheduleService shiftScheduleService
            , ITimeSheetRawService timeSheetRawService
            , IOvertimePlanService overtimePlanService)
        {
            _organizationDBContext = organizationDBContext;
            _mapper = mapper;
            _departmentCalendarService = departmentCalendarService;
            _shiftScheduleService = shiftScheduleService;
            _timeSheetRawService = timeSheetRawService;
            _overtimePlanService = overtimePlanService;
        }

        public async Task<long> AddTimeSheet(TimeSheetModel model)
        {
            var trans = await _organizationDBContext.Database.BeginTransactionAsync();
            try
            {
                if (_organizationDBContext.TimeSheet.Any(t => t.Title == model.Title))
                {
                    throw new BadRequestException("Tên bảng chấm công đã tồn tại");
                }

                await ValidateOverlap(model);

                model.IsApprove = false;

                var entity = _mapper.Map<TimeSheet>(model);

                foreach (var item in model.TimeSheetDetail)
                {
                    item.TimeSheetDetailId = 0;
                    entity.TimeSheetDetail.Add(_mapper.Map<TimeSheetDetail>(item));
                }

                foreach (var item in model.TimeSheetAggregate)
                {
                    item.TimeSheetAggregateId = 0;
                    entity.TimeSheetAggregate.Add(_mapper.Map<TimeSheetAggregate>(item));
                }

                await _organizationDBContext.TimeSheet.AddAsync(entity);
                await _organizationDBContext.SaveChangesAsync();

                await trans.CommitAsync();

                return entity.TimeSheetId;
            }
            catch (Exception)
            {
                await trans.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> UpdateTimeSheet(long timeSheetId, TimeSheetModel model)
        {
            var trans = await _organizationDBContext.Database.BeginTransactionAsync();
            try
            {
                var timeSheet = await _organizationDBContext.TimeSheet.AsNoTracking().FirstOrDefaultAsync(x => x.TimeSheetId == timeSheetId);
                if (timeSheet == null)
                    throw new BadRequestException(GeneralCode.ItemNotFound, $"Không tìm thấy bảng chấm công có ID {timeSheetId}");

                if (_organizationDBContext.TimeSheet.Any(t => t.Title != timeSheet.Title && t.Title == model.Title))
                    throw new BadRequestException("Tên bảng chấm công đã tồn tại");

                await ValidateOverlap(model);

                var timeSheetDetails = await _organizationDBContext.TimeSheetDetail.Where(x => x.TimeSheetId == timeSheet.TimeSheetId).ToListAsync();
                var timeSheetAggregates = await _organizationDBContext.TimeSheetAggregate.Where(x => x.TimeSheetId == timeSheet.TimeSheetId).ToListAsync();

                await RemoveTimeSheetDepartment(timeSheetId);

                var eTimeSheetDPMs = new List<TimeSheetDepartment>();
                _mapper.Map(model.TimeSheetDepartment, eTimeSheetDPMs);
                _organizationDBContext.TimeSheetDepartment.AddRange(eTimeSheetDPMs);

                var eDetailSet = new HashSet<(long EmployeeId, long Date)>(timeSheetDetails.Select(e => (e.EmployeeId, e.Date.GetUnix())));
                var modelDetailSet = new HashSet<(long EmployeeId, long Date)>(model.TimeSheetDetail.Select(m => (m.EmployeeId, m.Date)));

                var detailsToRemove = timeSheetDetails.Where(e => !modelDetailSet.Contains((e.EmployeeId, e.Date.GetUnix()))).ToList();
                foreach (var detail in detailsToRemove)
                {
                    detail.IsDeleted = true;
                }

                var timeSheetDetailIds = timeSheetDetails.Where(d => modelDetailSet.Contains((d.EmployeeId, d.Date.GetUnix()))).Select(d => d.TimeSheetDetailId).ToList();

                await RemoveTimeSheetDetailShift(timeSheetDetailIds);

                var newDetails = new List<TimeSheetDetail>();

                foreach (var mDetail in model.TimeSheetDetail)
                {
                    mDetail.TimeSheetId = timeSheetId;

                    if (eDetailSet.Contains((mDetail.EmployeeId, mDetail.Date)))
                    {
                        var eDetailToUpdate = timeSheetDetails.FirstOrDefault(e => e.EmployeeId == mDetail.EmployeeId && e.Date.GetUnix() == mDetail.Date);
                        mDetail.TimeSheetDetailId = eDetailToUpdate.TimeSheetDetailId;

                        var counteds = new List<TimeSheetDetailShiftCountedModel>();

                        if (mDetail.TimeSheetDetailShift.Any(ds => ds.TimeSheetDetailShiftCounted.Count > 0))
                        {
                            foreach (var ds in mDetail.TimeSheetDetailShift)
                            {
                                foreach (var counted in ds.TimeSheetDetailShiftCounted)
                                {
                                    counted.TimeSheetDetailId = eDetailToUpdate.TimeSheetDetailId;
                                    counted.TimeSheetDetailShiftCountedId = 0;
                                    counteds.Add(counted);
                                }
                                ds.TimeSheetDetailShiftCounted = null;
                            }
                        }

                        _mapper.Map(mDetail, eDetailToUpdate);

                        var eCounted = new List<TimeSheetDetailShiftCounted>();
                        await _organizationDBContext.TimeSheetDetailShiftCounted.AddRangeAsync(_mapper.Map(counteds, eCounted));
                        //await _organizationDBContext.SaveChangesAsync();
                    }
                    else
                    {
                        mDetail.TimeSheetDetailId = 0;
                        foreach (var ds in mDetail.TimeSheetDetailShift)
                        {
                            foreach (var counted in ds.TimeSheetDetailShiftCounted)
                            {
                                counted.TimeSheetDetailShiftCountedId = 0;
                            }
                        }
                        var newEDetail = _mapper.Map<TimeSheetDetail>(mDetail);
                        newDetails.Add(newEDetail);
                    }
                }
                await _organizationDBContext.TimeSheetDetail.AddRangeAsync(newDetails);
                //await _organizationDBContext.SaveChangesAsync();

                var eAggregateIds = new HashSet<long>(timeSheetAggregates.Select(e => e.TimeSheetAggregateId));
                var modelAggregateIds = new HashSet<long>(model.TimeSheetAggregate.Select(m => m.TimeSheetAggregateId));

                var aggregatesToRemove = timeSheetAggregates.Where(e => !modelAggregateIds.Contains(e.TimeSheetAggregateId)).ToList();
                foreach (var aggregate in aggregatesToRemove)
                {
                    aggregate.IsDeleted = true;
                }

                var newAggregates = new List<TimeSheetAggregate>();

                var timeSheetAggregateIds = model.TimeSheetAggregate.Where(a => eAggregateIds.Contains(a.TimeSheetAggregateId)).Select(d => d.TimeSheetAggregateId).ToList();

                await RemoveTimeSheetAggregateAbsence(timeSheetAggregateIds);
                await RemoveTimeSheetAggregateOvertime(timeSheetAggregateIds);

                foreach (var mAggregate in model.TimeSheetAggregate)
                {
                    mAggregate.TimeSheetId = timeSheetId;

                    if (eAggregateIds.Contains(mAggregate.TimeSheetAggregateId))
                    {
                        var eAggregateToUpdate = timeSheetAggregates.FirstOrDefault(e => e.TimeSheetAggregateId == mAggregate.TimeSheetAggregateId);

                        mAggregate.TimeSheetId = eAggregateToUpdate.TimeSheetId;
                        _mapper.Map(mAggregate, eAggregateToUpdate);
                    }
                    else
                    {
                        mAggregate.TimeSheetAggregateId = 0;
                        var newEAggregate = _mapper.Map<TimeSheetAggregate>(mAggregate);
                        newAggregates.Add(newEAggregate);
                    }
                }
                await _organizationDBContext.TimeSheetAggregate.AddRangeAsync(newAggregates);

                model.TimeSheetId = timeSheet.TimeSheetId;
                model.TimeSheetDepartment = null;
                model.IsApprove = false;
                _mapper.Map(model, timeSheet);

                _organizationDBContext.TimeSheet.Update(timeSheet);

                await _organizationDBContext.SaveChangesAsync();

                await trans.CommitAsync();

                return true;

            }
            catch (Exception ex)
            {
                await trans.RollbackAsync();
                throw new BadRequestException(ex.Message);
            }
        }

        public async Task<bool> DeleteTimeSheet(long timeSheetId)
        {
            var trans = await _organizationDBContext.Database.BeginTransactionAsync();
            try
            {
                var timeSheet = await _organizationDBContext.TimeSheet.FirstOrDefaultAsync(x => x.TimeSheetId == timeSheetId);
                if (timeSheet == null)
                    throw new BadRequestException(GeneralCode.ItemNotFound, $"Không tồn tại bảng chấm công có ID {timeSheetId}");

                timeSheet.IsDeleted = true;
                _organizationDBContext.TimeSheetDetail.Where(x => x.TimeSheetId == timeSheet.TimeSheetId).ToList().ForEach(x => x.IsDeleted = true);
                _organizationDBContext.TimeSheetAggregate.Where(x => x.TimeSheetId == timeSheet.TimeSheetId).ToList().ForEach(x => x.IsDeleted = true);

                await RemoveTimeSheetDepartment(timeSheetId);

                var timeSheetDetailIds = timeSheet.TimeSheetDetail.Select(d => d.TimeSheetDetailId).ToList();
                var timeSheetAggregateIds = timeSheet.TimeSheetAggregate.Select(d => d.TimeSheetAggregateId).ToList();

                await RemoveTimeSheetDetailShift(timeSheetDetailIds);
                await RemoveTimeSheetAggregateAbsence(timeSheetAggregateIds);
                await RemoveTimeSheetAggregateOvertime(timeSheetAggregateIds);

                await _organizationDBContext.SaveChangesAsync();

                await trans.CommitAsync();

                return true;

            }
            catch (Exception)
            {
                await trans.RollbackAsync();
                throw;
            }
        }

        public async Task<TimeSheetModel> GetTimeSheet(long timeSheetId)
        {
            var timeSheet = await _organizationDBContext.TimeSheet
                .Include(t => t.TimeSheetDepartment)
                .FirstOrDefaultAsync(x => x.TimeSheetId == timeSheetId);
            if (timeSheet == null)
                throw new BadRequestException(GeneralCode.ItemNotFound, $"Không tồn tại bảng chấm công có ID {timeSheetId}");


            var timeSheetDetails = await _organizationDBContext.TimeSheetDetail.Where(x => x.TimeSheetId == timeSheetId).ProjectTo<TimeSheetDetailModel>(_mapper.ConfigurationProvider).ToListAsync();
            var timeSheetAggregates = await _organizationDBContext.TimeSheetAggregate.Where(x => x.TimeSheetId == timeSheetId).ProjectTo<TimeSheetAggregateModel>(_mapper.ConfigurationProvider).ToListAsync();

            var result = _mapper.Map<TimeSheetModel>(timeSheet);
            result.TimeSheetDetail = timeSheetDetails;
            result.TimeSheetAggregate = timeSheetAggregates;

            return result;
        }

        public async Task<TimeSheetModel> GetTimeSheetByEmployee(long timeSheetId, int employeeId)
        {
            var timeSheet = await _organizationDBContext.TimeSheet
            .FirstOrDefaultAsync(x => x.TimeSheetId == timeSheetId);
            if (timeSheet == null)
                throw new BadRequestException(GeneralCode.ItemNotFound, $"Không tồn tại bảng chấm công có ID {timeSheetId}");

            var timeSheetDetails = await _organizationDBContext.TimeSheetDetail.Where(x => x.TimeSheetId == timeSheet.TimeSheetId && x.EmployeeId == employeeId).ProjectTo<TimeSheetDetailModel>(_mapper.ConfigurationProvider).ToListAsync();
            var timeSheetAggregates = await _organizationDBContext.TimeSheetAggregate.Where(x => x.TimeSheetId == timeSheet.TimeSheetId && x.EmployeeId == employeeId).ProjectTo<TimeSheetAggregateModel>(_mapper.ConfigurationProvider).ToListAsync();

            var result = _mapper.Map<TimeSheetModel>(timeSheet);
            result.TimeSheetAggregate = timeSheetAggregates;
            result.TimeSheetDetail = timeSheetDetails;
            return result;
        }

        public async Task<PageData<TimeSheetModel>> GetListTimeSheet(TimeSheetFilterModel filter, int page, int size)
        {
            var query = _organizationDBContext.TimeSheet.Include(x => x.TimeSheetDepartment).AsNoTracking();

            if (!string.IsNullOrWhiteSpace(filter.Keyword))
                query = query.Where(t => t.Title.Contains(filter.Keyword));

            query = query.InternalFilter(filter.ColumnsFilters);

            query = query.InternalOrderBy(filter.OrderBy, filter.Asc);

            //if (filter.DepartmentIds != null && filter.DepartmentIds.Count > 0)
            //{

            //}

            var total = query.Count();

            query = size > 0 && page > 0 ? query.Skip((page - 1) * size).Take(size) : query;

            var data = new List<TimeSheetModel>();
            _mapper.Map(query, data).ToList();

            return (data, total);
        }

        public async Task<TimeSheetDetailModel> SingleTimeKeeping(TimeSheetDetailRequestModel model)
        {
            async Task<EnumOvertimeMode> GetOvertimeMode(TimeSheetDetailShiftModel detailShift)
            {
                var scheduleDetail = await _organizationDBContext.ShiftScheduleDetail
                          .FirstOrDefaultAsync(sd => sd.ShiftConfigurationId == detailShift.ShiftConfigurationId && sd.AssignedDate == model.TimeSheetDetail.Date.UnixToDateTime() && sd.EmployeeId == model.TimeSheetDetail.EmployeeId);
                var schedule = await _organizationDBContext.ShiftSchedule.FirstOrDefaultAsync(s => s.ShiftScheduleId == scheduleDetail.ShiftScheduleId);

                return (EnumOvertimeMode)schedule.OvertimeMode;
            }

            var absences = await _organizationDBContext.AbsenceTypeSymbol.Where(a => a.IsUsed).ProjectTo<AbsenceTypeSymbolModel>(_mapper.ConfigurationProvider).ToListAsync();
            var countedSymbols = await _organizationDBContext.CountedSymbol.ProjectTo<CountedSymbolModel>(_mapper.ConfigurationProvider).ToListAsync();

            var shiftIds = model.TimeSheetDetail.TimeSheetDetailShift.Select(ds => ds.ShiftConfigurationId).ToList();
            var shifts = await _organizationDBContext.ShiftConfiguration
                .Where(s => shiftIds.Contains(s.ShiftConfigurationId))
                .ProjectTo<ShiftConfigurationModel>(_mapper.ConfigurationProvider)
                .ToListAsync();

            var overtimePlans = await _overtimePlanService.GetListOvertimePlan(new OvertimePlanRequestModel
            {
                OvertimePlans = new List<OvertimePlanModel>(),
                FromDate = model.TimeSheetDetail.Date,
                ToDate = model.TimeSheetDetail.Date,
                EmployeeIds = new List<long>() { model.TimeSheetDetail.EmployeeId }
            });

            var lstDetailShift = new List<TimeSheetDetailShiftModel>();

            switch (model.TimeSheetDetail.TimeSheetMode)
            {
                case EnumTimeSheetMode.ByCheckinCheckoutInDay:
                    var shiftsWithoutNight = shifts.Where(s => !s.IsNightShift).ToList();

                    var earliestShift = shiftsWithoutNight.OrderBy(s => s.EntryTime).FirstOrDefault(shift => model.TimeIn.HasValue && model.TimeIn >= shift.StartTimeOnRecord && model.TimeIn <= shift.EndTimeOnRecord);
                    var lastestShift = shiftsWithoutNight.OrderBy(s => s.ExitTime).FirstOrDefault(shift => model.TimeOut.HasValue && model.TimeOut >= shift.StartTimeOutRecord && model.TimeOut <= shift.EndTimeOutRecord);

                    var timeIn = (model.TimeIn.HasValue && earliestShift != null) ? model.TimeIn : null;
                    var timeOut = (model.TimeOut.HasValue && lastestShift != null) ? model.TimeOut : null;

                    foreach (var shift in shiftsWithoutNight)
                    {
                        var detailShift = model.TimeSheetDetail.TimeSheetDetailShift.FirstOrDefault(s => s.ShiftConfigurationId == shift.ShiftConfigurationId);

                        var overtimeMode = await GetOvertimeMode(detailShift);

                        if (shiftsWithoutNight.Count() == 1 || (earliestShift != null && lastestShift != null && earliestShift.ShiftConfigurationId == shift.ShiftConfigurationId && earliestShift.ShiftConfigurationId == lastestShift.ShiftConfigurationId))
                        {
                            detailShift = CreateDetailShift(shift, model.TimeSheetDetail, timeIn, timeOut, countedSymbols, absences, overtimeMode, overtimePlans, false);
                        }
                        else
                        {
                            if (earliestShift != null && earliestShift.ShiftConfigurationId == shift.ShiftConfigurationId)
                            {
                                detailShift = CreateDetailShift(shift, model.TimeSheetDetail, timeIn, (timeIn.HasValue && timeOut.HasValue) ? shift.ExitTime : null, countedSymbols, absences, overtimeMode, overtimePlans, false);
                            }
                            else if (lastestShift != null && lastestShift.ShiftConfigurationId == shift.ShiftConfigurationId)
                            {
                                detailShift = CreateDetailShift(shift, model.TimeSheetDetail, (timeIn.HasValue && timeOut.HasValue) ? shift.EntryTime : null, timeOut, countedSymbols, absences, overtimeMode, overtimePlans, false);
                            }
                            else if (!timeIn.HasValue || !timeOut.HasValue || shift.EntryTime < earliestShift.EntryTime || shift.ExitTime > lastestShift.ExitTime)
                            {
                                detailShift = CreateDetailShift(shift, model.TimeSheetDetail, null, null, countedSymbols, absences, overtimeMode, overtimePlans, false);
                            }
                            else
                            {
                                detailShift = CreateDetailShift(shift, model.TimeSheetDetail, shift.EntryTime, shift.ExitTime, countedSymbols, absences, overtimeMode, overtimePlans, false);
                            }
                        }

                        if (detailShift != null)
                        {
                            lstDetailShift.Add(detailShift);
                        }
                    }

                    break;

                case EnumTimeSheetMode.ByDayLabour:
                    foreach (var shift in shifts)
                    {
                        var detailShift = new TimeSheetDetailShiftModel()
                        {
                            TimeSheetDetailId = model.TimeSheetDetail.TimeSheetDetailId,
                            WorkCounted = shift.ConfirmationUnit,
                            ActualWorkMins = shift.ConvertToMins,
                            ShiftConfigurationId = shift.ShiftConfigurationId,
                            TimeIn = shift.EntryTime,
                            TimeOut = shift.ExitTime
                        };
                        detailShift.TimeSheetDetailShiftCounted.Add(GetCountedSymbolModel(shift, countedSymbols, shift.IsNightShift ? EnumCountedSymbol.ShiftNightSymbol : EnumCountedSymbol.FullCountedSymbol));

                        lstDetailShift.Add(detailShift);
                    }

                    break;

                case EnumTimeSheetMode.Absence:
                    foreach (var shift in shifts)
                    {
                        var absenceTypeSymbolId = model.TimeSheetDetail.TimeSheetDetailShift
                            .Where(s => s.ShiftConfigurationId == shift.ShiftConfigurationId)
                            .Select(d => d.AbsenceTypeSymbolId).FirstOrDefault();

                        var detailShift = new TimeSheetDetailShiftModel()
                        {
                            TimeSheetDetailId = model.TimeSheetDetail.TimeSheetDetailId,
                            ShiftConfigurationId = shift.ShiftConfigurationId,
                            TimeIn = null,
                            TimeOut = null
                        };
                        SetDetailShiftForAbsence(EnumTimeSheetDateType.Weekday, detailShift, absenceTypeSymbolId, shift, countedSymbols, absences);

                        lstDetailShift.Add(detailShift);
                    }

                    break;

                default:
                    foreach (var shift in shifts)
                    {
                        var detailShift = model.TimeSheetDetail.TimeSheetDetailShift.FirstOrDefault(s => s.ShiftConfigurationId == shift.ShiftConfigurationId);

                        var overtimeMode = await GetOvertimeMode(detailShift);

                        detailShift = CreateDetailShift(shift, model.TimeSheetDetail, detailShift.TimeIn, detailShift.TimeOut, countedSymbols, absences, overtimeMode, overtimePlans, false);

                        if (detailShift != null)
                        {
                            lstDetailShift.Add(detailShift);
                        }
                    }

                    break;
            }

            model.TimeSheetDetail.TimeSheetDetailShift.Clear();
            model.TimeSheetDetail.TimeSheetDetailShift = lstDetailShift;
            return model.TimeSheetDetail;

        }

        public async Task<List<TimeSheetByEmployeeModel>> GenerateTimeSheet(long timeSheetId, int[] departmentIds, long beginDate, long endDate, bool ignoreOvertimePlan)
        {
            var result = new List<TimeSheetByEmployeeModel>();

            var dateRange = new List<long>();
            for (var date = beginDate; date <= endDate; date += 86400)
            {
                dateRange.Add(date);
            }
            var lstEmployees = await _shiftScheduleService.GetEmployeesByDepartments(departmentIds.ToList());
            var employeesByDepartment = lstEmployees.GroupBy(e => (int)e[EmployeeConstants.DEPARTMENT]).ToDictionary(g => g.Key, g => g.ToList());

            var departmentCalendars = await _departmentCalendarService.GetListDepartmentCalendar(departmentIds, beginDate, endDate);

            var dayOffByDepartment = departmentCalendars.GroupBy(x => x.DepartmentId)
                .ToDictionary(g => g.Key, g => g.SelectMany(x => x.DepartmentDayOffCalendar.Where(d => d.Day >= beginDate && d.Day <= endDate)).ToList());

            var allSchedules = _organizationDBContext.ShiftSchedule.AsNoTracking();

            var allScheduleDetails = await _organizationDBContext.ShiftScheduleDetail
                .Where(s => lstEmployees.Select(e => (long)e[EmployeeConstants.EMPLOYEE_ID]).Contains(s.EmployeeId)
                    && dateRange.Select(d => d.UnixToDateTime()).ToList().Contains(s.AssignedDate))
                .ToListAsync();

            var allShiftConfigurations = await _organizationDBContext.ShiftConfiguration
                .Where(s => allScheduleDetails.Select(d => d.ShiftConfigurationId).Distinct().Contains(s.ShiftConfigurationId))
                .ProjectTo<ShiftConfigurationModel>(_mapper.ConfigurationProvider)
                .ToListAsync();

            var overtimePlans = await _overtimePlanService.GetListOvertimePlan(new OvertimePlanRequestModel
            {
                OvertimePlans = new List<OvertimePlanModel>(),
                FromDate = beginDate,
                ToDate = endDate,
                DepartmentIds = departmentIds.ToList()
            });

            var allTimeSheetRaws = await _timeSheetRawService.GetDistinctTimeSheetRawByEmployee(lstEmployees.Select(e => (long?)e[EmployeeConstants.EMPLOYEE_ID]).ToList());

            var countedSymbols = await _organizationDBContext.CountedSymbol.ProjectTo<CountedSymbolModel>(_mapper.ConfigurationProvider).ToListAsync();

            var absences = await _organizationDBContext.AbsenceTypeSymbol.Where(a => a.IsUsed).ProjectTo<AbsenceTypeSymbolModel>(_mapper.ConfigurationProvider).ToListAsync();

            foreach (var (departmentId, employees) in employeesByDepartment)
            {
                foreach (var employee in employees)
                {
                    var employeeId = (long)employee[EmployeeConstants.EMPLOYEE_ID];

                    var timeSheetByEmployee = new TimeSheetByEmployeeModel();
                    timeSheetByEmployee.EmployeeId = employeeId;

                    var details = new List<TimeSheetDetailModel>();

                    foreach (var date in dateRange)
                    {
                        var detail = new TimeSheetDetailModel();
                        detail.EmployeeId = employeeId;
                        detail.Date = date;
                        detail.TimeSheetId = timeSheetId;
                        detail.TimeSheetMode = EnumTimeSheetMode.ByShift;

                        var dayOff = dayOffByDepartment[departmentId].FirstOrDefault(d => d.Day == date);
                        if (dayOff != null)
                        {
                            detail.TimeSheetDateType = (EnumTimeSheetDateType)dayOff.DayOffType;
                        }
                        else
                        {
                            detail.TimeSheetDateType = EnumTimeSheetDateType.Weekday;
                        }

                        var detailShifts = new List<TimeSheetDetailShiftModel>();

                        var shiftsForDate = allScheduleDetails.Where(d => d.EmployeeId == employeeId && d.AssignedDate.GetUnix() == date);

                        if (!shiftsForDate.Any())
                        {
                            detail.IsScheduled = false;
                            details.Add(detail);
                            continue;
                        }

                        detail.IsScheduled = true;

                        var shifts = allShiftConfigurations.Where(s => shiftsForDate.Any(sd => sd.ShiftConfigurationId == s.ShiftConfigurationId)).ToList();


                        foreach (var shift in shifts)
                        {
                            var scheduleDetail = shiftsForDate.FirstOrDefault(sd => sd.ShiftConfigurationId == shift.ShiftConfigurationId && sd.AssignedDate == date.UnixToDateTime() && sd.EmployeeId == employeeId);

                            var schedule = await allSchedules.FirstOrDefaultAsync(s => s.ShiftScheduleId == scheduleDetail.ShiftScheduleId);

                            IEnumerable<TimeSheetRawModel> timeSheetRaw;

                            timeSheetRaw = shift.IsNightShift ?
                                        allTimeSheetRaws.Where(r => r.EmployeeId == employeeId && (r.Date == date || r.Date == ((bool)shift.IsCheckOutDateTimekeeping ? date - 86400 : date + 86400))) :
                                        allTimeSheetRaws.Where(r => r.EmployeeId == employeeId && r.Date == date);

                            double? timeInRaw = null;
                            double? timeOutRaw = null;

                            if (timeSheetRaw != null && timeSheetRaw.Any())
                            {
                                var dateIn = shift.IsNightShift && (bool)shift.IsCheckOutDateTimekeeping ? detail.Date - 86400 : detail.Date;
                                var dateOut = shift.IsNightShift && !(bool)shift.IsCheckOutDateTimekeeping ? detail.Date + 86400 : detail.Date;

                                var timeInRaws = timeSheetRaw.Where(r => r.Date == dateIn && r.Time >= shift.StartTimeOnRecord && r.Time <= shift.EndTimeOnRecord).ToList();
                                var timeOutRaws = timeSheetRaw.Where(r => r.Date == dateOut && r.Time >= shift.StartTimeOutRecord && r.Time <= shift.EndTimeOutRecord).ToList();

                                timeInRaw = timeInRaws.Any() ? timeInRaws.Min(r => r.Time) : null;
                                timeOutRaw = timeOutRaws.Any() ? timeOutRaws.Max(r => r.Time) : null;
                            }


                            var detailShift = CreateDetailShift(shift, detail, timeInRaw, timeOutRaw, countedSymbols, absences, (EnumOvertimeMode)schedule.OvertimeMode, overtimePlans, ignoreOvertimePlan);

                            detailShifts.Add(detailShift);
                        }
                        detail.TimeSheetDetailShift = detailShifts;
                        details.Add(detail);
                    }

                    timeSheetByEmployee.TimeSheetDetail = details;

                    result.Add(timeSheetByEmployee);
                }
            }

            return result;
        }

        private TimeSheetDetailShiftModel CreateDetailShift(ShiftConfigurationModel shift
            , TimeSheetDetailModel detail
            , double? timeInRaw
            , double? timeOutRaw
            , List<CountedSymbolModel> countedSymbols
            , List<AbsenceTypeSymbolModel> absences
            , EnumOvertimeMode overtimeMode
            , IList<OvertimePlanModel> overtimePlans
            , bool ignoreOvertimePlan)
        {
            var detailShift = new TimeSheetDetailShiftModel();
            detailShift.ShiftConfigurationId = shift.ShiftConfigurationId;
            if (overtimeMode == EnumOvertimeMode.ByOvertimePlan)
            {
                SetOvertimeByPlan(detailShift, detail, shift, countedSymbols, overtimePlans);
            }

            detailShift.TimeIn = timeInRaw;
            detailShift.TimeOut = timeOutRaw;
            SetsMinsLate(detailShift, shift, countedSymbols, timeInRaw);
            SetsMinsEarly(detailShift, shift, countedSymbols, timeOutRaw);

            if (timeInRaw.HasValue)
            {
                if (timeInRaw <= shift.EntryTime
                    || (shift.MinsAllowToLate * 60 >= (timeInRaw - shift.EntryTime)))
                {
                    //Đủ công (X)
                    if (shift.IsNightShift)
                    {
                        detailShift.TimeSheetDetailShiftCounted.Add(GetCountedSymbolModel(shift, countedSymbols, EnumCountedSymbol.ShiftNightSymbol));
                    }
                    detailShift.WorkCounted = shift.ConfirmationUnit;
                    detailShift.ActualWorkMins = shift.ConvertToMins;

                    if (shift.OvertimeConfiguration.OvertimeCalculationMode == EnumOvertimeCalculationMode.ByTotalEarlyLateHours)
                    {
                        //Tăng ca trc giờ (X+)
                        CalcOvertime(detailShift, detail, shift, EnumTimeSheetOvertimeType.BeforeWork, countedSymbols, overtimeMode, overtimePlans, ignoreOvertimePlan, timeInRaw, null);
                    }
                }
                else
                {
                    if (shift.PartialShiftCalculationMode == EnumPartialShiftCalculationMode.CalculateByHalfDay)
                    {
                        //(X/2)
                        detailShift.ActualWorkMins = shift.ConvertToMins / 2;
                        detailShift.WorkCounted = shift.ConfirmationUnit / 2;

                        detailShift.TimeSheetDetailShiftCounted.Add(GetCountedSymbolModel(shift, countedSymbols, EnumCountedSymbol.HalfWorkOnTimeSymbol));

                    }
                    else
                    {
                        if ((timeInRaw - shift.EntryTime) <= shift.MaxLateMins * 60)
                        {
                            //Trễ (TR)
                            if (shift.IsSubtractionForLate)
                            {
                                detailShift.WorkCounted = shift.ConfirmationUnit * (1 - (decimal)detailShift.MinsLate / shift.ConvertToMins);
                                detailShift.ActualWorkMins = shift.ConvertToMins - (long)detailShift.MinsLate;
                            }
                            else
                            {
                                detailShift.WorkCounted = shift.ConfirmationUnit;
                                detailShift.ActualWorkMins = shift.ConvertToMins;
                            }
                        }
                        else
                        {
                            //Vắng (V)
                            SetDetailShiftForAbsence(detail.TimeSheetDateType, detailShift, shift.ExceededLateAbsenceTypeId, shift, countedSymbols, absences);
                            return detailShift;
                        }
                    }
                }
            }
            else
            {
                if (shift.IsNoEntryTimeWorkMins)
                {
                    //(X-)
                    detailShift.ActualWorkMins = (long)shift.NoEntryTimeWorkMins;
                    detailShift.WorkCounted = shift.ConfirmationUnit * ((decimal)shift.NoEntryTimeWorkMins / shift.ConvertToMins);
                    detailShift.TimeSheetDetailShiftCounted.Add(GetCountedSymbolModel(shift, countedSymbols, EnumCountedSymbol.BasedOnActualHoursSymbol));
                }
                else
                {
                    //(V)
                    SetDetailShiftForAbsence(detail.TimeSheetDateType, detailShift, shift.NoEntryTimeAbsenceTypeId, shift, countedSymbols, absences);
                    return detailShift;
                }
            }

            if (timeOutRaw.HasValue)
            {
                if (timeOutRaw >= shift.ExitTime
                    || shift.MinsAllowToEarly * 60 >= (shift.ExitTime - timeOutRaw))
                {
                    if (shift.OvertimeConfiguration.OvertimeCalculationMode == EnumOvertimeCalculationMode.ByTotalEarlyLateHours)
                    {
                        //Tăng ca sau giờ (X+)
                        CalcOvertime(detailShift, detail, shift, EnumTimeSheetOvertimeType.AfterWork, countedSymbols, overtimeMode, overtimePlans, ignoreOvertimePlan, null, timeOutRaw);
                    }
                }
                else
                {
                    if (shift.PartialShiftCalculationMode == EnumPartialShiftCalculationMode.CalculateByHalfDay)
                    {
                        //(X/2)
                        detailShift.ActualWorkMins = shift.ConvertToMins / 2;
                        detailShift.WorkCounted = shift.ConfirmationUnit / 2;

                        detailShift.TimeSheetDetailShiftCounted.Add(GetCountedSymbolModel(shift, countedSymbols, EnumCountedSymbol.HalfWorkOnTimeSymbol));

                    }
                    else
                    {
                        if ((shift.ExitTime - timeOutRaw) <= shift.MaxEarlyMins * 60)
                        {
                            //Sớm (SM)
                            if (shift.IsSubtractionForEarly)
                            {
                                detailShift.WorkCounted -= shift.ConfirmationUnit * ((decimal)detailShift.MinsEarly / shift.ConvertToMins);
                                if (detailShift.WorkCounted < 0)
                                    detailShift.WorkCounted = 0;

                                detailShift.ActualWorkMins -= (long)detailShift.MinsEarly;
                                if (detailShift.ActualWorkMins < 0)
                                    detailShift.ActualWorkMins = 0;
                            }

                            detailShift.TimeSheetDetailShiftCounted.Add(GetCountedSymbolModel(shift, countedSymbols, EnumCountedSymbol.EarlySymbol));
                        }
                        else
                        {
                            //Vắng (V)
                            SetDetailShiftForAbsence(detail.TimeSheetDateType, detailShift, shift.ExceededEarlyAbsenceTypeId, shift, countedSymbols, absences);
                            return detailShift;
                        }
                    }
                }
            }
            else
            {
                if (shift.IsNoExitTimeWorkMins)
                {
                    //(X-)
                    detailShift.ActualWorkMins -= (shift.ConvertToMins - (long)shift.NoExitTimeWorkMins);
                    if (detailShift.ActualWorkMins < 0)
                        detailShift.ActualWorkMins = 0;

                    var actualWorkCountTmp = shift.ConfirmationUnit * ((decimal)shift.NoExitTimeWorkMins / shift.ConvertToMins);
                    detailShift.WorkCounted -= (shift.ConfirmationUnit - actualWorkCountTmp);
                    if (detailShift.WorkCounted < 0)
                        detailShift.WorkCounted = 0;

                    detailShift.TimeSheetDetailShiftCounted.Add(GetCountedSymbolModel(shift, countedSymbols, EnumCountedSymbol.BasedOnActualHoursSymbol));
                }
                else
                {
                    //(V)
                    SetDetailShiftForAbsence(detail.TimeSheetDateType, detailShift, shift.NoExitTimeAbsenceTypeId, shift, countedSymbols, absences);
                    return detailShift;
                }
            }

            if (timeInRaw == null && timeOutRaw == null)
            {
                SetDetailShiftForAbsence(detail.TimeSheetDateType, detailShift, null, shift, countedSymbols, absences);
                return detailShift;
            }
            if (shift.OvertimeConfiguration.OvertimeCalculationMode == EnumOvertimeCalculationMode.ByActualWorkingHours
                && ((timeInRaw != null && timeInRaw < shift.EntryTime) || (timeOutRaw != null && timeOutRaw > shift.ExitTime)))
            {
                //Tăng ca tổng hợp
                CalcOvertime(detailShift, detail, shift, EnumTimeSheetOvertimeType.Default, countedSymbols, overtimeMode, overtimePlans, ignoreOvertimePlan, timeInRaw, timeOutRaw);
            }

            //Case Date As Overtime
            var timeFrames = GetOverTimeLevelByTimeFrame(detail.TimeSheetDateType, shift, true, EnumTimeSheetOvertimeType.DateAsOvertime, timeInRaw, timeOutRaw);
            if (timeFrames.TryGetValue((new TimeFrame(shift.EntryTime, shift.ExitTime), new TimeFrame(shift.EntryTime, shift.ExitTime)), out int? dateAsOvertimeLevelId)
                && !detailShift.TimeSheetDetailShiftCounted.Any(c => c.CountedSymbolId == GetCountedSymbolModel(shift, countedSymbols, EnumCountedSymbol.AbsentSymbol).CountedSymbolId))
            {
                detailShift.DateAsOvertimeLevelId = dateAsOvertimeLevelId;
                detailShift.TimeSheetDetailShiftCounted = detailShift.TimeSheetDetailShiftCounted.Where(c => c.CountedSymbolId != GetCountedSymbolModel(shift, countedSymbols, EnumCountedSymbol.OvertimeSymbol).CountedSymbolId).ToIList();
                detailShift.TimeSheetDetailShiftCounted.Add(GetCountedSymbolModel(shift, countedSymbols, EnumCountedSymbol.OvertimeDateSymbol));
                detailShift.TimeSheetDetailShiftOvertime.Add(new TimeSheetDetailShiftOvertimeModel()
                {
                    ShiftConfigurationId = shift.ShiftConfigurationId,
                    OvertimeLevelId = (int)detailShift.DateAsOvertimeLevelId,
                    OvertimeType = EnumTimeSheetOvertimeType.DateAsOvertime,
                    MinsOvertime = detailShift.ActualWorkMins
                });

                detailShift.ActualWorkMins = 0;
                detailShift.WorkCounted = 0;
                return detailShift;
            }

            //Add default countedSymbol
            detailShift.TimeSheetDetailShiftCounted.Add(GetCountedSymbolModel(shift, countedSymbols, shift.IsNightShift ? EnumCountedSymbol.ShiftNightSymbol : EnumCountedSymbol.FullCountedSymbol));

            return detailShift;
        }

        private void CalcOvertime(
            TimeSheetDetailShiftModel detailShift,
            TimeSheetDetailModel detail,
            ShiftConfigurationModel shift,
            EnumTimeSheetOvertimeType overtimeType,
            List<CountedSymbolModel> countedSymbols,
            EnumOvertimeMode overtimeMode,
            IList<OvertimePlanModel> overtimePlans,
            bool ignoreOvertimePlan,
            double? timeInRaw, double? timeOutRaw)
        {
            if (overtimeMode == EnumOvertimeMode.ByOvertimePlan)
                return;

            var overTimeLevelByTimeFrame = GetOverTimeLevelByTimeFrame(detail.TimeSheetDateType, shift, true, overtimeType, timeInRaw, timeOutRaw);
            if (overTimeLevelByTimeFrame.Count == 0)
            {
                return;
            }

            var filteredPlans = overtimePlans.Where(p => p.AssignedDate == detail.Date && p.EmployeeId == detail.EmployeeId && p.OvertimeHours > 0);
            var planSet = new Dictionary<TimeFrame, decimal>();

            var totalWorkOvertimes = new List<TimeSheetDetailShiftOvertimeModel>();
            var beforeWorkOvertimes = new List<TimeSheetDetailShiftOvertimeModel>();
            var afterWorkOvertimes = new List<TimeSheetDetailShiftOvertimeModel>();

            foreach (var frame in overTimeLevelByTimeFrame)
            {
                var plan = filteredPlans.FirstOrDefault(p => p.OvertimeLevelId == frame.Value);
                if (overtimeMode == EnumOvertimeMode.ByAll && plan == null)
                {
                    continue;
                }
                if (plan != null)
                {
                    planSet.Add(frame.Key.OriginFrame, plan.OvertimeHours);
                }

                var overtime = new TimeSheetDetailShiftOvertimeModel()
                {
                    ShiftConfigurationId = shift.ShiftConfigurationId,
                    StartTime = frame.Key.OriginFrame.StartTime,
                    EndTime = frame.Key.OriginFrame.EndTime,
                    OvertimeLevelId = (int)frame.Value,
                    OvertimeType = overtimeType,
                    MinsOvertime = (long)(frame.Key.Frame.EndTime - frame.Key.Frame.StartTime) / 60
                };

                if (overtimeType == EnumTimeSheetOvertimeType.Default)
                {
                    totalWorkOvertimes.Add(overtime);
                }
                else if (overtime.EndTime <= shift.EntryTime)
                {
                    beforeWorkOvertimes.Add(overtime);
                }
                else
                {
                    afterWorkOvertimes.Add(overtime);
                }
            }

            totalWorkOvertimes = totalWorkOvertimes.OrderByDescending(o => o.StartTime).ToList();
            beforeWorkOvertimes = beforeWorkOvertimes.OrderBy(o => o.StartTime).ToList();
            afterWorkOvertimes = afterWorkOvertimes.OrderByDescending(o => o.StartTime).ToList();

            var (minsReaches, minsBonus, minsLimit, isMinThresholdMins, minThresholdMins, isCalculationThresholdMins) = GetOvertimeConfigParas(shift, overtimeType);

            if (totalWorkOvertimes.Any())
            {
                ApllyConfigParas(ref totalWorkOvertimes, minsReaches, minsBonus, minsLimit, isMinThresholdMins, minThresholdMins, isCalculationThresholdMins);
            }
            if (beforeWorkOvertimes.Any())
            {
                ApllyConfigParas(ref beforeWorkOvertimes, minsReaches, minsBonus, minsLimit, isMinThresholdMins, minThresholdMins, isCalculationThresholdMins);
            }
            if (afterWorkOvertimes.Any())
            {
                ApllyConfigParas(ref afterWorkOvertimes, minsReaches, minsBonus, minsLimit, isMinThresholdMins, minThresholdMins, isCalculationThresholdMins);
            }
            var overtimes = overtimeType == EnumTimeSheetOvertimeType.Default ? totalWorkOvertimes : beforeWorkOvertimes.Concat(afterWorkOvertimes).ToList();

            if (overtimes.Any())
            {
                var roundMinutes = shift.OvertimeConfiguration.RoundMinutes ?? 0;
                overtimes.ForEach(o =>
                {
                    o.MinsOvertime = LimitedByOvertimeLevel(o.MinsOvertime, shift.OvertimeConfiguration, o.OvertimeLevelId);
                    o.MinsOvertime = RoundValue(o.MinsOvertime, shift.OvertimeConfiguration.IsRoundBack, (long)roundMinutes);

                    if (!ignoreOvertimePlan && overtimeMode == EnumOvertimeMode.ByAll && planSet.TryGetValue(new TimeFrame(o.StartTime, o.EndTime), out decimal overtimeHours) && o.MinsOvertime > overtimeHours * 60)
                    {
                        o.MinsOvertime = (long)(overtimeHours * 60);
                    }

                    detailShift.TimeSheetDetailShiftOvertime.Add(o);
                });

                detailShift.TimeSheetDetailShiftCounted.Add(GetCountedSymbolModel(shift, countedSymbols, EnumCountedSymbol.OvertimeSymbol));
            }
        }

        private Dictionary<(TimeFrame OriginFrame, TimeFrame Frame), int?> GetOverTimeLevelByTimeFrame(EnumTimeSheetDateType dateType, ShiftConfigurationModel shift, bool isOvertime, EnumTimeSheetOvertimeType overtimeType, double? timeInRaw, double? timeOutRaw)
        {
            var result = new Dictionary<(TimeFrame originFrame, TimeFrame frame), int?>();
            var timeFrames = shift.OvertimeConfiguration.OvertimeConfigurationTimeFrame
                    .Where(tf => tf.TimeSheetDateType == dateType && tf.OvertimeLevelId.HasValue);

            var framesWithoutWorkingHours = timeFrames.Where(tf => !tf.IsWorkingHours);

            switch (overtimeType)
            {
                case EnumTimeSheetOvertimeType.BeforeWork:
                    foreach (var frame in framesWithoutWorkingHours)
                    {
                        if (timeInRaw > frame.EndTime || shift.EntryTime < frame.EndTime)
                        {
                            continue;
                        }
                        if (timeInRaw < frame.StartTime)
                        {
                            result.Add((new TimeFrame(frame.StartTime, frame.EndTime), new TimeFrame(frame.StartTime, frame.EndTime)), frame.OvertimeLevelId);
                        }
                        else if (timeInRaw < frame.EndTime)
                        {
                            result.Add((new TimeFrame(frame.StartTime, frame.EndTime), new TimeFrame((double)timeInRaw, frame.EndTime)), frame.OvertimeLevelId);
                        }
                    }
                    break;

                case EnumTimeSheetOvertimeType.AfterWork:
                    foreach (var frame in framesWithoutWorkingHours)
                    {
                        if (timeOutRaw < frame.StartTime || shift.ExitTime > frame.StartTime)
                        {
                            continue;
                        }
                        if (timeOutRaw > frame.EndTime)
                        {
                            result.Add((new TimeFrame(frame.StartTime, frame.EndTime), new TimeFrame(frame.StartTime, frame.EndTime)), frame.OvertimeLevelId);
                        }
                        else if (timeOutRaw > frame.StartTime)
                        {
                            result.Add((new TimeFrame(frame.StartTime, frame.EndTime), new TimeFrame(frame.StartTime, (double)timeOutRaw)), frame.OvertimeLevelId);
                        }
                    }
                    break;

                case EnumTimeSheetOvertimeType.Default:
                    foreach (var frame in framesWithoutWorkingHours)
                    {
                        if (timeInRaw > frame.EndTime || timeOutRaw < frame.StartTime)
                        {
                            continue;
                        }
                        if (timeInRaw < frame.StartTime && timeOutRaw > frame.EndTime)
                        {
                            result.Add((new TimeFrame(frame.StartTime, frame.EndTime), new TimeFrame(frame.StartTime, frame.EndTime)), frame.OvertimeLevelId);
                        }
                        else if (timeInRaw < frame.EndTime && frame.EndTime <= shift.EntryTime)
                        {
                            result.Add((new TimeFrame(frame.StartTime, frame.EndTime), new TimeFrame((double)timeInRaw, frame.EndTime)), frame.OvertimeLevelId);
                        }
                        else if (timeOutRaw > frame.StartTime && frame.StartTime >= shift.ExitTime)
                        {
                            result.Add((new TimeFrame(frame.StartTime, frame.EndTime), new TimeFrame(frame.StartTime, (double)timeOutRaw)), frame.OvertimeLevelId);
                        }
                    }
                    break;

                default:
                    var workingHoursFrame = timeFrames.FirstOrDefault(tf => tf.IsWorkingHours);
                    if (workingHoursFrame != null)
                    {
                        result.Add((new TimeFrame(workingHoursFrame.StartTime, workingHoursFrame.EndTime), new TimeFrame(workingHoursFrame.StartTime, workingHoursFrame.EndTime)), workingHoursFrame.OvertimeLevelId);
                    }
                    break;
            }

            return result;
        }
        private void ApllyConfigParas(ref List<TimeSheetDetailShiftOvertimeModel> overtimes, long minsReaches, long minsBonus, long minsLimit, bool isMinThresholdMins, long minThresholdMins, bool isCalculationThresholdMins)
        {
            //Apply MinThreshold Minutes
            var sumMins = overtimes.Sum(o => o.MinsOvertime);
            if (!isMinThresholdMins || sumMins <= minThresholdMins)
            {
                overtimes.Clear();
                return;
            }

            if (minThresholdMins > 0 && !isCalculationThresholdMins)
            {
                for (int i = overtimes.Count - 1; i >= 0; i--)
                {
                    var overtime = overtimes[i];

                    if (overtime.MinsOvertime > minThresholdMins)
                    {
                        overtime.MinsOvertime -= minThresholdMins;
                        break;
                    }
                    else
                    {
                        overtimes.RemoveAt(i);
                        minThresholdMins -= overtime.MinsOvertime;
                    }
                }
            }

            //Apply Bonus Minutes
            sumMins = overtimes.Sum(o => o.MinsOvertime);
            if (minsReaches > 0 && sumMins >= minsReaches)
            {
                foreach (var overtime in overtimes)
                {
                    var frame = (long)(overtime.EndTime - overtime.StartTime) / 60;

                    if (overtime.MinsOvertime + minsBonus < frame)
                    {
                        overtime.MinsOvertime += minsBonus;
                        break;
                    }
                    else
                    {
                        minsBonus -= frame - overtime.MinsOvertime;
                        overtime.MinsOvertime = frame;
                    }
                }
            }

            //Apply Limit Minutes
            sumMins = overtimes.Sum(o => o.MinsOvertime);
            if (minsLimit == 0 || sumMins <= minsLimit)
            {
                return;
            }

            long minutesToReduce = sumMins - minsLimit;
            foreach (var overtime in overtimes)
            {
                if (minutesToReduce <= 0) break;

                long reduction = Math.Min(overtime.MinsOvertime, minutesToReduce);
                overtime.MinsOvertime -= reduction;
                minutesToReduce -= reduction;
            }

            overtimes = overtimes.Where(o => o.MinsOvertime > 0).ToList();
        }

        private void SetDetailShiftForAbsence(EnumTimeSheetDateType timeSheetDateType
            , TimeSheetDetailShiftModel detailShift
            , int? absenceTypeSymbolId
            , ShiftConfigurationModel shift
            , List<CountedSymbolModel> countedSymbols
            , List<AbsenceTypeSymbolModel> absences)
        {
            if (timeSheetDateType == EnumTimeSheetDateType.Holiday && shift.IsCountWorkForHoliday)
            {
                //Đủ công (X)
                detailShift.WorkCounted = shift.ConfirmationUnit;
                detailShift.ActualWorkMins = shift.ConvertToMins;
                detailShift.TimeSheetDetailShiftCounted.Clear();
                detailShift.TimeSheetDetailShiftCounted.Add(GetCountedSymbolModel(shift, countedSymbols, shift.IsNightShift ? EnumCountedSymbol.ShiftNightSymbol : EnumCountedSymbol.FullCountedSymbol));
                return;
            }

            if ((timeSheetDateType == EnumTimeSheetDateType.Weekend && shift.IsSkipWeeklyOffDayWithShift) || (timeSheetDateType == EnumTimeSheetDateType.Holiday && shift.IsSkipHolidayWithShift))
            {
                detailShift.NonAbsentScheduled = true;
                detailShift.WorkCounted = 0;
                detailShift.ActualWorkMins = 0;
                detailShift.TimeSheetDetailShiftCounted.Add(GetCountedSymbolModel(shift, countedSymbols, EnumCountedSymbol.OffSymbol));

                return;
            }

            var absence = absenceTypeSymbolId != null ? absences.FirstOrDefault(a => a.AbsenceTypeSymbolId == absenceTypeSymbolId) : absences.FirstOrDefault(a => a.IsUnpaidLeave);
            if (absence == null)
            {
                detailShift.WorkCounted = 0;
                detailShift.ActualWorkMins = 0;
            }
            else
            {
                detailShift.AbsenceTypeSymbolId = absence.AbsenceTypeSymbolId;
                detailShift.WorkCounted = absence.IsCounted ? shift.ConfirmationUnit * (decimal)absence.SalaryRate : 0;
                detailShift.ActualWorkMins = absence.IsCounted ? (long)(shift.ConvertToMins * absence.SalaryRate) : 0;
            }

            detailShift.TimeSheetDetailShiftCounted.Clear();
            detailShift.TimeSheetDetailShiftOvertime.Clear();
            detailShift.TimeSheetDetailShiftCounted.Add(GetCountedSymbolModel(shift, countedSymbols, EnumCountedSymbol.AbsentSymbol));
        }

        private TimeSheetDetailShiftCountedModel GetCountedSymbolModel(ShiftConfigurationModel shift, List<CountedSymbolModel> countedSymbols, EnumCountedSymbol symbol)
        {
            return new TimeSheetDetailShiftCountedModel()
            {
                ShiftConfigurationId = shift.ShiftConfigurationId,
                CountedSymbolId = countedSymbols.FirstOrDefault(c => c.CountedSymbolType == symbol).CountedSymbolId
            };
        }

        private long RoundValue(long actualValue, bool isRoundBack, long? roundValue)
        {
            return roundValue == null || roundValue == 0 ?
                    actualValue :
                    (isRoundBack ?
                    actualValue / roundValue.Value * roundValue.Value :
                    (long)Math.Ceiling((double)actualValue / roundValue.Value) * roundValue.Value);
        }

        private long LimitedByOvertimeLevel(long minsOvertime, OvertimeConfigurationModel overtimeConfig, int overtimeLevelId)
        {
            var limitOvertimeLevel = overtimeConfig.OvertimeConfigurationMapping.FirstOrDefault(o => o.OvertimeLevelId == overtimeLevelId);
            return limitOvertimeLevel != null ? Math.Min(minsOvertime, limitOvertimeLevel.MinsLimit) : minsOvertime;
        }

        private (long minsReaches, long minsBonus, long minsLimit, bool isMinThresholdMins, long minThresholdMins, bool isCalculationThresholdMins) GetOvertimeConfigParas(ShiftConfigurationModel shift, EnumTimeSheetOvertimeType overtimeType)
        {
            long minsReaches;
            long minsBonus;
            long minsLimit;
            bool isMinThresholdMins;
            long minThresholdMins;
            bool isCalculationThresholdMins;

            switch (overtimeType)
            {
                case EnumTimeSheetOvertimeType.BeforeWork:
                    minsReaches = shift.OvertimeConfiguration.MinsReachesBeforeWork;
                    minsBonus = shift.OvertimeConfiguration.MinsBonusWhenMinsReachesBeforeWork;
                    minsLimit = shift.OvertimeConfiguration.MinsLimitOvertimeBeforeWork;
                    isMinThresholdMins = shift.OvertimeConfiguration.IsMinThresholdMinutesBeforeWork;
                    minThresholdMins = shift.OvertimeConfiguration.MinThresholdMinutesBeforeWork ?? 0;
                    isCalculationThresholdMins = shift.OvertimeConfiguration.IsCalculationThresholdMinsBeforeWork;
                    break;
                case EnumTimeSheetOvertimeType.AfterWork:
                    minsReaches = shift.OvertimeConfiguration.MinsReachesAfterWork;
                    minsBonus = shift.OvertimeConfiguration.MinsBonusWhenMinsReachesAfterWork;
                    minsLimit = shift.OvertimeConfiguration.MinsLimitOvertimeAfterWork;
                    isMinThresholdMins = shift.OvertimeConfiguration.IsMinThresholdMinutesAfterWork;
                    minThresholdMins = shift.OvertimeConfiguration.MinThresholdMinutesAfterWork ?? 0;
                    isCalculationThresholdMins = shift.OvertimeConfiguration.IsCalculationThresholdMinsAfterWork;
                    break;
                default:
                    minsReaches = shift.OvertimeConfiguration.MinsReaches;
                    minsBonus = shift.OvertimeConfiguration.MinsBonusWhenMinsReaches;
                    minsLimit = shift.OvertimeConfiguration.MinsLimitOvertime;
                    isMinThresholdMins = shift.OvertimeConfiguration.IsOvertimeThresholdMins;
                    minThresholdMins = shift.OvertimeConfiguration.OvertimeThresholdMins ?? 0;
                    isCalculationThresholdMins = shift.OvertimeConfiguration.IsCalculationThresholdMins;
                    break;
            }

            return (minsReaches, minsBonus, minsLimit, isMinThresholdMins, minThresholdMins, isCalculationThresholdMins);
        }

        private void SetOvertimeByPlan(TimeSheetDetailShiftModel detailShift, TimeSheetDetailModel detail, ShiftConfigurationModel shift, List<CountedSymbolModel> countedSymbols, IList<OvertimePlanModel> overtimePlans)
        {
            detailShift.TimeSheetDetailShiftOvertime.Clear();

            var overtimePlan = overtimePlans.Where(p => p.AssignedDate == detail.Date && p.EmployeeId == detail.EmployeeId && p.OvertimeHours > 0);

            if (overtimePlan.Any())
            {
                foreach (var plan in overtimePlan)
                {
                    detailShift.TimeSheetDetailShiftOvertime.Add(new TimeSheetDetailShiftOvertimeModel()
                    {
                        ShiftConfigurationId = detailShift.ShiftConfigurationId,
                        OvertimeLevelId = plan.OvertimeLevelId,
                        OvertimeType = EnumTimeSheetOvertimeType.DateAsOvertime,
                        MinsOvertime = (long)(plan.OvertimeHours * 60)
                    });
                }

                detailShift.TimeSheetDetailShiftCounted.Add(GetCountedSymbolModel(shift, countedSymbols, EnumCountedSymbol.OvertimeSymbol));
            }
        }

        private void SetsMinsLate(TimeSheetDetailShiftModel detailShift, ShiftConfigurationModel shift, List<CountedSymbolModel> countedSymbols, double? timeInRaw)
        {
            if (timeInRaw == null || timeInRaw <= shift.EntryTime || (shift.MinsAllowToLate * 60 >= (timeInRaw - shift.EntryTime)))
            {
                return;
            }

            var actualMinsLate = shift.IsCalculationForLate ? (long)(timeInRaw - shift.EntryTime) : (long)(timeInRaw - shift.EntryTime) - shift.MinsAllowToLate * 60;

            if (timeInRaw > shift.LunchTimeFinish)
            {
                actualMinsLate -= (long)(shift.LunchTimeFinish - shift.LunchTimeStart);
            }
            else if (timeInRaw > shift.LunchTimeStart)
            {
                actualMinsLate -= (long)(timeInRaw - shift.LunchTimeStart);
            }
            actualMinsLate /= 60;

            detailShift.MinsLate = RoundValue((long)actualMinsLate, shift.IsRoundBackForLate, shift.MinsRoundForLate);

            detailShift.TimeSheetDetailShiftCounted.Add(GetCountedSymbolModel(shift, countedSymbols, EnumCountedSymbol.BeLateSymbol));
        }
        private void SetsMinsEarly(TimeSheetDetailShiftModel detailShift, ShiftConfigurationModel shift, List<CountedSymbolModel> countedSymbols, double? timeOutRaw)
        {
            if (timeOutRaw == null || timeOutRaw >= shift.ExitTime  || shift.MinsAllowToEarly * 60 >= (shift.ExitTime - timeOutRaw))
            {
                return;
            }    
             
            var actualMinsEarly = shift.IsCalculationForEarly ? (long)(shift.ExitTime - timeOutRaw) : (long)(shift.ExitTime - timeOutRaw) - shift.MinsAllowToEarly * 60;

            if (timeOutRaw < shift.LunchTimeStart)
            {
                actualMinsEarly -= (long)(shift.LunchTimeFinish - shift.LunchTimeStart);
            }
            else if (timeOutRaw < shift.LunchTimeFinish)
            {
                actualMinsEarly -= (long)(shift.LunchTimeFinish - timeOutRaw);
            }
            actualMinsEarly /= 60;

            detailShift.MinsEarly = RoundValue(actualMinsEarly, shift.IsRoundBackForEarly, shift.MinsRoundForEarly);

            detailShift.TimeSheetDetailShiftCounted.Add(GetCountedSymbolModel(shift, countedSymbols, EnumCountedSymbol.EarlySymbol));
        }

        //public CategoryNameModel GetFieldDataForMapping(long beginDate, long endDate)
        //{
        //    var result = new CategoryNameModel()
        //    {
        //        CategoryCode = "TimeSheet",
        //        CategoryTitle = "Chấm công",
        //        IsTreeView = false,
        //        Fields = new List<CategoryFieldNameModel>()
        //    };

        //    var fields = ExcelUtils.GetFieldNameModels<TimeSheetImportFieldModel>().ToList();

        //    var fieldsAbsenceTypeSymbols = (_organizationDBContext.AbsenceTypeSymbol.ToList()).Select(x => new CategoryFieldNameModel
        //    {
        //        FieldName = x.SymbolCode,
        //        FieldTitle = x.TypeSymbolDescription,
        //        GroupName = "Ngày nghỉ",
        //    });

        //    var fieldsOvertimeLevel = (_organizationDBContext.OvertimeLevel.ToList()).Select(x => new CategoryFieldNameModel
        //    {
        //        FieldName = $"Overtime_{x.OvertimeCode}",
        //        FieldTitle = $"Tổng thời gian(giờ) làm tăng ca {x.OvertimeCode}",
        //        GroupName = "Tăng ca(giờ)",
        //    });

        //    for (long unixTime = beginDate; unixTime <= endDate; unixTime += 86400)
        //    {
        //        var date = unixTime.UnixToDateTime().Value;
        //        fields.Add(new CategoryFieldNameModel
        //        {
        //            FieldName = $"TimeKeepingDay{unixTime}",
        //            FieldTitle = $"Thời gian chấm công ngày {date.ToString("dd/MM/yyyy")} (hh:mm)",
        //            GroupName = "TT chấm công",
        //        });
        //    }

        //    fields.AddRange(fieldsAbsenceTypeSymbols);
        //    fields.AddRange(fieldsOvertimeLevel);

        //    fields.Add(new CategoryFieldNameModel
        //    {
        //        FieldName = ImportStaticFieldConsants.CheckImportRowEmpty,
        //        FieldTitle = "Cột kiểm tra",
        //    });

        //    result.Fields = fields;
        //    return result;
        //}

        //public async Task<bool> ImportTimeSheetFromMapping(int month, int year, long beginDate, long endDate, ImportExcelMapping mapping, Stream stream)
        //{
        //    string timeKeepingDayPropPrefix = "TimeKeepingDay";
        //    string timeKeepingOvertimePropPrefix = "Overtime";
        //    Type typeInfo = typeof(TimeSheetImportFieldModel);

        //    var reader = new ExcelReader(stream);

        //    var employees = await _organizationDBContext.Employee.ToListAsync();
        //    var absenceTypeSymbols = await _organizationDBContext.AbsenceTypeSymbol.ToListAsync();
        //    var overtimeLevels = await _organizationDBContext.OvertimeLevel.ToListAsync();
        //    var absentSymbol = await _organizationDBContext.CountedSymbol.FirstOrDefaultAsync(x => x.CountedSymbolType == (int)EnumCountedSymbol.AbsentSymbol);

        //    var _importData = reader.ReadSheets(mapping.SheetName, mapping.FromRow, mapping.ToRow, null).First();

        //    var dataTimeSheetWithPrimaryKey = new List<RowDataImportTimeSheetModel>();
        //    int i = 0;
        //    foreach (var row in _importData.Rows)
        //    {
        //        var fieldCheckImportEmpty = mapping.MappingFields.FirstOrDefault(x => x.FieldName == ImportStaticFieldConsants.CheckImportRowEmpty);
        //        if (fieldCheckImportEmpty != null)
        //        {
        //            string value = null;
        //            if (row.ContainsKey(fieldCheckImportEmpty.Column))
        //                value = row[fieldCheckImportEmpty.Column]?.ToString();

        //            if (string.IsNullOrWhiteSpace(value)) continue;
        //        }

        //        var timeSheetImportModel = new TimeSheetImportFieldModel();
        //        foreach (var prop in typeInfo.GetProperties())
        //        {
        //            var mappingField = mapping.MappingFields.FirstOrDefault(x => x.FieldName == prop.Name);
        //            if (mappingField == null)
        //                throw new BadRequestException(GeneralCode.ItemNotFound, $"Không tìm thấy field {prop.Name}");

        //            string value = null;
        //            if (row.ContainsKey(mappingField.Column))
        //                value = row[mappingField.Column]?.ToString();

        //            if (value != null && value.StartsWith(PREFIX_ERROR_CELL))
        //            {
        //                throw ValidatorResources.ExcelFormulaNotSupported.BadRequestFormat(i + mapping.FromRow, mappingField.Column, $"{value}");
        //            }

        //            prop.SetValue(timeSheetImportModel, value.ConvertValueByType(prop.PropertyType));
        //        }

        //        dataTimeSheetWithPrimaryKey.Add(new RowDataImportTimeSheetModel()
        //        {
        //            EmployeeCode = timeSheetImportModel.EmployeeCode,
        //            row = row,
        //            timeSheetImportModel = timeSheetImportModel
        //        });
        //        i++;
        //    }






        //    var timeSheetDetails = new List<TimeSheetDetail>();
        //    var timeSheetAggregates = new List<TimeSheetAggregateModel>();
        //    var timeSheetDayOffs = new List<TimeSheetDayOffModel>();
        //    var timeSheetOvertimes = new List<TimeSheetAggregateOvertimeModel>();
        //    foreach (var (key, rows) in dataTimeSheetWithPrimaryKey.GroupBy(x => x.EmployeeCode).ToDictionary(k => k.Key, v => v.ToList()))
        //    {
        //        var employeeCode = key.NormalizeAsInternalName();

        //        var employee = employees.FirstOrDefault(e => e.EmployeeCode.NormalizeAsInternalName() == employeeCode || e.Email.NormalizeAsInternalName() == employeeCode);

        //        if (employee == null) throw new BadRequestException(GeneralCode.InvalidParams, $"Nhân viên {employeeCode} không tồn tại");



        //        var rowIn = rows.First();
        //        var rowOut = rows.Last();

        //        for (long unixTime = beginDate; unixTime <= endDate; unixTime += 86400)
        //        {
        //            var timeKeepingDayProp = $"{timeKeepingDayPropPrefix}{unixTime}";

        //            var mappingFieldTimeKeepingDay = mapping.MappingFields.FirstOrDefault(x => x.FieldName == timeKeepingDayProp);
        //            if (mappingFieldTimeKeepingDay == null)
        //                continue;

        //            string timeInAsString = null;
        //            string timeOutAsString = null;

        //            if (rowIn.row.ContainsKey(mappingFieldTimeKeepingDay.Column))
        //                timeInAsString = rowIn.row[mappingFieldTimeKeepingDay.Column]?.ToString();

        //            if (rowOut.row.ContainsKey(mappingFieldTimeKeepingDay.Column))
        //                timeInAsString = rowOut.row[mappingFieldTimeKeepingDay.Column]?.ToString();

        //            // var timeInAsString = typeInfo.GetProperty(timeKeepingDayProp).GetValue(rowIn.timeSheetImportModel) as string;
        //            // var timeOutAsString = typeInfo.GetProperty(timeKeepingDayProp).GetValue(rowOut.timeSheetImportModel) as string;

        //            if (timeInAsString == absentSymbol.SymbolCode) continue;

        //            int? absenceTypeSymbolId = null;

        //            if (timeInAsString.Contains('-')) continue;

        //            if (!timeInAsString.Contains(':'))
        //            {
        //                var absenceTypeSymbolCode = timeInAsString;
        //                var absenceType = absenceTypeSymbols.FirstOrDefault(x => x.SymbolCode == absenceTypeSymbolCode);
        //                if (absenceType == null)
        //                    throw new BadRequestException(GeneralCode.InvalidParams, $"Không có ký hiệu loại vắng {absenceTypeSymbolCode} trong hệ thống");
        //                absenceTypeSymbolId = absenceType.AbsenceTypeSymbolId;
        //            }

        //            var date = unixTime.UnixToDateTime().Value;


        //            TimeSheetDetail timeSheetDetail;

        //            if (absenceTypeSymbolId.HasValue)
        //            {
        //                timeSheetDetail = new TimeSheetDetail()
        //                {
        //                    AbsenceTypeSymbolId = absenceTypeSymbolId,
        //                    Date = date,
        //                    EmployeeId = employee.UserId
        //                };
        //            }
        //            else
        //            {
        //                var anyTimeAsStringEmpty = string.IsNullOrWhiteSpace(timeInAsString) || string.IsNullOrWhiteSpace(timeOutAsString);
        //                if (anyTimeAsStringEmpty)
        //                {
        //                    timeSheetDetail = new TimeSheetDetail()
        //                    {
        //                        Date = date,
        //                        EmployeeId = employee.UserId
        //                    };
        //                }
        //                else
        //                {
        //                    timeSheetDetail = new TimeSheetDetail()
        //                    {
        //                        Date = date,
        //                        TimeIn = TimeSpan.Parse(timeInAsString),
        //                        TimeOut = TimeSpan.Parse(timeOutAsString),
        //                        EmployeeId = employee.UserId
        //                    };
        //                }
        //            }

        //            timeSheetDetails.Add(timeSheetDetail);
        //        }

        //        var timeSheetAggregate = new TimeSheetAggregateModel
        //        {
        //            CountedAbsence = rowIn.timeSheetImportModel.CountedAbsence,
        //            CountedEarly = rowIn.timeSheetImportModel.CountedEarly,
        //            CountedLate = rowIn.timeSheetImportModel.CountedLate,
        //            CountedWeekday = rowIn.timeSheetImportModel.CountedWeekday,
        //            CountedWeekdayHour = rowIn.timeSheetImportModel.CountedWeekdayHour,
        //            CountedWeekend = rowIn.timeSheetImportModel.CountedWeekend,
        //            CountedWeekendHour = rowIn.timeSheetImportModel.CountedWeekendHour,
        //            EmployeeId = employee.UserId,
        //            MinsEarly = rowIn.timeSheetImportModel.MinsEarly,
        //            MinsLate = rowIn.timeSheetImportModel.MinsLate,
        //            // Overtime1 = rowIn.timeSheetImportModel.Overtime1,
        //            // Overtime2 = rowIn.timeSheetImportModel.Overtime2,
        //            // Overtime3 = rowIn.timeSheetImportModel.Overtime3,
        //        };

        //        var timeSheetDayOffForPerson = absenceTypeSymbols.Where(absence => mapping.MappingFields.Any(x => x.FieldName == absence.SymbolCode))
        //        .Select(absence =>
        //        {
        //            var mappingField = mapping.MappingFields.FirstOrDefault(x => x.FieldName == absence.SymbolCode);
        //            if (mappingField == null)
        //                throw new BadRequestException(GeneralCode.ItemNotFound, $"Không tìm thấy trường dữ liệu {absence.SymbolCode}");

        //            string countedDayOffAsString = null;
        //            if (rowIn.row.ContainsKey(mappingField.Column))
        //                countedDayOffAsString = rowIn.row[mappingField.Column]?.ToString();

        //            var countedDayOff = int.Parse(string.IsNullOrWhiteSpace(countedDayOffAsString) ? "0" : countedDayOffAsString);

        //            return new TimeSheetDayOffModel
        //            {
        //                AbsenceTypeSymbolId = absence.AbsenceTypeSymbolId,
        //                EmployeeId = employee.UserId,
        //                CountedDayOff = countedDayOff
        //            };
        //        }).Where(x => x.CountedDayOff > 0).ToList();

        //        var timeSheetOvertimeForPerson = overtimeLevels.Where(overtimeLevel => mapping.MappingFields.Any(x => x.FieldName == $"{timeKeepingOvertimePropPrefix}_{overtimeLevel.OvertimeCode}"))
        //        .Select(overtimeLevel =>
        //        {
        //            var timeKeepingOvertimeProp = $"{timeKeepingOvertimePropPrefix}_{overtimeLevel.OvertimeCode}";
        //            var mappingField = mapping.MappingFields.FirstOrDefault(x => x.FieldName == timeKeepingOvertimeProp);
        //            if (mappingField == null)
        //                throw new BadRequestException(GeneralCode.ItemNotFound, $"Không tìm thấy trường dữ liệu {timeKeepingOvertimeProp}");

        //            string minsOvertimeAsString = null;
        //            if (rowIn.row.ContainsKey(mappingField.Column))
        //                minsOvertimeAsString = rowIn.row[mappingField.Column]?.ToString();

        //            var minsOvertime = decimal.Parse(string.IsNullOrWhiteSpace(minsOvertimeAsString) ? "0" : minsOvertimeAsString);

        //            return new TimeSheetAggregateOvertimeModel
        //            {
        //                OvertimeLevelId = overtimeLevel.OvertimeLevelId,
        //                EmployeeId = employee.UserId,
        //                MinsOvertime = minsOvertime
        //            };
        //        }).Where(x => x.MinsOvertime > 0).ToList();


        //        timeSheetAggregates.Add(timeSheetAggregate);
        //        timeSheetDayOffs.AddRange(timeSheetDayOffForPerson);
        //        timeSheetOvertimes.AddRange(timeSheetOvertimeForPerson);
        //    }

        //    var trans = await _organizationDBContext.Database.BeginTransactionAsync();
        //    try
        //    {
        //        var timeSheet = await _organizationDBContext.TimeSheet.FirstOrDefaultAsync(x => x.Month == month && x.Year == year);
        //        if (timeSheet == null)
        //        {
        //            timeSheet = new TimeSheet()
        //            {
        //                Month = month,
        //                Year = year,
        //                IsApprove = false,
        //            };
        //            await _organizationDBContext.TimeSheet.AddAsync(timeSheet);
        //            await _organizationDBContext.SaveChangesAsync();
        //        }

        //        var existsTimeSheetDetails = await _organizationDBContext.TimeSheetDetail.Where(x => x.TimeSheetId == timeSheet.TimeSheetId).ToArrayAsync();
        //        var existsTimeSheetAggregates = await _organizationDBContext.TimeSheetAggregate.Where(x => x.TimeSheetId == timeSheet.TimeSheetId).ToArrayAsync();
        //        var existsTimeSheetDayOffs = await _organizationDBContext.TimeSheetDayOff.Where(x => x.TimeSheetId == timeSheet.TimeSheetId).ToArrayAsync();
        //        var existsTimeSheetOvertimes = await _organizationDBContext.TimeSheetOvertime.Where(x => x.TimeSheetId == timeSheet.TimeSheetId).ToArrayAsync();

        //        foreach (var timeSheetDetail in timeSheetDetails)
        //        {
        //            var oldTimeSheetDetail = existsTimeSheetDetails.FirstOrDefault(x => x.Date == timeSheetDetail.Date && x.EmployeeId == timeSheetDetail.EmployeeId);
        //            var employee = employees.FirstOrDefault(x => x.UserId == timeSheetDetail.EmployeeId);
        //            if (oldTimeSheetDetail != null && mapping.ImportDuplicateOptionId == EnumImportDuplicateOption.Denied)
        //                throw new BadRequestException(GeneralCode.InvalidParams, $"Tồn tại chấm công ngày {timeSheetDetail.Date.ToString("dd/MM/yyyy")} của nhân viên có mã {employee.EmployeeCode}");

        //            if (oldTimeSheetDetail == null)
        //            {
        //                timeSheetDetail.TimeSheetId = timeSheet.TimeSheetId;
        //                await _organizationDBContext.TimeSheetDetail.AddAsync(timeSheetDetail);
        //            }
        //            else if (mapping.ImportDuplicateOptionId == EnumImportDuplicateOption.Update)
        //            {
        //                oldTimeSheetDetail.TimeIn = timeSheetDetail.TimeIn;
        //                oldTimeSheetDetail.TimeOut = timeSheetDetail.TimeOut;
        //                oldTimeSheetDetail.AbsenceTypeSymbolId = timeSheetDetail.AbsenceTypeSymbolId;
        //                oldTimeSheetDetail.MinsEarly = timeSheetDetail.MinsEarly;
        //                oldTimeSheetDetail.MinsLate = timeSheetDetail.MinsLate;
        //                oldTimeSheetDetail.MinsOvertime = timeSheetDetail.MinsOvertime;
        //            }

        //            await _organizationDBContext.SaveChangesAsync();
        //        }

        //        foreach (var timeSheetAggregate in timeSheetAggregates)
        //        {
        //            var oldTimeSheetAggregate = existsTimeSheetAggregates.FirstOrDefault(x => x.EmployeeId == timeSheetAggregate.EmployeeId);
        //            var employee = employees.FirstOrDefault(x => x.UserId == timeSheetAggregate.EmployeeId);
        //            if (oldTimeSheetAggregate != null && mapping.ImportDuplicateOptionId == EnumImportDuplicateOption.Denied)
        //                throw new BadRequestException(GeneralCode.InvalidParams, $"Tồn tại chấm công của nhân viên có mã {employee.EmployeeCode}");

        //            if (oldTimeSheetAggregate == null)
        //            {
        //                var entity = _mapper.Map<TimeSheetAggregate>(timeSheetAggregate);
        //                entity.TimeSheetId = timeSheet.TimeSheetId;
        //                await _organizationDBContext.TimeSheetAggregate.AddAsync(entity);
        //            }
        //            else if (mapping.ImportDuplicateOptionId == EnumImportDuplicateOption.Update)
        //            {
        //                timeSheetAggregate.TimeSheetAggregateId = oldTimeSheetAggregate.TimeSheetAggregateId;
        //                timeSheetAggregate.TimeSheetId = oldTimeSheetAggregate.TimeSheetId;
        //                _mapper.Map(timeSheetAggregate, oldTimeSheetAggregate);
        //            }

        //            await _organizationDBContext.SaveChangesAsync();
        //        }

        //        foreach (var timeSheetDayOff in timeSheetDayOffs)
        //        {
        //            var oldTimeSheetDayOff = existsTimeSheetDayOffs.FirstOrDefault(x => x.EmployeeId == timeSheetDayOff.EmployeeId && x.AbsenceTypeSymbolId == timeSheetDayOff.AbsenceTypeSymbolId);
        //            var employee = employees.FirstOrDefault(x => x.UserId == timeSheetDayOff.EmployeeId);
        //            if (oldTimeSheetDayOff != null && mapping.ImportDuplicateOptionId == EnumImportDuplicateOption.Denied)
        //                throw new BadRequestException(GeneralCode.InvalidParams, $"Tồn tại chấm công của nhân viên có mã {employee.EmployeeCode}");

        //            if (oldTimeSheetDayOff == null)
        //            {
        //                var entity = _mapper.Map<TimeSheetDayOff>(timeSheetDayOff);
        //                entity.TimeSheetId = timeSheet.TimeSheetId;
        //                await _organizationDBContext.TimeSheetDayOff.AddAsync(entity);
        //            }
        //            else if (mapping.ImportDuplicateOptionId == EnumImportDuplicateOption.Update)
        //            {
        //                timeSheetDayOff.TimeSheetDayOffId = oldTimeSheetDayOff.TimeSheetDayOffId;
        //                timeSheetDayOff.TimeSheetId = oldTimeSheetDayOff.TimeSheetId;
        //                _mapper.Map(timeSheetDayOff, oldTimeSheetDayOff);
        //            }

        //            await _organizationDBContext.SaveChangesAsync();
        //        }

        //        foreach (var timeSheetDayOvertime in timeSheetOvertimes)
        //        {
        //            var oldTimeSheetOvertime = existsTimeSheetOvertimes.FirstOrDefault(x => x.EmployeeId == timeSheetDayOvertime.EmployeeId && x.OvertimeLevelId == timeSheetDayOvertime.OvertimeLevelId);
        //            var employee = employees.FirstOrDefault(x => x.UserId == timeSheetDayOvertime.EmployeeId);
        //            if (oldTimeSheetOvertime != null && mapping.ImportDuplicateOptionId == EnumImportDuplicateOption.Denied)
        //                throw new BadRequestException(GeneralCode.InvalidParams, $"Tồn tại chấm công của nhân viên có mã {employee.EmployeeCode}");

        //            if (oldTimeSheetOvertime == null)
        //            {
        //                var entity = _mapper.Map<TimeSheetOvertime>(timeSheetDayOvertime);
        //                entity.TimeSheetId = timeSheet.TimeSheetId;
        //                await _organizationDBContext.TimeSheetOvertime.AddAsync(entity);
        //            }
        //            else if (mapping.ImportDuplicateOptionId == EnumImportDuplicateOption.Update)
        //            {
        //                timeSheetDayOvertime.TimeSheetOvertimeId = oldTimeSheetOvertime.TimeSheetOvertimeId;
        //                timeSheetDayOvertime.TimeSheetId = oldTimeSheetOvertime.TimeSheetId;
        //                _mapper.Map(timeSheetDayOvertime, oldTimeSheetOvertime);
        //            }

        //            await _organizationDBContext.SaveChangesAsync();
        //        }

        //        await _organizationDBContext.SaveChangesAsync();
        //        await trans.CommitAsync();
        //    }
        //    catch (Exception)
        //    {
        //        await trans.RollbackAsync();
        //        throw;
        //    }

        //    return true;
        //}

        public class RowDataImportTimeSheetModel
        {
            public string EmployeeCode { get; set; }
            public NonCamelCaseDictionary<string> row { get; set; }
            public TimeSheetImportFieldModel timeSheetImportModel { get; set; }
        }

        public async Task<bool> ApproveTimeSheet(long timeSheetId)
        {
            var timeSheet = await _organizationDBContext.TimeSheet.FirstOrDefaultAsync(x => x.TimeSheetId == timeSheetId);
            if (timeSheet == null)
                throw new BadRequestException(GeneralCode.ItemNotFound, $"Không tồn tại bảng chấm công có ID {timeSheetId}");

            if (!await _organizationDBContext.TimeSheetAggregate.AnyAsync(d => d.TimeSheetId == timeSheetId))
            {
                throw new BadRequestException("Lưu bảng chấm công trước khi duyệt");
            }

            timeSheet.IsApprove = true;
            await _organizationDBContext.SaveChangesAsync();
            return true;
        }

        private async Task ValidateOverlap(TimeSheetModel model)
        {
            var existTimeSheets = _organizationDBContext.TimeSheet.Where(t => t.Month == model.Month && t.Year == model.Year && t.TimeSheetId != model.TimeSheetId).AsNoTracking();
            var existTimeSheetDepartments = await _organizationDBContext.TimeSheetDepartment.Where(d => existTimeSheets.Select(t => t.TimeSheetId).Contains(d.TimeSheetId)).ToListAsync();

            foreach (var item in model.TimeSheetDepartment)
            {
                var violationDepartments = existTimeSheetDepartments.FirstOrDefault(d => item.DepartmentId == d.DepartmentId);
                if (violationDepartments != null)
                {
                    var department = await _organizationDBContext.Department.FindAsync(item.DepartmentId);
                    throw new BadRequestException($"Đã tồn tại BCC tháng {model.Month}/{model.Year} cho bộ phận \"{department.DepartmentCode} - {department.DepartmentName}\"");
                }
            }
        }

        private async Task RemoveTimeSheetDepartment(long timeSheetId)
        {
            await _organizationDBContext.TimeSheetDepartment
                    .Where(m => m.TimeSheetId == timeSheetId).DeleteByBatch();
        }
        private async Task RemoveTimeSheetAggregateAbsence(List<long> timeSheetAggregateIds)
        {
            await _organizationDBContext.TimeSheetAggregateAbsence
                       .Where(m => timeSheetAggregateIds.Contains(m.TimeSheetAggregateId)).DeleteByBatch();
        }
        private async Task RemoveTimeSheetAggregateOvertime(List<long> timeSheetAggregateIds)
        {
            await _organizationDBContext.TimeSheetAggregateOvertime
                    .Where(m => timeSheetAggregateIds.Contains(m.TimeSheetAggregateId)).DeleteByBatch();
        }
        private async Task RemoveTimeSheetDetailShift(List<long> timeSheetDetailIds)
        {
            await _organizationDBContext.TimeSheetDetailShiftCounted
                    .Where(c => timeSheetDetailIds.Contains(c.TimeSheetDetailId)).DeleteByBatch();

            await _organizationDBContext.TimeSheetDetailShiftOvertime
                    .Where(o => timeSheetDetailIds.Contains(o.TimeSheetDetailId)).DeleteByBatch();

            await _organizationDBContext.TimeSheetDetailShift
                    .Where(m => timeSheetDetailIds.Contains(m.TimeSheetDetailId)).DeleteByBatch();
        }

    }

    public class TimeFrame
    {
        public TimeFrame(double startTime, double endTime)
        {
            StartTime = startTime;
            EndTime = endTime;
        }

        public double StartTime { get; set; }
        public double EndTime { get; set; }

        public override int GetHashCode()
        {
            return StartTime.GetHashCode() ^ EndTime.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is TimeFrame other))
                return false;

            return StartTime == other.StartTime && EndTime == other.EndTime;
        }
    }
}