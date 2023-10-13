using AutoMapper;
using AutoMapper.QueryableExtensions;
using DocumentFormat.OpenXml.Spreadsheet;
using Elasticsearch.Net;
using Microsoft.EntityFrameworkCore;
using Services.Organization.Model.TimeKeeping;
using System;
using System.Collections.Generic;
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
using VErp.Services.Organization.Service.DepartmentCalendar;
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
        Task<List<TimeSheetByEmployeeModel>> GenerateTimeSheet(long timeSheetId, int[] departmentIds, long beginDate, long endDate);

        //CategoryNameModel GetFieldDataForMapping(long beginDate, long endDate);

        //Task<bool> ImportTimeSheetFromMapping(int month, int year, long beginDate, long endDate, ImportExcelMapping mapping, Stream stream);

        Task<bool> ApproveTimeSheet(long timeSheetId);
    }

    public class TimeSheetService : ITimeSheetService
    {
        private const string EmployeeIdField = "F_Id";
        private const string DepartmentIdField = "bo_phan";

        private readonly OrganizationDBContext _organizationDBContext;
        private readonly IDepartmentCalendarService _departmentCalendarService;
        private readonly IShiftScheduleService _shiftScheduleService;
        private readonly ITimeSheetRawService _timeSheetRawService;
        private readonly IMapper _mapper;

        public TimeSheetService(OrganizationDBContext organizationDBContext
            , IMapper mapper
            , IDepartmentCalendarService departmentCalendarService
            , IShiftScheduleService shiftScheduleService
            , ITimeSheetRawService timeSheetRawService)
        {
            _organizationDBContext = organizationDBContext;
            _mapper = mapper;
            _departmentCalendarService = departmentCalendarService;
            _shiftScheduleService = shiftScheduleService;
            _timeSheetRawService = timeSheetRawService;
        }

        public async Task<long> AddTimeSheet(TimeSheetModel model)
        {
            var trans = await _organizationDBContext.Database.BeginTransactionAsync();
            try
            {
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
                var timeSheet = await _organizationDBContext.TimeSheet.FirstOrDefaultAsync(x => x.TimeSheetId == timeSheetId);
                if (timeSheet == null)
                    throw new BadRequestException(GeneralCode.ItemNotFound, $"Không tìm thấy bảng chấm công có ID {timeSheetId}");

                var timeSheetDetails = await _organizationDBContext.TimeSheetDetail.Where(x => x.TimeSheetId == timeSheet.TimeSheetId).ToListAsync();
                var timeSheetAggregates = await _organizationDBContext.TimeSheetAggregate.Where(x => x.TimeSheetId == timeSheet.TimeSheetId).ToListAsync();

                await RemoveTimeSheetDepartment(timeSheetId);

                var existingDetailIds = new HashSet<long>(timeSheetDetails.Select(e => e.TimeSheetDetailId));
                var modelDetailIds = new HashSet<long>(model.TimeSheetDetail.Select(m => m.TimeSheetDetailId));

                var detailsToRemove = timeSheetDetails.Where(e => !modelDetailIds.Contains(e.TimeSheetDetailId)).ToList();
                foreach (var detail in detailsToRemove)
                {
                    detail.IsDeleted = true;
                    await _organizationDBContext.SaveChangesAsync();
                }

                foreach (var mDetail in model.TimeSheetDetail)
                {
                    mDetail.TimeSheetId = timeSheetId;

                    if (existingDetailIds.Contains(mDetail.TimeSheetDetailId))
                    {
                        var eDetailToUpdate = timeSheetDetails.First(e => e.TimeSheetDetailId == mDetail.TimeSheetDetailId);
                        await RemoveTimeSheetDetailShift(eDetailToUpdate.TimeSheetDetailId);

                        _mapper.Map(mDetail, eDetailToUpdate);
                        await _organizationDBContext.SaveChangesAsync();
                    }
                    else
                    {
                        mDetail.TimeSheetDetailId = 0;
                        var newEDetail = _mapper.Map<TimeSheetDetail>(mDetail);
                        await _organizationDBContext.TimeSheetDetail.AddAsync(newEDetail);
                        await _organizationDBContext.SaveChangesAsync();
                    }
                }

                var existingAggregateIds = new HashSet<long>(timeSheet.TimeSheetAggregate.Select(e => e.TimeSheetAggregateId));
                var modelAggregateIds = new HashSet<long>(model.TimeSheetAggregate.Select(m => m.TimeSheetAggregateId));

                var aggregatesToRemove = timeSheet.TimeSheetAggregate.Where(e => !modelAggregateIds.Contains(e.TimeSheetAggregateId)).ToList();
                foreach (var aggregate in aggregatesToRemove)
                {
                    aggregate.IsDeleted = true;
                    await _organizationDBContext.SaveChangesAsync();
                }

                foreach (var mAggregate in model.TimeSheetAggregate)
                {
                    mAggregate.TimeSheetId = timeSheetId;

                    if (existingAggregateIds.Contains(mAggregate.TimeSheetAggregateId))
                    {
                        var eAggregateToUpdate = timeSheet.TimeSheetAggregate.First(e => e.TimeSheetAggregateId == mAggregate.TimeSheetAggregateId);
                        await RemoveTimeSheetAggregateAbsence(eAggregateToUpdate.TimeSheetAggregateId);
                        await RemoveTimeSheetAggregateOvertime(eAggregateToUpdate.TimeSheetAggregateId);
                        mAggregate.TimeSheetId = eAggregateToUpdate.TimeSheetId;
                        _mapper.Map(mAggregate, eAggregateToUpdate);
                        await _organizationDBContext.SaveChangesAsync();
                    }
                    else
                    {
                        mAggregate.TimeSheetAggregateId = 0;
                        var newEAggregate = _mapper.Map<TimeSheetAggregate>(mAggregate);
                        await _organizationDBContext.TimeSheetAggregate.AddAsync(newEAggregate);
                        await _organizationDBContext.SaveChangesAsync();
                    }
                }

                model.TimeSheetId = timeSheet.TimeSheetId;
                model.IsApprove = false;
                _mapper.Map(model, timeSheet);

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

        public async Task<bool> DeleteTimeSheet(long timeSheetId)
        {
            var trans = await _organizationDBContext.Database.BeginTransactionAsync();
            try
            {
                var timeSheet = await _organizationDBContext.TimeSheet.FirstOrDefaultAsync(x => x.TimeSheetId == timeSheetId);
                if (timeSheet == null)
                    throw new BadRequestException(GeneralCode.ItemNotFound, $"Không tồn tại bảng chấm công có ID {timeSheetId}");

                timeSheet.IsDeleted = true;

                var timeSheetDetails = await _organizationDBContext.TimeSheetDetail.Where(x => x.TimeSheetId == timeSheet.TimeSheetId).ToListAsync();
                var timeSheetAggregates = await _organizationDBContext.TimeSheetAggregate.Where(x => x.TimeSheetId == timeSheet.TimeSheetId).ToListAsync();

                timeSheetAggregates.ForEach(x => x.IsDeleted = true);
                timeSheetDetails.ForEach(x => x.IsDeleted = true);
                timeSheet.IsDeleted = true;

                await RemoveTimeSheetDepartment(timeSheetId);

                foreach (var eDetail in timeSheet.TimeSheetDetail)
                {
                    await RemoveTimeSheetDetailShift(eDetail.TimeSheetDetailId);
                }

                foreach (var eAggregate in timeSheet.TimeSheetAggregate)
                {
                    await RemoveTimeSheetAggregateAbsence(eAggregate.TimeSheetAggregateId);
                    await RemoveTimeSheetAggregateOvertime(eAggregate.TimeSheetAggregateId);
                }

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

            if (!String.IsNullOrWhiteSpace(filter.Keyword))
                query = query.Where(t => t.Title.Contains(filter.Keyword));

            query = query.InternalFilter(filter.ColumnsFilters);

            query = query.InternalOrderBy(filter.OrderBy, filter.Asc);

            //if (filter.DepartmentIds != null && filter.DepartmentIds.Count > 0)
            //{

            //}

            var total = query.Count();

            query = size > 0 && page > 0 ? query.Skip((page - 1) * size).Take(size) : query;

            var data = await query.ProjectTo<TimeSheetModel>(_mapper.ConfigurationProvider).ToListAsync();

            return (data, total);
        }

        public async Task<List<TimeSheetByEmployeeModel>> GenerateTimeSheet(long timeSheetId, int[] departmentIds, long beginDate, long endDate)
        {
            var result = new List<TimeSheetByEmployeeModel>();

            var dateRange = new List<long>();
            for (var date = beginDate; date <= endDate; date += 86400)
            {
                dateRange.Add(date);
            }
            var lstEmployees = await _shiftScheduleService.GetEmployeesByDepartments(departmentIds.ToList());
            var employeesByDepartment = lstEmployees.GroupBy(e => (int)e[DepartmentIdField]).ToDictionary(g => g.Key, g => g.ToList());

            var departmentCalendars = await _departmentCalendarService.GetListDepartmentCalendar(departmentIds, beginDate, endDate);

            var dayOffByDepartment = departmentCalendars.GroupBy(x => x.DepartmentId)
                .ToDictionary(g => g.Key, g => g.SelectMany(x => x.DepartmentDayOffCalendar.Where(d => d.Day >= beginDate && d.Day <= endDate)).ToList());

            var allShiftDetails = await _organizationDBContext.ShiftScheduleDetail
                .Where(s => lstEmployees.Select(e => (long)e[EmployeeIdField]).Contains(s.EmployeeId)
                    && dateRange.Select(d => d.UnixToDateTime()).ToList().Contains(s.AssignedDate))
                .ToListAsync();

            var allShiftConfigurations = await _organizationDBContext.ShiftConfiguration
                .Where(s => allShiftDetails.Select(d => d.ShiftConfigurationId).Distinct().Contains(s.ShiftConfigurationId))
                .ProjectTo<ShiftConfigurationModel>(_mapper.ConfigurationProvider)
                .ToListAsync();

            var allTimeSheetRaws = await _timeSheetRawService.GetDistinctTimeSheetRawByEmployee(lstEmployees.Select(e => (long?)e[EmployeeIdField]).ToList());

            var countedSymbols = await _organizationDBContext.CountedSymbol.ProjectTo<CountedSymbolModel>(_mapper.ConfigurationProvider).ToListAsync();

            foreach (var (departmentId, employees) in employeesByDepartment)
            {
                foreach (var employee in employees)
                {
                    var employeeId = (long)employee[EmployeeIdField];

                    var timeSheetByEmployee = new TimeSheetByEmployeeModel();
                    timeSheetByEmployee.EmployeeId = employeeId;

                    var details = new List<TimeSheetDetailModel>();

                    foreach (var date in dateRange)
                    {
                        var detail = new TimeSheetDetailModel();
                        detail.EmployeeId = employeeId;
                        detail.Date = date;
                        detail.TimeSheetId = timeSheetId;

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

                        var shiftsForDate = allShiftDetails.Where(d => d.EmployeeId == employeeId && d.AssignedDate.GetUnix() == date);

                        if (!shiftsForDate.Any())
                        {
                            detail.IsScheduled = false;
                            details.Add(detail);
                            continue;
                        }

                        detail.IsScheduled = true;

                        var shifts = allShiftConfigurations.Where(s => shiftsForDate.Any(sd => sd.ShiftConfigurationId == s.ShiftConfigurationId)).ToList();

                        var timeSheetRaw = allTimeSheetRaws.Where(r => r.EmployeeId == employeeId && r.Date == date);

                        foreach (var shift in shifts)
                        {
                            var detailShift = CreateDetailShift(shift, detail, timeSheetRaw, countedSymbols);

                            if(detailShift != null)
                            {
                                detailShifts.Add(detailShift);
                            }
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

        private TimeSheetDetailShiftModel CreateDetailShift(ShiftConfigurationModel shift, TimeSheetDetailModel detail, IEnumerable<TimeSheetRawModel> timeSheetRaw, List<CountedSymbolModel> countedSymbols)
        {
            var detailShift = new TimeSheetDetailShiftModel();
            detailShift.ShiftConfigurationId = shift.ShiftConfigurationId;
            detailShift.HasOvertimePlan = true;
            detailShift.DateAsOvertimeLevelId = GetOverTimeLevelId(detail.TimeSheetDateType, shift.OvertimeConfiguration, false);

            if (detailShift.DateAsOvertimeLevelId != 0 && detailShift.DateAsOvertimeLevelId != null)
            {
                detailShift.TimeSheetDetailShiftCounted.Add(GetCountedSymbolModel(shift, countedSymbols, EnumCountedSymbol.OvertimeDateSymbol));
            }

            //TimeIn
            double? timeInRaw = null;
            var timeInRaws = timeSheetRaw.Where(r => r.Time > shift.StartTimeOnRecord && r.Time < shift.EndTimeOnRecord).ToList();
            if (timeInRaws.Any())
            {
                timeInRaw = timeInRaws.Min(r => r.Time);
                detailShift.TimeIn = timeInRaw;

                if (timeInRaw <= shift.EntryTime
                    || !shift.IsSubtractionForLate
                    || (shift.IsSubtractionForLate && shift.MinsAllowToLate * 60 > (timeInRaw - shift.EntryTime)))
                {
                    //Đủ công (X)
                    detailShift.TimeSheetDetailShiftCounted.Add(GetCountedSymbolModel(shift, countedSymbols, EnumCountedSymbol.FullCountedSymbol));

                    if (detailShift.HasOvertimePlan && shift.OvertimeConfiguration.OvertimeCalculationMode == EnumOvertimeCalculationMode.ByTotalEarlyLateHours)
                    {
                        //Tăng ca trc giờ (X+)
                        var overtime = GetDetailShiftOvertimeModel(detail, shift);

                        overtime.OvertimeType = EnumTimeSheetOvertimeType.BeforeWork;

                        var actualMinsOvertime = (long)(shift.OvertimeConfiguration.IsMinThresholdMinutesBeforeWork
                                                        && (shift.EntryTime - timeInRaw) > shift.OvertimeConfiguration.MinThresholdMinutesBeforeWork * 60 ?
                                                    (shift.EntryTime - timeInRaw) / 60 : 0);

                        var roundMinutes = shift.OvertimeConfiguration.RoundMinutes;

                        overtime.MinsOvertime = RoundValue(actualMinsOvertime, shift.OvertimeConfiguration.IsRoundBack, (long)roundMinutes);

                        if (overtime.MinsOvertime >= shift.OvertimeConfiguration.MinsReachesBeforeWork)
                        {
                            overtime.MinsOvertime += shift.OvertimeConfiguration.MinsBonusWhenMinsReachesBeforeWork;
                        }

                        if (overtime.MinsOvertime > shift.OvertimeConfiguration.MinsLimitOvertimeBeforeWork)
                        {
                            overtime.MinsOvertime = shift.OvertimeConfiguration.MinsLimitOvertimeBeforeWork;
                        }

                        overtime.MinsOvertime = CheckOvertimeLevelLimit(overtime.MinsOvertime, shift.OvertimeConfiguration, overtime.OvertimeLevelId);

                        if (overtime.MinsOvertime > 0)
                        {
                            detailShift.TimeSheetDetailShiftOvertime.Add(overtime);
                            detailShift.TimeSheetDetailShiftCounted.Add(GetCountedSymbolModel(shift, countedSymbols, EnumCountedSymbol.OvertimeSymbol));
                        }
                    }
                }
                else
                {
                    if (shift.PartialShiftCalculationMode == EnumPartialShiftCalculationMode.CalculateByHalfDay)
                    {
                        //(X-)
                        detailShift.ActualWorkMins = shift.ConvertToMins / 2;
                        detailShift.TimeSheetDetailShiftCounted.Add(GetCountedSymbolModel(shift, countedSymbols, EnumCountedSymbol.HalfWorkOnTimeSymbol));
                    }
                    else
                    {
                        if ((timeInRaw - shift.EntryTime) <= shift.MaxLateMins * 60
                        || ((timeInRaw - shift.EntryTime) > shift.MaxLateMins * 60 && (shift.ExceededLateAbsenceTypeId == null || shift.ExceededLateAbsenceTypeId == 0)))
                        {
                            //Trễ (TR)

                            var actualMinsLate = shift.IsCalculationForLate ? (long)(timeInRaw - shift.EntryTime) : (long)(timeInRaw - shift.EntryTime) - shift.MinsAllowToLate * 60;
                            actualMinsLate /= 60;

                            detailShift.MinsLate = RoundValue((long)actualMinsLate, shift.IsRoundBackForLate, shift.MinsRoundForLate);

                            detailShift.TimeSheetDetailShiftCounted.Add(GetCountedSymbolModel(shift, countedSymbols, EnumCountedSymbol.BeLateSymbol));
                        }
                        else
                        {
                            //Vắng (V)
                            detailShift.AbsenceTypeSymbolId = shift.ExceededLateAbsenceTypeId;
                            detailShift.TimeSheetDetailShiftCounted.Add(GetCountedSymbolModel(shift, countedSymbols, EnumCountedSymbol.AbsentSymbol));
                        }
                    }
                }
            }
            else
            {
                if (shift.IsNoEntryTimeWorkMins)
                {
                    //(X-)
                    detailShift.ActualWorkMins = shift.NoEntryTimeWorkMins;
                    detailShift.TimeSheetDetailShiftCounted.Add(GetCountedSymbolModel(shift, countedSymbols, EnumCountedSymbol.BasedOnActualHoursSymbol));
                }
                else
                {
                    //(V)
                    detailShift.AbsenceTypeSymbolId = shift.NoEntryTimeAbsenceTypeId;
                    detailShift.TimeSheetDetailShiftCounted.Add(GetCountedSymbolModel(shift, countedSymbols, EnumCountedSymbol.AbsentSymbol));
                }
            }

            //TimeOut
            double? timeOutRaw = null;
            var timeOutRaws = timeSheetRaw.Where(r => r.Time > shift.StartTimeOutRecord && r.Time < shift.EndTimeOutRecord).ToList();
            if (timeOutRaws.Any())
            {
                timeOutRaw = timeOutRaws.Max(r => r.Time);
                detailShift.TimeOut = timeOutRaw;

                if (timeOutRaw >= shift.ExitTime
                    || !shift.IsSubtractionForLate
                    || (shift.IsSubtractionForLate && shift.MinsAllowToLate * 60 > (shift.ExitTime - timeOutRaw)))
                {
                    if (detailShift.HasOvertimePlan && shift.OvertimeConfiguration.OvertimeCalculationMode == EnumOvertimeCalculationMode.ByTotalEarlyLateHours)
                    {
                        //Tăng ca sau giờ (X+)
                        var overtime = GetDetailShiftOvertimeModel(detail, shift);

                        overtime.OvertimeType = EnumTimeSheetOvertimeType.AfterWork;

                        var actualMinsOvertime = (long)(shift.OvertimeConfiguration.IsMinThresholdMinutesAfterWork
                                                        && (timeOutRaw - shift.ExitTime) > shift.OvertimeConfiguration.MinThresholdMinutesAfterWork * 60 ?
                                                    (timeOutRaw - shift.ExitTime) / 60 : 0);

                        var roundMinutes = shift.OvertimeConfiguration.RoundMinutes;

                        overtime.MinsOvertime = RoundValue(actualMinsOvertime, shift.OvertimeConfiguration.IsRoundBack, (long)roundMinutes);

                        if (overtime.MinsOvertime >= shift.OvertimeConfiguration.MinsReachesAfterWork)
                        {
                            overtime.MinsOvertime += shift.OvertimeConfiguration.MinsBonusWhenMinsReachesAfterWork;
                        }

                        if (overtime.MinsOvertime > shift.OvertimeConfiguration.MinsLimitOvertimeAfterWork)
                        {
                            overtime.MinsOvertime = shift.OvertimeConfiguration.MinsLimitOvertimeAfterWork;
                        }

                        overtime.MinsOvertime = CheckOvertimeLevelLimit(overtime.MinsOvertime, shift.OvertimeConfiguration, overtime.OvertimeLevelId);

                        if (overtime.MinsOvertime > 0)
                        {
                            detailShift.TimeSheetDetailShiftOvertime.Add(overtime);
                            detailShift.TimeSheetDetailShiftCounted.Add(GetCountedSymbolModel(shift, countedSymbols, EnumCountedSymbol.OvertimeSymbol));
                        }
                    }
                }
                else
                {
                    if (shift.PartialShiftCalculationMode == EnumPartialShiftCalculationMode.CalculateByHalfDay)
                    {
                        //(X-)
                        detailShift.ActualWorkMins = shift.ConvertToMins / 2;
                        detailShift.TimeSheetDetailShiftCounted.Add(GetCountedSymbolModel(shift, countedSymbols, EnumCountedSymbol.HalfWorkOnTimeSymbol));
                    }
                    else
                    {
                        if ((shift.ExitTime - timeOutRaw) <= shift.MaxEarlyMins * 60
                            || ((shift.ExitTime - timeOutRaw) > shift.MaxEarlyMins * 60 && (shift.ExceededEarlyAbsenceTypeId == null || shift.ExceededEarlyAbsenceTypeId == 0)))
                        {
                            //Sớm (SM)
                            var actualMinsEarly = shift.IsCalculationForEarly ? (long)(shift.ExitTime - timeOutRaw) : (long)(shift.ExitTime - timeOutRaw) - shift.MinsAllowToEarly * 60;
                            actualMinsEarly /= 60;

                            detailShift.MinsEarly = RoundValue(actualMinsEarly, shift.IsRoundBackForEarly, shift.MinsRoundForEarly);

                            detailShift.TimeSheetDetailShiftCounted.Add(GetCountedSymbolModel(shift, countedSymbols, EnumCountedSymbol.EarlySymbol));
                        }
                        else
                        {
                            //Vắng (V)
                            detailShift.AbsenceTypeSymbolId = shift.ExceededEarlyAbsenceTypeId;
                            detailShift.TimeSheetDetailShiftCounted.Add(GetCountedSymbolModel(shift, countedSymbols, EnumCountedSymbol.AbsentSymbol));
                        }
                    }
                }
            }
            else
            {
                if (shift.IsNoExitTimeWorkMins)
                {
                    //(X-)
                    detailShift.ActualWorkMins = shift.NoExitTimeWorkMins;
                    detailShift.TimeSheetDetailShiftCounted.Add(GetCountedSymbolModel(shift, countedSymbols, EnumCountedSymbol.BasedOnActualHoursSymbol));
                }
                else
                {
                    //(V)
                    detailShift.AbsenceTypeSymbolId = shift.NoExitTimeAbsenceTypeId;

                    detailShift.TimeSheetDetailShiftCounted.Add(GetCountedSymbolModel(shift, countedSymbols, EnumCountedSymbol.AbsentSymbol));
                }
            }

            if (timeInRaw == null && timeOutRaw == null)
            {
                if (detail.TimeSheetDateType == EnumTimeSheetDateType.Weekend && (shift.IsSkipSaturdayWithShift || shift.IsSkipSundayWithShift))
                {
                    detail.IsScheduled = false;
                    return null;
                }

                if (detail.TimeSheetDateType == EnumTimeSheetDateType.Holiday && shift.IsCountWorkForHoliday)
                {
                    //Đủ công (X)
                    detailShift.TimeSheetDetailShiftCounted.Add(GetCountedSymbolModel(shift, countedSymbols, EnumCountedSymbol.FullCountedSymbol));
                }
            }

            //Tăng ca tổng hợp
            if (shift.OvertimeConfiguration.OvertimeCalculationMode == EnumOvertimeCalculationMode.ByActualWorkingHours
                && timeInRaw != null && timeInRaw < shift.EntryTime && timeOutRaw != null && timeOutRaw > shift.ExitTime)
            {
                var overtime = GetDetailShiftOvertimeModel(detail, shift);

                overtime.OvertimeType = EnumTimeSheetOvertimeType.Default;

                var actualTime = timeOutRaw - timeInRaw;

                var actualMinsOvertime = (long)(shift.OvertimeConfiguration.IsOvertimeThresholdMins
                                                && (actualTime - shift.ExitTime + shift.EntryTime) > shift.OvertimeConfiguration.OvertimeThresholdMins * 60 ?
                                            (actualTime - shift.ExitTime + shift.EntryTime) / 60 : 0);

                var roundMinutes = shift.OvertimeConfiguration.RoundMinutes;

                overtime.MinsOvertime = RoundValue(actualMinsOvertime, shift.OvertimeConfiguration.IsRoundBack, (long)roundMinutes);

                if (overtime.MinsOvertime >= shift.OvertimeConfiguration.MinsReaches)
                {
                    overtime.MinsOvertime += shift.OvertimeConfiguration.MinsBonusWhenMinsReaches;
                }

                if (overtime.MinsOvertime > shift.OvertimeConfiguration.MinsLimitOvertime)
                {
                    overtime.MinsOvertime = shift.OvertimeConfiguration.MinsLimitOvertime;
                }

                overtime.MinsOvertime = CheckOvertimeLevelLimit(overtime.MinsOvertime, shift.OvertimeConfiguration, overtime.OvertimeLevelId);

                if (overtime.MinsOvertime > 0)
                {
                    detailShift.TimeSheetDetailShiftOvertime.Add(overtime);

                    detailShift.TimeSheetDetailShiftCounted.Add(GetCountedSymbolModel(shift, countedSymbols, EnumCountedSymbol.OvertimeSymbol));
                }
            }

            return detailShift;
        }

        private TimeSheetDetailShiftCountedModel GetCountedSymbolModel(ShiftConfigurationModel shift, List<CountedSymbolModel> countedSymbols, EnumCountedSymbol symbol)
        {
            return new TimeSheetDetailShiftCountedModel()
            {
                ShiftConfigurationId = shift.ShiftConfigurationId,
                CountedSymbolId = countedSymbols.FirstOrDefault(c => c.CountedSymbolType == (int)symbol).CountedSymbolId
            };
        }
        private TimeSheetDetailShiftOvertimeModel GetDetailShiftOvertimeModel(TimeSheetDetailModel detail, ShiftConfigurationModel shift)
        {
            return new TimeSheetDetailShiftOvertimeModel()
            {
                ShiftConfigurationId = shift.ShiftConfigurationId,
                OvertimeLevelId = (int)GetOverTimeLevelId(detail.TimeSheetDateType, shift.OvertimeConfiguration, true)
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

        private long CheckOvertimeLevelLimit(long minsOvertime, OvertimeConfigurationModel overtimeConfig, int overtimeLevelId)
        {
            var limitOvertimeLevel = overtimeConfig.OvertimeConfigurationMapping.FirstOrDefault(o => o.OvertimeLevelId == overtimeLevelId);
            if (limitOvertimeLevel != null && minsOvertime > limitOvertimeLevel.MinsLimit)
            {
                return limitOvertimeLevel.MinsLimit;
            }
            return minsOvertime;
        }

        private int? GetOverTimeLevelId(EnumTimeSheetDateType dateType, OvertimeConfigurationModel overtimeConfig, bool isOvertime)
        {
            int? overTimeLevelId;
            switch (dateType)
            {
                case EnumTimeSheetDateType.Weekend:
                    if (isOvertime)
                    {
                        overTimeLevelId = overtimeConfig.IsWeekendOvertimeLevel ? overtimeConfig.WeekendOvertimeLevel : 0;
                    }
                    else
                    {
                        overTimeLevelId = overtimeConfig.IsWeekendLevel ? overtimeConfig.WeekendLevel : 0;
                    }
                    break;
                case EnumTimeSheetDateType.Holiday:
                    if (isOvertime)
                    {
                        overTimeLevelId = overtimeConfig.IsHolidayOvertimeLevel ? overtimeConfig.HolidayOvertimeLevel : 0;
                    }
                    else
                    {
                        overTimeLevelId = overtimeConfig.IsHolidayLevel ? overtimeConfig.HolidayLevel : 0;
                    }
                    break;
                default:
                    if (isOvertime)
                    {
                        overTimeLevelId = overtimeConfig.IsWeekdayOvertimeLevel ? overtimeConfig.WeekdayOvertimeLevel : 0;
                    }
                    else
                    {
                        overTimeLevelId = overtimeConfig.IsWeekdayLevel ? overtimeConfig.WeekdayLevel : 0;
                    }
                    break;
            }
            return overTimeLevelId;
        }
        private async Task<List<ShiftConfigurationModel>> GetShiftsByEmployee(long employeeId, long date)
        {
            var shiftConfigurationIds = _organizationDBContext.ShiftScheduleDetail
                .Where(s => s.EmployeeId == employeeId && s.AssignedDate == date.UnixToDateTime())
                .Select(d => d.ShiftConfigurationId)
                .Distinct().ToList();

            return await _organizationDBContext.ShiftConfiguration
                .Where(s => shiftConfigurationIds
                .Contains(s.ShiftConfigurationId))
                .ProjectTo<ShiftConfigurationModel>(_mapper.ConfigurationProvider)
                .ToListAsync();
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
            var timeSheet = await _organizationDBContext.TimeSheet
            .FirstOrDefaultAsync(x => x.TimeSheetId == timeSheetId);
            if (timeSheet == null)
                throw new BadRequestException(GeneralCode.ItemNotFound, $"Không tồn tại bảng chấm công có ID {timeSheetId}");

            timeSheet.IsApprove = true;
            await _organizationDBContext.SaveChangesAsync();
            return true;
        }

        private async Task RemoveTimeSheetDepartment(long timeSheetId)
        {
            var entity = _organizationDBContext.TimeSheetDepartment
                    .Where(m => m.TimeSheetId == timeSheetId).AsNoTracking();
            _organizationDBContext.TimeSheetDepartment.RemoveRange(entity);
            await _organizationDBContext.SaveChangesAsync();
        }
        private async Task RemoveTimeSheetAggregateAbsence(long timeSheetAggregateId)
        {
            var entity = _organizationDBContext.TimeSheetAggregateAbsence
                    .Where(m => m.TimeSheetAggregateId == timeSheetAggregateId).AsNoTracking();
            var lst = entity.ToList();
            _organizationDBContext.TimeSheetAggregateAbsence.RemoveRange(entity);
            await _organizationDBContext.SaveChangesAsync();
        }
        private async Task RemoveTimeSheetAggregateOvertime(long timeSheetAggregateId)
        {
            var entity = _organizationDBContext.TimeSheetAggregateOvertime
                    .Where(m => m.TimeSheetAggregateId == timeSheetAggregateId).AsNoTracking();
            _organizationDBContext.TimeSheetAggregateOvertime.RemoveRange(entity);
            await _organizationDBContext.SaveChangesAsync();
        }
        private async Task RemoveTimeSheetDetailShift(long timeSheetDetailId)
        {
            var countedEntities = _organizationDBContext.TimeSheetDetailShiftCounted
                    .Where(c => c.TimeSheetDetailId == timeSheetDetailId).AsNoTracking();
            _organizationDBContext.TimeSheetDetailShiftCounted.RemoveRange(countedEntities);

            var overtimeEntities = _organizationDBContext.TimeSheetDetailShiftOvertime
                    .Where(o => o.TimeSheetDetailId == timeSheetDetailId).AsNoTracking();
            _organizationDBContext.TimeSheetDetailShiftOvertime.RemoveRange(overtimeEntities);

            var entity = _organizationDBContext.TimeSheetDetailShift
                    .Where(m => m.TimeSheetDetailId == timeSheetDetailId).AsNoTracking();
            _organizationDBContext.TimeSheetDetailShift.RemoveRange(entity);
            await _organizationDBContext.SaveChangesAsync();
        }

    }
}