using AutoMapper;
using AutoMapper.QueryableExtensions;
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
using VErp.Infrastructure.EF.OrganizationDB;
using static VErp.Commons.Library.ExcelReader;

namespace VErp.Services.Organization.Service.TimeKeeping
{
    public interface ITimeSheetService
    {
        Task<long> AddTimeSheet(TimeSheetModel model);
        Task<bool> DeleteTimeSheet(int year, int month);
        Task<IList<TimeSheetModel>> GetListTimeSheet();
        Task<TimeSheetModel> GetTimeSheet(int year, int month);
        Task<TimeSheetModel> GetTimeSheetByEmployee(int year, int month, int employeeId);
        Task<bool> UpdateTimeSheet(int year, int month, TimeSheetModel model);

        CategoryNameModel GetFieldDataForMapping(long beginDate, long endDate);

        Task<bool> ImportTimeSheetFromMapping(int month, int year, long beginDate, long endDate, ImportExcelMapping mapping, Stream stream);

        Task<bool> ApproveTimeSheet(int year, int month);
    }

    public class TimeSheetService : ITimeSheetService
    {
        private readonly OrganizationDBContext _organizationDBContext;
        private readonly IMapper _mapper;

        public TimeSheetService(OrganizationDBContext organizationDBContext, IMapper mapper)
        {
            _organizationDBContext = organizationDBContext;
            _mapper = mapper;
        }

        public async Task<long> AddTimeSheet(TimeSheetModel model)
        {
            if (_organizationDBContext.TimeSheet.Any(x => x.Year == model.Year && x.Month == model.Month))
                throw new BadRequestException(GeneralCode.ItemCodeExisted, $"Đã tồn tại bảng chấm công tháng {model.Month} năm {model.Year}");

            var trans = await _organizationDBContext.Database.BeginTransactionAsync();
            try
            {
                model.IsApprove = false;
                var entity = _mapper.Map<TimeSheet>(model);

                await _organizationDBContext.TimeSheet.AddAsync(entity);
                await _organizationDBContext.SaveChangesAsync();

                if (model.TimeSheetDetails.Count > 0)
                {
                    foreach (var detail in model.TimeSheetDetails)
                    {
                        if (detail.TimeOut < detail.TimeIn)
                            throw new BadRequestException(GeneralCode.InvalidParams, "Thời gian vào phải nhỏ hơn thời gian ra");
                        var eDetail = _mapper.Map<TimeSheetDetail>(detail);
                        eDetail.TimeSheetId = entity.TimeSheetId;

                        await _organizationDBContext.TimeSheetDetail.AddAsync(eDetail);
                    }

                    await _organizationDBContext.SaveChangesAsync();
                }

                if (model.TimeSheetDayOffs.Count > 0)
                {
                    foreach (var dayOff in model.TimeSheetDayOffs)
                    {
                        var eDayOff = _mapper.Map<TimeSheetDayOff>(dayOff);
                        eDayOff.TimeSheetId = entity.TimeSheetId;

                        await _organizationDBContext.TimeSheetDayOff.AddAsync(eDayOff);
                    }

                    await _organizationDBContext.SaveChangesAsync();
                }

                if (model.TimeSheetAggregates.Count > 0)
                {
                    foreach (var aggregate in model.TimeSheetAggregates)
                    {
                        var eAggregate = _mapper.Map<TimeSheetAggregate>(aggregate);
                        eAggregate.TimeSheetId = entity.TimeSheetId;

                        await _organizationDBContext.TimeSheetAggregate.AddAsync(eAggregate);
                    }

                    await _organizationDBContext.SaveChangesAsync();
                }

                if (model.TimeSheetOvertimes.Count > 0)
                {
                    foreach (var overtime in model.TimeSheetOvertimes)
                    {
                        var eOvertime = _mapper.Map<TimeSheetOvertime>(overtime);
                        eOvertime.TimeSheetId = entity.TimeSheetId;

                        await _organizationDBContext.TimeSheetOvertime.AddAsync(eOvertime);
                    }

                    await _organizationDBContext.SaveChangesAsync();
                }

                await trans.CommitAsync();

                return entity.TimeSheetId;
            }
            catch (Exception)
            {
                await trans.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> UpdateTimeSheet(int year, int month, TimeSheetModel model)
        {
            var trans = await _organizationDBContext.Database.BeginTransactionAsync();
            try
            {
                var timeSheet = await _organizationDBContext.TimeSheet.FirstOrDefaultAsync(x => x.Year == year && x.Month == month);
                if (timeSheet == null)
                    throw new BadRequestException(GeneralCode.ItemNotFound, $"Không tồn tại bảng chấm công tháng {month} năm {year}");

                var timeSheetDetails = await _organizationDBContext.TimeSheetDetail.Where(x => x.TimeSheetId == timeSheet.TimeSheetId).ToListAsync();
                var timeSheetAggregates = await _organizationDBContext.TimeSheetAggregate.Where(x => x.TimeSheetId == timeSheet.TimeSheetId).ToListAsync();
                var timeSheetDayOffs = await _organizationDBContext.TimeSheetDayOff.Where(x => x.TimeSheetId == timeSheet.TimeSheetId).ToListAsync();
                var timeSheetOvertimes = await _organizationDBContext.TimeSheetOvertime.Where(x => x.TimeSheetId == timeSheet.TimeSheetId).ToListAsync();

                model.TimeSheetId = timeSheet.TimeSheetId;
                model.IsApprove = false;
                _mapper.Map(model, timeSheet);

                foreach (var eDetail in timeSheetDetails)
                {
                    var mDetail = model.TimeSheetDetails.FirstOrDefault(x => x.TimeSheetDetailId == eDetail.TimeSheetDetailId);
                    if (mDetail != null)
                    {
                        if (mDetail.TimeOut < mDetail.TimeIn)
                            throw new BadRequestException(GeneralCode.InvalidParams, "Thời gian vào phải nhỏ hơn thời gian ra");
                        _mapper.Map(mDetail, eDetail);
                    }
                    else
                        eDetail.IsDeleted = true;
                }

                foreach (var detail in model.TimeSheetDetails.Where(x => x.TimeSheetDetailId <= 0))
                {
                    if (detail.TimeOut < detail.TimeIn)
                        throw new BadRequestException(GeneralCode.InvalidParams, "Thời gian vào phải nhỏ hơn thời gian ra");
                    var eDetail = _mapper.Map<TimeSheetDetail>(detail);
                    eDetail.TimeSheetId = timeSheet.TimeSheetId;

                    await _organizationDBContext.TimeSheetDetail.AddAsync(eDetail);
                }


                foreach (var eAggregate in timeSheetAggregates)
                {
                    var mAggregate = model.TimeSheetAggregates.FirstOrDefault(x => x.TimeSheetAggregateId == eAggregate.TimeSheetAggregateId);
                    if (mAggregate != null)
                        _mapper.Map(mAggregate, eAggregate);
                    else
                        eAggregate.IsDeleted = true;
                }

                foreach (var eOvertime in timeSheetOvertimes)
                {
                    var mOvertime = model.TimeSheetOvertimes.FirstOrDefault(x => x.TimeSheetOvertimeId == eOvertime.TimeSheetOvertimeId);
                    if (mOvertime != null)
                        _mapper.Map(mOvertime, eOvertime);
                    else
                        eOvertime.IsDeleted = true;
                }

                foreach (var aggregate in model.TimeSheetAggregates.Where(x => x.TimeSheetAggregateId <= 0))
                {
                    var eAggregate = _mapper.Map<TimeSheetAggregate>(aggregate);
                    eAggregate.TimeSheetId = timeSheet.TimeSheetId;

                    await _organizationDBContext.TimeSheetAggregate.AddAsync(eAggregate);
                }

                foreach (var eAggregate in timeSheetDayOffs)
                {
                    var mAggregate = model.TimeSheetDayOffs.FirstOrDefault(x => x.TimeSheetDayOffId == eAggregate.TimeSheetDayOffId);
                    if (mAggregate != null)
                        _mapper.Map(mAggregate, eAggregate);
                    else
                        eAggregate.IsDeleted = true;
                }

                foreach (var dayOff in model.TimeSheetDayOffs.Where(x => x.TimeSheetDayOffId <= 0))
                {
                    var eDayOff = _mapper.Map<TimeSheetDayOff>(dayOff);
                    eDayOff.TimeSheetId = timeSheet.TimeSheetId;

                    await _organizationDBContext.TimeSheetDayOff.AddAsync(eDayOff);
                }

                foreach (var overtime in model.TimeSheetOvertimes.Where(x => x.TimeSheetOvertimeId <= 0))
                {
                    var eOvertime = _mapper.Map<TimeSheetOvertime>(overtime);
                    eOvertime.TimeSheetId = timeSheet.TimeSheetId;

                    await _organizationDBContext.TimeSheetOvertime.AddAsync(eOvertime);
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

        public async Task<bool> DeleteTimeSheet(int year, int month)
        {
            var trans = await _organizationDBContext.Database.BeginTransactionAsync();
            try
            {
                var timeSheet = await _organizationDBContext.TimeSheet.FirstOrDefaultAsync(x => x.Year == year && x.Month == month);
                if (timeSheet == null)
                    throw new BadRequestException(GeneralCode.ItemNotFound, $"Không tồn tại bảng chấm công tháng {month} năm {year}");

                var timeSheetDetails = await _organizationDBContext.TimeSheetDetail.Where(x => x.TimeSheetId == timeSheet.TimeSheetId).ToListAsync();
                var timeSheetAggregates = await _organizationDBContext.TimeSheetAggregate.Where(x => x.TimeSheetId == timeSheet.TimeSheetId).ToListAsync();
                var timeSheetDayOffs = await _organizationDBContext.TimeSheetDayOff.Where(x => x.TimeSheetId == timeSheet.TimeSheetId).ToListAsync();
                var timeSheetOvertimes = await _organizationDBContext.TimeSheetOvertime.Where(x => x.TimeSheetId == timeSheet.TimeSheetId).ToListAsync();

                timeSheetDayOffs.ForEach(x => x.IsDeleted = true);
                timeSheetAggregates.ForEach(x => x.IsDeleted = true);
                timeSheetDetails.ForEach(x => x.IsDeleted = true);
                timeSheetOvertimes.ForEach(x => x.IsDeleted = true);
                timeSheet.IsDeleted = true;

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

        public async Task<TimeSheetModel> GetTimeSheet(int year, int month)
        {
            var timeSheet = await _organizationDBContext.TimeSheet
            .FirstOrDefaultAsync(x => x.Year == year && x.Month == month);
            if (timeSheet == null)
                throw new BadRequestException(GeneralCode.ItemNotFound, $"Không tồn tại bảng chấm công tháng {month} năm {year}");

            var timeSheetDetails = await _organizationDBContext.TimeSheetDetail.Where(x => x.TimeSheetId == timeSheet.TimeSheetId).ProjectTo<TimeSheetDetailModel>(_mapper.ConfigurationProvider).ToListAsync();
            var timeSheetDayOffs = await _organizationDBContext.TimeSheetDayOff.Where(x => x.TimeSheetId == timeSheet.TimeSheetId).ProjectTo<TimeSheetDayOffModel>(_mapper.ConfigurationProvider).ToListAsync();
            var timeSheetAggregates = await _organizationDBContext.TimeSheetAggregate.Where(x => x.TimeSheetId == timeSheet.TimeSheetId).ProjectTo<TimeSheetAggregateModel>(_mapper.ConfigurationProvider).ToListAsync();
            var timeSheetOvertimes = await _organizationDBContext.TimeSheetOvertime.Where(x => x.TimeSheetId == timeSheet.TimeSheetId).ProjectTo<TimeSheetOvertimeModel>(_mapper.ConfigurationProvider).ToListAsync();

            var result = _mapper.Map<TimeSheetModel>(timeSheet);
            result.TimeSheetAggregates = timeSheetAggregates;
            result.TimeSheetDayOffs = timeSheetDayOffs;
            result.TimeSheetDetails = timeSheetDetails;
            result.TimeSheetOvertimes = timeSheetOvertimes;
            return result;
        }

        public async Task<TimeSheetModel> GetTimeSheetByEmployee(int year, int month, int employeeId)
        {
            var timeSheet = await _organizationDBContext.TimeSheet
            .FirstOrDefaultAsync(x => x.Year == year && x.Month == month);
            if (timeSheet == null)
                throw new BadRequestException(GeneralCode.ItemNotFound, $"Không tồn tại bảng chấm công tháng {month} năm {year}");

            var timeSheetDetails = await _organizationDBContext.TimeSheetDetail.Where(x => x.TimeSheetId == timeSheet.TimeSheetId && x.EmployeeId == employeeId).ProjectTo<TimeSheetDetailModel>(_mapper.ConfigurationProvider).ToListAsync();
            var timeSheetDayOffs = await _organizationDBContext.TimeSheetDayOff.Where(x => x.TimeSheetId == timeSheet.TimeSheetId && x.EmployeeId == employeeId).ProjectTo<TimeSheetDayOffModel>(_mapper.ConfigurationProvider).ToListAsync();
            var timeSheetAggregates = await _organizationDBContext.TimeSheetAggregate.Where(x => x.TimeSheetId == timeSheet.TimeSheetId && x.EmployeeId == employeeId).ProjectTo<TimeSheetAggregateModel>(_mapper.ConfigurationProvider).ToListAsync();
            var timeSheetOvertimes = await _organizationDBContext.TimeSheetOvertime.Where(x => x.TimeSheetId == timeSheet.TimeSheetId && x.EmployeeId == employeeId).ProjectTo<TimeSheetOvertimeModel>(_mapper.ConfigurationProvider).ToListAsync();

            var result = _mapper.Map<TimeSheetModel>(timeSheet);
            result.TimeSheetAggregates = timeSheetAggregates;
            result.TimeSheetDayOffs = timeSheetDayOffs;
            result.TimeSheetDetails = timeSheetDetails;
            result.TimeSheetOvertimes = timeSheetOvertimes;
            return result;
        }

        public async Task<IList<TimeSheetModel>> GetListTimeSheet()
        {
            var query = _organizationDBContext.TimeSheet.AsNoTracking().Include(x => x.TimeSheetDetail);

            return await query.Select(x => new TimeSheetModel
            {
                TimeSheetId = x.TimeSheetId,
                IsApprove = x.IsApprove,
                Month = x.Month,
                Year = x.Year,
                Note = x.Note,
                TimeSheetDetails = _mapper.Map<IList<TimeSheetDetailModel>>(x.TimeSheetDetail)
            }).ToListAsync();
        }


        public CategoryNameModel GetFieldDataForMapping(long beginDate, long endDate)
        {
            var result = new CategoryNameModel()
            {
                CategoryCode = "TimeSheet",
                CategoryTitle = "Chấm công",
                IsTreeView = false,
                Fields = new List<CategoryFieldNameModel>()
            };

            var fields = ExcelUtils.GetFieldNameModels<TimeSheetImportFieldModel>().ToList();

            var fieldsAbsenceTypeSymbols = (_organizationDBContext.AbsenceTypeSymbol.ToList()).Select(x => new CategoryFieldNameModel
            {
                FieldName = x.SymbolCode,
                FieldTitle = x.TypeSymbolDescription,
                GroupName = "Ngày nghỉ",
            });

            var fieldsOvertimeLevel = (_organizationDBContext.OvertimeLevel.ToList()).Select(x => new CategoryFieldNameModel
            {
                FieldName = $"Overtime_{x.OvertimeCode}",
                FieldTitle = $"Tổng thời gian(giờ) làm tăng ca {x.OvertimeCode}",
                GroupName = "Tăng ca(giờ)",
            });

            for (long unixTime = beginDate; unixTime <= endDate; unixTime += 86400)
            {
                var date = unixTime.UnixToDateTime().Value;
                fields.Add(new CategoryFieldNameModel
                {
                    FieldName = $"TimeKeepingDay{unixTime}",
                    FieldTitle = $"Thời gian chấm công ngày {date.ToString("dd/MM/yyyy")} (hh:mm)",
                    GroupName = "TT chấm công",
                });
            }

            fields.AddRange(fieldsAbsenceTypeSymbols);
            fields.AddRange(fieldsOvertimeLevel);

            fields.Add(new CategoryFieldNameModel
            {
                FieldName = ImportStaticFieldConsants.CheckImportRowEmpty,
                FieldTitle = "Cột kiểm tra",
            });

            result.Fields = fields;
            return result;
        }

        public async Task<bool> ImportTimeSheetFromMapping(int month, int year, long beginDate, long endDate, ImportExcelMapping mapping, Stream stream)
        {
            string timeKeepingDayPropPrefix = "TimeKeepingDay";
            string timeKeepingOvertimePropPrefix = "Overtime";
            Type typeInfo = typeof(TimeSheetImportFieldModel);

            var reader = new ExcelReader(stream);

            var employees = await _organizationDBContext.Employee.ToListAsync();
            var absenceTypeSymbols = await _organizationDBContext.AbsenceTypeSymbol.ToListAsync();
            var overtimeLevels = await _organizationDBContext.OvertimeLevel.ToListAsync();
            var absentSymbol = await _organizationDBContext.CountedSymbol.FirstOrDefaultAsync(x => x.CountedSymbolType == (int)EnumCountedSymbol.AbsentSymbol);

            var _importData = reader.ReadSheets(mapping.SheetName, mapping.FromRow, mapping.ToRow, null).First();

            var dataTimeSheetWithPrimaryKey = new List<RowDataImportTimeSheetModel>();
            int i = 0;
            foreach (var row in _importData.Rows)
            {
                var fieldCheckImportEmpty = mapping.MappingFields.FirstOrDefault(x => x.FieldName == ImportStaticFieldConsants.CheckImportRowEmpty);
                if (fieldCheckImportEmpty != null)
                {
                    string value = null;
                    if (row.ContainsKey(fieldCheckImportEmpty.Column))
                        value = row[fieldCheckImportEmpty.Column]?.ToString();

                    if (string.IsNullOrWhiteSpace(value)) continue;
                }

                var timeSheetImportModel = new TimeSheetImportFieldModel();
                foreach (var prop in typeInfo.GetProperties())
                {
                    var mappingField = mapping.MappingFields.FirstOrDefault(x => x.FieldName == prop.Name);
                    if (mappingField == null)
                        throw new BadRequestException(GeneralCode.ItemNotFound, $"Không tìm thấy field {prop.Name}");

                    string value = null;
                    if (row.ContainsKey(mappingField.Column))
                        value = row[mappingField.Column]?.ToString();

                    if (value != null && value.StartsWith(PREFIX_ERROR_CELL))
                    {
                        throw ValidatorResources.ExcelFormulaNotSupported.BadRequestFormat(i + mapping.FromRow, mappingField.Column, $"{value}");
                    }

                    prop.SetValue(timeSheetImportModel, value.ConvertValueByType(prop.PropertyType));
                }

                dataTimeSheetWithPrimaryKey.Add(new RowDataImportTimeSheetModel()
                {
                    EmployeeCode = timeSheetImportModel.EmployeeCode,
                    row = row,
                    timeSheetImportModel = timeSheetImportModel
                });
                i++;
            }




        

            var timeSheetDetails = new List<TimeSheetDetail>();
            var timeSheetAggregates = new List<TimeSheetAggregateModel>();
            var timeSheetDayOffs = new List<TimeSheetDayOffModel>();
            var timeSheetOvertimes = new List<TimeSheetOvertimeModel>();
            foreach (var (key, rows) in dataTimeSheetWithPrimaryKey.GroupBy(x => x.EmployeeCode).ToDictionary(k => k.Key, v => v.ToList()))
            {
                var employeeCode = key.NormalizeAsInternalName();

                var employee = employees.FirstOrDefault(e => e.EmployeeCode.NormalizeAsInternalName() == employeeCode || e.Email.NormalizeAsInternalName() == employeeCode);

                if (employee == null) throw new BadRequestException(GeneralCode.InvalidParams, $"Nhân viên {employeeCode} không tồn tại");

                

                var rowIn = rows.First();
                var rowOut = rows.Last();

                for (long unixTime = beginDate; unixTime <= endDate; unixTime += 86400)
                {
                    var timeKeepingDayProp = $"{timeKeepingDayPropPrefix}{unixTime}";

                    var mappingFieldTimeKeepingDay = mapping.MappingFields.FirstOrDefault(x => x.FieldName == timeKeepingDayProp);
                    if (mappingFieldTimeKeepingDay == null)
                        continue;

                    string timeInAsString = null;
                    string timeOutAsString = null;

                    if (rowIn.row.ContainsKey(mappingFieldTimeKeepingDay.Column))
                        timeInAsString = rowIn.row[mappingFieldTimeKeepingDay.Column]?.ToString();

                    if (rowOut.row.ContainsKey(mappingFieldTimeKeepingDay.Column))
                        timeInAsString = rowOut.row[mappingFieldTimeKeepingDay.Column]?.ToString();

                    // var timeInAsString = typeInfo.GetProperty(timeKeepingDayProp).GetValue(rowIn.timeSheetImportModel) as string;
                    // var timeOutAsString = typeInfo.GetProperty(timeKeepingDayProp).GetValue(rowOut.timeSheetImportModel) as string;

                    if (timeInAsString == absentSymbol.SymbolCode) continue;

                    int? absenceTypeSymbolId = null;

                    if (timeInAsString.Contains('-')) continue;

                    if (!timeInAsString.Contains(':'))
                    {
                        var absenceTypeSymbolCode = timeInAsString;
                        var absenceType = absenceTypeSymbols.FirstOrDefault(x => x.SymbolCode == absenceTypeSymbolCode);
                        if (absenceType == null)
                            throw new BadRequestException(GeneralCode.InvalidParams, $"Không có ký hiệu loại vắng {absenceTypeSymbolCode} trong hệ thống");
                        absenceTypeSymbolId = absenceType.AbsenceTypeSymbolId;
                    }

                    var date = unixTime.UnixToDateTime().Value;


                    TimeSheetDetail timeSheetDetail;

                    if (absenceTypeSymbolId.HasValue)
                    {
                        timeSheetDetail = new TimeSheetDetail()
                        {
                            AbsenceTypeSymbolId = absenceTypeSymbolId,
                            Date = date,
                            EmployeeId = employee.UserId
                        };
                    }
                    else
                    {
                        var anyTimeAsStringEmpty = string.IsNullOrWhiteSpace(timeInAsString) || string.IsNullOrWhiteSpace(timeOutAsString);
                        if (anyTimeAsStringEmpty)
                        {
                            timeSheetDetail = new TimeSheetDetail()
                            {
                                Date = date,
                                EmployeeId = employee.UserId
                            };
                        }
                        else
                        {
                            timeSheetDetail = new TimeSheetDetail()
                            {
                                Date = date,
                                TimeIn = TimeSpan.Parse(timeInAsString),
                                TimeOut = TimeSpan.Parse(timeOutAsString),
                                EmployeeId = employee.UserId
                            };
                        }
                    }

                    timeSheetDetails.Add(timeSheetDetail);
                }

                var timeSheetAggregate = new TimeSheetAggregateModel
                {
                    CountedAbsence = rowIn.timeSheetImportModel.CountedAbsence,
                    CountedEarly = rowIn.timeSheetImportModel.CountedEarly,
                    CountedLate = rowIn.timeSheetImportModel.CountedLate,
                    CountedWeekday = rowIn.timeSheetImportModel.CountedWeekday,
                    CountedWeekdayHour = rowIn.timeSheetImportModel.CountedWeekdayHour,
                    CountedWeekend = rowIn.timeSheetImportModel.CountedWeekend,
                    CountedWeekendHour = rowIn.timeSheetImportModel.CountedWeekendHour,
                    EmployeeId = employee.UserId,
                    MinsEarly = rowIn.timeSheetImportModel.MinsEarly,
                    MinsLate = rowIn.timeSheetImportModel.MinsLate,
                    // Overtime1 = rowIn.timeSheetImportModel.Overtime1,
                    // Overtime2 = rowIn.timeSheetImportModel.Overtime2,
                    // Overtime3 = rowIn.timeSheetImportModel.Overtime3,
                };

                var timeSheetDayOffForPerson = absenceTypeSymbols.Where(absence => mapping.MappingFields.Any(x => x.FieldName == absence.SymbolCode))
                .Select(absence =>
                {
                    var mappingField = mapping.MappingFields.FirstOrDefault(x => x.FieldName == absence.SymbolCode);
                    if (mappingField == null)
                        throw new BadRequestException(GeneralCode.ItemNotFound, $"Không tìm thấy trường dữ liệu {absence.SymbolCode}");

                    string countedDayOffAsString = null;
                    if (rowIn.row.ContainsKey(mappingField.Column))
                        countedDayOffAsString = rowIn.row[mappingField.Column]?.ToString();

                    var countedDayOff = int.Parse(string.IsNullOrWhiteSpace(countedDayOffAsString) ? "0" : countedDayOffAsString);

                    return new TimeSheetDayOffModel
                    {
                        AbsenceTypeSymbolId = absence.AbsenceTypeSymbolId,
                        EmployeeId = employee.UserId,
                        CountedDayOff = countedDayOff
                    };
                }).Where(x => x.CountedDayOff > 0).ToList();

                var timeSheetOvertimeForPerson = overtimeLevels.Where(overtimeLevel => mapping.MappingFields.Any(x => x.FieldName == $"{timeKeepingOvertimePropPrefix}_{overtimeLevel.OvertimeCode}"))
                .Select(overtimeLevel =>
                {
                    var timeKeepingOvertimeProp = $"{timeKeepingOvertimePropPrefix}_{overtimeLevel.OvertimeCode}";
                    var mappingField = mapping.MappingFields.FirstOrDefault(x => x.FieldName == timeKeepingOvertimeProp);
                    if (mappingField == null)
                        throw new BadRequestException(GeneralCode.ItemNotFound, $"Không tìm thấy trường dữ liệu {timeKeepingOvertimeProp}");

                    string minsOvertimeAsString = null;
                    if (rowIn.row.ContainsKey(mappingField.Column))
                        minsOvertimeAsString = rowIn.row[mappingField.Column]?.ToString();

                    var minsOvertime = decimal.Parse(string.IsNullOrWhiteSpace(minsOvertimeAsString) ? "0" : minsOvertimeAsString);

                    return new TimeSheetOvertimeModel
                    {
                        OvertimeLevelId = overtimeLevel.OvertimeLevelId,
                        EmployeeId = employee.UserId,
                        MinsOvertime = minsOvertime
                    };
                }).Where(x => x.MinsOvertime > 0).ToList();


                timeSheetAggregates.Add(timeSheetAggregate);
                timeSheetDayOffs.AddRange(timeSheetDayOffForPerson);
                timeSheetOvertimes.AddRange(timeSheetOvertimeForPerson);
            }

            var trans = await _organizationDBContext.Database.BeginTransactionAsync();
            try
            {
                var timeSheet = await _organizationDBContext.TimeSheet.FirstOrDefaultAsync(x => x.Month == month && x.Year == year);
                if (timeSheet == null)
                {
                    timeSheet = new TimeSheet()
                    {
                        Month = month,
                        Year = year,
                        IsApprove = false,
                    };
                    await _organizationDBContext.TimeSheet.AddAsync(timeSheet);
                    await _organizationDBContext.SaveChangesAsync();
                }

                var existsTimeSheetDetails = await _organizationDBContext.TimeSheetDetail.Where(x => x.TimeSheetId == timeSheet.TimeSheetId).ToArrayAsync();
                var existsTimeSheetAggregates = await _organizationDBContext.TimeSheetAggregate.Where(x => x.TimeSheetId == timeSheet.TimeSheetId).ToArrayAsync();
                var existsTimeSheetDayOffs = await _organizationDBContext.TimeSheetDayOff.Where(x => x.TimeSheetId == timeSheet.TimeSheetId).ToArrayAsync();
                var existsTimeSheetOvertimes = await _organizationDBContext.TimeSheetOvertime.Where(x => x.TimeSheetId == timeSheet.TimeSheetId).ToArrayAsync();

                foreach (var timeSheetDetail in timeSheetDetails)
                {
                    var oldTimeSheetDetail = existsTimeSheetDetails.FirstOrDefault(x => x.Date == timeSheetDetail.Date && x.EmployeeId == timeSheetDetail.EmployeeId);
                    var employee = employees.FirstOrDefault(x => x.UserId == timeSheetDetail.EmployeeId);
                    if (oldTimeSheetDetail != null && mapping.ImportDuplicateOptionId == EnumImportDuplicateOption.Denied)
                        throw new BadRequestException(GeneralCode.InvalidParams, $"Tồn tại chấm công ngày {timeSheetDetail.Date.ToString("dd/MM/yyyy")} của nhân viên có mã {employee.EmployeeCode}");

                    if (oldTimeSheetDetail == null)
                    {
                        timeSheetDetail.TimeSheetId = timeSheet.TimeSheetId;
                        await _organizationDBContext.TimeSheetDetail.AddAsync(timeSheetDetail);
                    }
                    else if (mapping.ImportDuplicateOptionId == EnumImportDuplicateOption.Update)
                    {
                        oldTimeSheetDetail.TimeIn = timeSheetDetail.TimeIn;
                        oldTimeSheetDetail.TimeOut = timeSheetDetail.TimeOut;
                        oldTimeSheetDetail.AbsenceTypeSymbolId = timeSheetDetail.AbsenceTypeSymbolId;
                        oldTimeSheetDetail.MinsEarly = timeSheetDetail.MinsEarly;
                        oldTimeSheetDetail.MinsLate = timeSheetDetail.MinsLate;
                        oldTimeSheetDetail.MinsOvertime = timeSheetDetail.MinsOvertime;
                    }

                    await _organizationDBContext.SaveChangesAsync();
                }

                foreach (var timeSheetAggregate in timeSheetAggregates)
                {
                    var oldTimeSheetAggregate = existsTimeSheetAggregates.FirstOrDefault(x => x.EmployeeId == timeSheetAggregate.EmployeeId);
                    var employee = employees.FirstOrDefault(x => x.UserId == timeSheetAggregate.EmployeeId);
                    if (oldTimeSheetAggregate != null && mapping.ImportDuplicateOptionId == EnumImportDuplicateOption.Denied)
                        throw new BadRequestException(GeneralCode.InvalidParams, $"Tồn tại chấm công của nhân viên có mã {employee.EmployeeCode}");

                    if (oldTimeSheetAggregate == null)
                    {
                        var entity = _mapper.Map<TimeSheetAggregate>(timeSheetAggregate);
                        entity.TimeSheetId = timeSheet.TimeSheetId;
                        await _organizationDBContext.TimeSheetAggregate.AddAsync(entity);
                    }
                    else if (mapping.ImportDuplicateOptionId == EnumImportDuplicateOption.Update)
                    {
                        timeSheetAggregate.TimeSheetAggregateId = oldTimeSheetAggregate.TimeSheetAggregateId;
                        timeSheetAggregate.TimeSheetId = oldTimeSheetAggregate.TimeSheetId;
                        _mapper.Map(timeSheetAggregate, oldTimeSheetAggregate);
                    }

                    await _organizationDBContext.SaveChangesAsync();
                }

                foreach (var timeSheetDayOff in timeSheetDayOffs)
                {
                    var oldTimeSheetDayOff = existsTimeSheetDayOffs.FirstOrDefault(x => x.EmployeeId == timeSheetDayOff.EmployeeId && x.AbsenceTypeSymbolId == timeSheetDayOff.AbsenceTypeSymbolId);
                    var employee = employees.FirstOrDefault(x => x.UserId == timeSheetDayOff.EmployeeId);
                    if (oldTimeSheetDayOff != null && mapping.ImportDuplicateOptionId == EnumImportDuplicateOption.Denied)
                        throw new BadRequestException(GeneralCode.InvalidParams, $"Tồn tại chấm công của nhân viên có mã {employee.EmployeeCode}");

                    if (oldTimeSheetDayOff == null)
                    {
                        var entity = _mapper.Map<TimeSheetDayOff>(timeSheetDayOff);
                        entity.TimeSheetId = timeSheet.TimeSheetId;
                        await _organizationDBContext.TimeSheetDayOff.AddAsync(entity);
                    }
                    else if (mapping.ImportDuplicateOptionId == EnumImportDuplicateOption.Update)
                    {
                        timeSheetDayOff.TimeSheetDayOffId = oldTimeSheetDayOff.TimeSheetDayOffId;
                        timeSheetDayOff.TimeSheetId = oldTimeSheetDayOff.TimeSheetId;
                        _mapper.Map(timeSheetDayOff, oldTimeSheetDayOff);
                    }

                    await _organizationDBContext.SaveChangesAsync();
                }

                foreach (var timeSheetDayOvertime in timeSheetOvertimes)
                {
                    var oldTimeSheetOvertime = existsTimeSheetOvertimes.FirstOrDefault(x => x.EmployeeId == timeSheetDayOvertime.EmployeeId && x.OvertimeLevelId == timeSheetDayOvertime.OvertimeLevelId);
                    var employee = employees.FirstOrDefault(x => x.UserId == timeSheetDayOvertime.EmployeeId);
                    if (oldTimeSheetOvertime != null && mapping.ImportDuplicateOptionId == EnumImportDuplicateOption.Denied)
                        throw new BadRequestException(GeneralCode.InvalidParams, $"Tồn tại chấm công của nhân viên có mã {employee.EmployeeCode}");

                    if (oldTimeSheetOvertime == null)
                    {
                        var entity = _mapper.Map<TimeSheetOvertime>(timeSheetDayOvertime);
                        entity.TimeSheetId = timeSheet.TimeSheetId;
                        await _organizationDBContext.TimeSheetOvertime.AddAsync(entity);
                    }
                    else if (mapping.ImportDuplicateOptionId == EnumImportDuplicateOption.Update)
                    {
                        timeSheetDayOvertime.TimeSheetOvertimeId = oldTimeSheetOvertime.TimeSheetOvertimeId;
                        timeSheetDayOvertime.TimeSheetId = oldTimeSheetOvertime.TimeSheetId;
                        _mapper.Map(timeSheetDayOvertime, oldTimeSheetOvertime);
                    }

                    await _organizationDBContext.SaveChangesAsync();
                }

                await _organizationDBContext.SaveChangesAsync();
                await trans.CommitAsync();
            }
            catch (Exception)
            {
                await trans.RollbackAsync();
                throw;
            }

            return true;
        }

        public class RowDataImportTimeSheetModel
        {
            public string EmployeeCode { get; set; }
            public NonCamelCaseDictionary<string> row { get; set; }
            public TimeSheetImportFieldModel timeSheetImportModel { get; set; }
        }

        public async Task<bool> ApproveTimeSheet(int year, int month)
        {
            var timeSheet = await _organizationDBContext.TimeSheet
            .FirstOrDefaultAsync(x => x.Year == year && x.Month == month);
            if (timeSheet == null)
                throw new BadRequestException(GeneralCode.ItemNotFound, $"Không tồn tại bảng chấm công tháng {month} năm {year}");

            timeSheet.IsApprove = true;
            await _organizationDBContext.SaveChangesAsync();
            return true;
        }
    }
}