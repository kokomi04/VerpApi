using AutoMapper;
using AutoMapper.QueryableExtensions;
using DocumentFormat.OpenXml.Spreadsheet;
using Elasticsearch.Net;
using Microsoft.EntityFrameworkCore;
using Services.Organization.Model.TimeKeeping;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
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
        Task<List<TimeSheetByEmployeeModel>> GenerateTimeSheet(long timeSheetId, int[] departmentIds, long beginDate, long endDate);
        Task<TimeSheetDetailModel> SingleTimeKeeping(TimeSheetDetailRequestModel model);

        //CategoryNameModel GetFieldDataForMapping(long beginDate, long endDate);

        //Task<bool> ImportTimeSheetFromMapping(int month, int year, long beginDate, long endDate, ImportExcelMapping mapping, Stream stream);

        Task<bool> ApproveTimeSheet(long timeSheetId);
    }

    public class TimeSheetService : ITimeSheetService
    {
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
                if (_organizationDBContext.TimeSheet.Any(t => t.Title == model.Title))
                {
                    throw new BadRequestException("Tên bảng chấm công đã tồn tại");
                }

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
            var absences = await _organizationDBContext.AbsenceTypeSymbol.Where(a => a.IsUsed).ProjectTo<AbsenceTypeSymbolModel>(_mapper.ConfigurationProvider).ToListAsync();
            var countedSymbols = await _organizationDBContext.CountedSymbol.ProjectTo<CountedSymbolModel>(_mapper.ConfigurationProvider).ToListAsync();

            var shiftIds = model.TimeSheetDetail.TimeSheetDetailShift.Select(ds => ds.ShiftConfigurationId).ToList();
            var shifts = await _organizationDBContext.ShiftConfiguration
                .Where(s => shiftIds.Contains(s.ShiftConfigurationId))
                .ProjectTo<ShiftConfigurationModel>(_mapper.ConfigurationProvider)
                .ToListAsync();

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
                        if(shiftsWithoutNight.Count() == 1 || (earliestShift != null && lastestShift != null && earliestShift.ShiftConfigurationId == shift.ShiftConfigurationId && earliestShift.ShiftConfigurationId == lastestShift.ShiftConfigurationId))
                        {
                            detailShift = CreateDetailShift(shift, model.TimeSheetDetail, timeIn, timeOut, countedSymbols, absences);
                        }
                        else 
                        {
                            if (earliestShift != null && earliestShift.ShiftConfigurationId == shift.ShiftConfigurationId)
                            {
                                detailShift = CreateDetailShift(shift, model.TimeSheetDetail, timeIn, (timeIn.HasValue && timeOut.HasValue) ? shift.ExitTime : null, countedSymbols, absences);
                            }
                            else if (lastestShift != null && lastestShift.ShiftConfigurationId == shift.ShiftConfigurationId)
                            {
                                detailShift = CreateDetailShift(shift, model.TimeSheetDetail, (timeIn.HasValue && timeOut.HasValue) ? shift.EntryTime : null, timeOut, countedSymbols, absences);
                            }
                            else if (!timeIn.HasValue || !timeOut.HasValue || shift.EntryTime < earliestShift.EntryTime || shift.ExitTime > lastestShift.ExitTime)
                            {
                                detailShift = CreateDetailShift(shift, model.TimeSheetDetail, null, null, countedSymbols, absences);
                            }
                            else
                            {
                                detailShift = CreateDetailShift(shift, model.TimeSheetDetail, shift.EntryTime, shift.ExitTime, countedSymbols, absences);
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

                        SetDetailShiftForAbsence(detailShift, EnumTimeSheetDateType.Weekday, absenceTypeSymbolId, shift, countedSymbols, absences);

                        lstDetailShift.Add(detailShift);
                    }

                    break;

                default:
                    foreach (var shift in shifts)
                    {
                        var detailShift = model.TimeSheetDetail.TimeSheetDetailShift.FirstOrDefault(s => s.ShiftConfigurationId == shift.ShiftConfigurationId);

                        detailShift = CreateDetailShift(shift, model.TimeSheetDetail, detailShift.TimeIn, detailShift.TimeOut, countedSymbols, absences);

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

        public async Task<List<TimeSheetByEmployeeModel>> GenerateTimeSheet(long timeSheetId, int[] departmentIds, long beginDate, long endDate)
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

            var allShiftDetails = await _organizationDBContext.ShiftScheduleDetail
                .Where(s => lstEmployees.Select(e => (long)e[EmployeeConstants.EMPLOYEE_ID]).Contains(s.EmployeeId)
                    && dateRange.Select(d => d.UnixToDateTime()).ToList().Contains(s.AssignedDate))
                .ToListAsync();

            var allShiftConfigurations = await _organizationDBContext.ShiftConfiguration
                .Where(s => allShiftDetails.Select(d => d.ShiftConfigurationId).Distinct().Contains(s.ShiftConfigurationId))
                .ProjectTo<ShiftConfigurationModel>(_mapper.ConfigurationProvider)
                .ToListAsync();

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

                        var shiftsForDate = allShiftDetails.Where(d => d.EmployeeId == employeeId && d.AssignedDate.GetUnix() == date);

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


                            var detailShift = CreateDetailShift(shift, detail, timeInRaw, timeOutRaw, countedSymbols, absences);

                            if (detailShift != null)
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

        private TimeSheetDetailShiftModel CreateDetailShift(ShiftConfigurationModel shift, TimeSheetDetailModel detail, double? timeInRaw, double? timeOutRaw, List<CountedSymbolModel> countedSymbols, List<AbsenceTypeSymbolModel> absences)
        {
            var detailShift = new TimeSheetDetailShiftModel();
            detailShift.ShiftConfigurationId = shift.ShiftConfigurationId;
            detailShift.HasOvertimePlan = true;
            detailShift.DateAsOvertimeLevelId = GetOverTimeLevelId(detail.TimeSheetDateType, shift.OvertimeConfiguration, false);

            if (detailShift.DateAsOvertimeLevelId != 0 && detailShift.DateAsOvertimeLevelId != null)
            {
                detailShift.TimeSheetDetailShiftCounted.Add(GetCountedSymbolModel(shift, countedSymbols, EnumCountedSymbol.OvertimeDateSymbol));
            }

            if (timeInRaw.HasValue)
            {
                detailShift.TimeIn = timeInRaw;

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

                    if (detailShift.HasOvertimePlan && shift.OvertimeConfiguration.OvertimeCalculationMode == EnumOvertimeCalculationMode.ByTotalEarlyLateHours)
                    {
                        //Tăng ca trc giờ (X+)
                        CalcOvertime(detailShift, detail, shift, EnumTimeSheetOvertimeType.BeforeWork, countedSymbols, timeInRaw, null);
                    }
                }
                else
                {
                    SetsMinsLate(detailShift, shift, countedSymbols, timeInRaw);

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
                            if (!SetDetailShiftForAbsence(detailShift, detail.TimeSheetDateType, shift.ExceededLateAbsenceTypeId, shift, countedSymbols, absences))
                            {
                                return null;
                            }
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
                    if(!SetDetailShiftForAbsence(detailShift, detail.TimeSheetDateType, shift.NoEntryTimeAbsenceTypeId, shift, countedSymbols, absences))
                    {
                        return null;
                    }
                }
            }

            if (timeOutRaw.HasValue)
            {
                detailShift.TimeOut = timeOutRaw;

                if (timeOutRaw >= shift.ExitTime
                    || shift.MinsAllowToEarly * 60 >= (shift.ExitTime - timeOutRaw))
                {
                    if (detailShift.HasOvertimePlan && shift.OvertimeConfiguration.OvertimeCalculationMode == EnumOvertimeCalculationMode.ByTotalEarlyLateHours)
                    {
                        //Tăng ca sau giờ (X+)
                        CalcOvertime(detailShift, detail, shift, EnumTimeSheetOvertimeType.AfterWork, countedSymbols, null, timeOutRaw);
                    }
                }
                else
                {
                    SetsMinsEarly(detailShift, shift, countedSymbols, timeOutRaw);

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
                            if (!SetDetailShiftForAbsence(detailShift, detail.TimeSheetDateType, shift.ExceededEarlyAbsenceTypeId, shift, countedSymbols, absences))
                            {
                                return null;
                            }
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
                    if (!SetDetailShiftForAbsence(detailShift, detail.TimeSheetDateType, shift.NoExitTimeAbsenceTypeId, shift, countedSymbols, absences))
                    {
                        return null;
                    }
                }
            }

            if (timeInRaw == null && timeOutRaw == null)
            {
                if(!SetDetailShiftForAbsence(detailShift, detail.TimeSheetDateType, null, shift, countedSymbols, absences))
                {
                    return null;
                }
            }

            if (detailShift.HasOvertimePlan && shift.OvertimeConfiguration.OvertimeCalculationMode == EnumOvertimeCalculationMode.ByActualWorkingHours
                && ((timeInRaw != null && timeInRaw < shift.EntryTime) || (timeOutRaw != null && timeOutRaw > shift.ExitTime)))
            {
                //Tăng ca tổng hợp
                CalcOvertime(detailShift, detail, shift, EnumTimeSheetOvertimeType.Default, countedSymbols, timeInRaw, timeOutRaw);
            }

            var s = countedSymbols.FirstOrDefault(c => c.CountedSymbolType == EnumCountedSymbol.OvertimeDateSymbol);
            if (!detailShift.TimeSheetDetailShiftCounted.Where(c => c.CountedSymbolId != s.CountedSymbolId).Any())
                detailShift.TimeSheetDetailShiftCounted.Add(GetCountedSymbolModel(shift, countedSymbols, shift.IsNightShift ? EnumCountedSymbol.ShiftNightSymbol : EnumCountedSymbol.FullCountedSymbol));

            return detailShift;
        }

        private void CalcOvertime(
            TimeSheetDetailShiftModel detailShift,
            TimeSheetDetailModel detail,
            ShiftConfigurationModel shift,
            EnumTimeSheetOvertimeType overtimeType,
            List<CountedSymbolModel> countedSymbols,
            double? timeInRaw, double? timeOutRaw)
        {

            var overtime = GetDetailShiftOvertimeModel(detail, shift);
            overtime.OvertimeType = overtimeType;

            long minsReaches;
            long minsBonus;
            long minsLimit;
            long actualMinsOvertime;

            switch (overtimeType)
            {
                case EnumTimeSheetOvertimeType.BeforeWork:
                    minsReaches = shift.OvertimeConfiguration.MinsReachesBeforeWork;
                    minsBonus = shift.OvertimeConfiguration.MinsBonusWhenMinsReachesBeforeWork;
                    minsLimit = shift.OvertimeConfiguration.MinsLimitOvertimeBeforeWork;
                    actualMinsOvertime = (long)(shift.OvertimeConfiguration.IsMinThresholdMinutesBeforeWork
                                        && (shift.EntryTime - timeInRaw) > shift.OvertimeConfiguration.MinThresholdMinutesBeforeWork * 60 ?
                                    (shift.EntryTime - timeInRaw) / 60 : 0);

                    break;
                case EnumTimeSheetOvertimeType.AfterWork:
                    minsReaches = shift.OvertimeConfiguration.MinsReachesAfterWork;
                    minsBonus = shift.OvertimeConfiguration.MinsBonusWhenMinsReachesAfterWork;
                    minsLimit = shift.OvertimeConfiguration.MinsLimitOvertimeAfterWork;
                    actualMinsOvertime = (long)(shift.OvertimeConfiguration.IsMinThresholdMinutesAfterWork
                                        && (timeOutRaw - shift.ExitTime) > shift.OvertimeConfiguration.MinThresholdMinutesAfterWork * 60 ?
                                    (timeOutRaw - shift.ExitTime) / 60 : 0);
                    break;
                default:
                    minsReaches = shift.OvertimeConfiguration.MinsReaches;
                    minsBonus = shift.OvertimeConfiguration.MinsBonusWhenMinsReaches;
                    minsLimit = shift.OvertimeConfiguration.MinsLimitOvertime;
                    var actualTime = timeOutRaw - timeInRaw;
                    actualMinsOvertime = (long)(shift.OvertimeConfiguration.IsOvertimeThresholdMins
                                        && (actualTime - shift.ExitTime + shift.EntryTime) > shift.OvertimeConfiguration.OvertimeThresholdMins * 60 ?
                                    (actualTime - shift.ExitTime + shift.EntryTime) / 60 : 0);
                    break;
            }

            var roundMinutes = shift.OvertimeConfiguration.RoundMinutes ?? 0;
            overtime.MinsOvertime = RoundValue(actualMinsOvertime, shift.OvertimeConfiguration.IsRoundBack, (long)roundMinutes);

            if (overtime.MinsOvertime >= minsReaches)
            {
                overtime.MinsOvertime += minsBonus;
            }

            if (overtime.MinsOvertime > minsLimit)
            {
                overtime.MinsOvertime = minsLimit;
            }

            overtime.MinsOvertime = CheckOvertimeLevelLimit(overtime.MinsOvertime, shift.OvertimeConfiguration, overtime.OvertimeLevelId);

            if (overtime.MinsOvertime > 0)
            {
                detailShift.TimeSheetDetailShiftOvertime.Add(overtime);
                detailShift.TimeSheetDetailShiftCounted.Add(GetCountedSymbolModel(shift, countedSymbols, EnumCountedSymbol.OvertimeSymbol));
            }
        }

        private bool SetDetailShiftForAbsence(TimeSheetDetailShiftModel detailShift, EnumTimeSheetDateType timeSheetDateType, int? absenceTypeSymbolId, ShiftConfigurationModel shift, List<CountedSymbolModel> countedSymbols, List<AbsenceTypeSymbolModel> absences)
        {
            if (timeSheetDateType == EnumTimeSheetDateType.Holiday && shift.IsCountWorkForHoliday)
            {
                //Đủ công (X)
                detailShift.WorkCounted = shift.ConfirmationUnit;
                detailShift.ActualWorkMins = shift.ConvertToMins;
                detailShift.AbsenceTypeSymbolId = null;
                detailShift.TimeSheetDetailShiftCounted.Clear();
                detailShift.TimeSheetDetailShiftCounted.Add(GetCountedSymbolModel(shift, countedSymbols, EnumCountedSymbol.FullCountedSymbol));
            }
            else if ((timeSheetDateType == EnumTimeSheetDateType.Weekend && shift.IsSkipWeeklyOffDayWithShift) || (timeSheetDateType == EnumTimeSheetDateType.Holiday && shift.IsSkipHolidayWithShift))
            {
                return false;
            }
            else
            {
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
                detailShift.TimeSheetDetailShiftCounted.Add(GetCountedSymbolModel(shift, countedSymbols, EnumCountedSymbol.AbsentSymbol));
            }

            return true;
        }

        private TimeSheetDetailShiftCountedModel GetCountedSymbolModel(ShiftConfigurationModel shift, List<CountedSymbolModel> countedSymbols, EnumCountedSymbol symbol)
        {
            return new TimeSheetDetailShiftCountedModel()
            {
                ShiftConfigurationId = shift.ShiftConfigurationId,
                CountedSymbolId = countedSymbols.FirstOrDefault(c => c.CountedSymbolType == symbol).CountedSymbolId
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

        private void SetsMinsLate(TimeSheetDetailShiftModel detailShift, ShiftConfigurationModel shift, List<CountedSymbolModel> countedSymbols, double? timeInRaw)
        {
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
}