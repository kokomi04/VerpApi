using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Services.Organization.Model.TimeKeeping;
using VErp.Commons.Constants;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.Organization.TimeKeeping;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.EF.OrganizationDB;

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

        Task<bool> ImportTimeSheetFromMapping(int year, int month, long beginDate, long endDate, ImportExcelMapping mapping, Stream stream);

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

                await trans.CommitAsync();

                return entity.TimeSheetId;
            }
            catch (System.Exception ex)
            {
                await trans.RollbackAsync();
                throw ex;
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


                await _organizationDBContext.SaveChangesAsync();

                await trans.CommitAsync();

                return true;

            }
            catch (System.Exception ex)
            {
                await trans.RollbackAsync();
                throw ex;
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

                timeSheetDayOffs.ForEach(x => x.IsDeleted = true);
                timeSheetAggregates.ForEach(x => x.IsDeleted = true);
                timeSheetDetails.ForEach(x => x.IsDeleted = true);
                timeSheet.IsDeleted = true;

                await _organizationDBContext.SaveChangesAsync();

                await trans.CommitAsync();
                
                return true;

            }
            catch (System.Exception ex)
            {
                await trans.RollbackAsync();
                throw ex;
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

            var result = _mapper.Map<TimeSheetModel>(timeSheet);
            result.TimeSheetAggregates = timeSheetAggregates;
            result.TimeSheetDayOffs = timeSheetDayOffs;
            result.TimeSheetDetails = timeSheetDetails;
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

            var result = _mapper.Map<TimeSheetModel>(timeSheet);
            result.TimeSheetAggregates = timeSheetAggregates;
            result.TimeSheetDayOffs = timeSheetDayOffs;
            result.TimeSheetDetails = timeSheetDetails;
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

            var fields = Utils.GetFieldNameModels<TimeSheetImportFieldModel>().ToList();

            var fieldsAbsenceTypeSymbols = (_organizationDBContext.AbsenceTypeSymbol.ToList()).Select(x => new CategoryFieldNameModel
            {
                FieldName = x.SymbolCode,
                FieldTitle = x.TypeSymbolDescription,
                GroupName = "Ngày nghỉ",
            }
            );

            for (long unixTime = beginDate; unixTime <= endDate; unixTime +=86400)
            {
                var date = unixTime.UnixToDateTime().Value;
                fields.Add(new CategoryFieldNameModel{
                    FieldName = $"TimeKeepingDay{unixTime}",
                    FieldTitle = $"Thời gian chấm công ngày {date.ToString("dd/MM/yyyy")}",
                    GroupName = "TT chấm công",
                });
            }

            fields.AddRange(fieldsAbsenceTypeSymbols);

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
            Type typeInfo = typeof(TimeSheetImportFieldModel);

            var reader = new ExcelReader(stream);

            var employees = await _organizationDBContext.Employee.ToListAsync();
            var absenceTypeSymbols = await _organizationDBContext.AbsenceTypeSymbol.ToListAsync();
            var absentSymbol = await _organizationDBContext.CountedSymbol.FirstOrDefaultAsync(x => x.CountedSymbolType == (int)EnumCountedSymbol.AbsentSymbol);

            var _importData = reader.ReadSheets(mapping.SheetName, mapping.FromRow, mapping.ToRow, null).FirstOrDefault();

            var dataTimeSheetWithPrimaryKey = new List<RowDataImportTimeSheetModel>();
            foreach (var row in _importData.Rows)
            {
                var fieldCheckImportEmpty = mapping.MappingFields.FirstOrDefault(x => x.FieldName == ImportStaticFieldConsants.CheckImportRowEmpty);
                if (fieldCheckImportEmpty != null)
                {
                    string value = null;
                    if (row.ContainsKey(fieldCheckImportEmpty.Column))
                        value = row[fieldCheckImportEmpty.Column]?.ToString();

                    if(string.IsNullOrWhiteSpace(value)) continue;
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

                    prop.SetValue(timeSheetImportModel, value.ConvertValueByType(prop.PropertyType));
                }

                dataTimeSheetWithPrimaryKey.Add(new RowDataImportTimeSheetModel()
                {
                    EmployeeCode = timeSheetImportModel.EmployeeCode,
                    row = row,
                    timeSheetImportModel = timeSheetImportModel
                });
            }


            
            
            // var data = (reader.ReadSheetEntity<TimeSheetImportFieldModel>(mapping, (entity, propertyName, value) =>
            // {
            //     return false;
            // })).GroupBy(x => x.EmployeeCode).ToDictionary(k => k.Key, v => v.ToList());

            var timeSheetDetails = new List<TimeSheetDetail>();
            var timeSheetAggregates = new List<TimeSheetAggregateModel>();
            var timeSheetDayOffs = new List<TimeSheetDayOffModel>();
            foreach (var (key, rows) in dataTimeSheetWithPrimaryKey.GroupBy(x => x.EmployeeCode).ToDictionary(k => k.Key, v => v.ToList()))
            {
                var employeeCode = key.NormalizeAsInternalName();

                var employee = employees.Where(e => e.EmployeeCode.NormalizeAsInternalName() == employeeCode || e.Email.NormalizeAsInternalName() == employeeCode)
                                        .FirstOrDefault();

                if (employee == null) throw new BadRequestException(GeneralCode.InvalidParams, $"Nhân viên {employeeCode} không tồn tại");

                // int maxDayNumberInMoth = 31;

                var rowIn = rows.First();
                var rowOut = rows.Last();

                for (long unixTime = beginDate; unixTime <= endDate; unixTime += 86400)
                {
                    var timeKeepingDayProp = $"{timeKeepingDayPropPrefix}{unixTime}";

                    var timeInAsString = typeInfo.GetProperty(timeKeepingDayProp).GetValue(rowIn.timeSheetImportModel) as string;
                    var timeOutAsString = typeInfo.GetProperty(timeKeepingDayProp).GetValue(rowOut.timeSheetImportModel) as string;

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

                    var timeSheetDetail = absenceTypeSymbolId.HasValue ? new TimeSheetDetail()
                    {
                        AbsenceTypeSymbolId = absenceTypeSymbolId,
                        Date = date,
                        EmployeeId = employee.UserId
                    } : string.IsNullOrWhiteSpace(timeInAsString) || string.IsNullOrWhiteSpace(timeOutAsString) ? new TimeSheetDetail()
                    {
                        Date = date,
                        EmployeeId = employee.UserId
                    } : new TimeSheetDetail()
                    {
                        Date = date,
                        TimeIn = TimeSpan.Parse(timeInAsString),
                        TimeOut = TimeSpan.Parse(timeOutAsString),
                        EmployeeId = employee.UserId
                    };

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
                        Overtime1 = rowIn.timeSheetImportModel.Overtime1,
                        Overtime2 = rowIn.timeSheetImportModel.Overtime2,
                        Overtime3 = rowIn.timeSheetImportModel.Overtime3,
                    };

                    var timeSheetDayOffForPerson = absenceTypeSymbols.Select(absence =>
                    {
                        var mappingField = mapping.MappingFields.FirstOrDefault(x => x.FieldName == absence.SymbolCode);
                        if (mappingField == null)
                            throw new BadRequestException(GeneralCode.ItemNotFound, $"Không tìm thấy field {absence.SymbolCode}");

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


                    timeSheetAggregates.Add(timeSheetAggregate);
                    timeSheetDayOffs.AddRange(timeSheetDayOffForPerson);
                    timeSheetDetails.Add(timeSheetDetail);
                }
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

                await _organizationDBContext.SaveChangesAsync();
                await trans.CommitAsync();
            }
            catch (System.Exception ex)
            {
                await trans.RollbackAsync();
                throw ex;
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