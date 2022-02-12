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
        Task<bool> DeleteTimeSheet(long timeSheetId);
        Task<IList<TimeSheetModel>> GetListTimeSheet();
        Task<TimeSheetModel> GetTimeSheet(long timeSheetId);
        Task<bool> UpdateTimeSheet(long timeSheetId, TimeSheetModel model);


        Task<CategoryNameModel> GetFieldDataForMapping();

        Task<bool> ImportTimeSheetFromMapping(int month, int year, ImportExcelMapping mapping, Stream stream);
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

            return entity.TimeSheetId;
        }

        public async Task<bool> UpdateTimeSheet(long timeSheetId, TimeSheetModel model)
        {
            var timeSheet = await _organizationDBContext.TimeSheet.FirstOrDefaultAsync(x => x.TimeSheetId == timeSheetId);
            if (timeSheet == null)
                throw new BadRequestException(GeneralCode.ItemNotFound);
            var timeSheetDetails = await _organizationDBContext.TimeSheetDetail.Where(x => x.TimeSheetId == timeSheetId).ToListAsync();

            model.TimeSheetId = timeSheetId;
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

            foreach (var detail in model.TimeSheetDetails.Where(x => x.TimeSheetId <= 0))
            {
                if (detail.TimeOut < detail.TimeIn)
                    throw new BadRequestException(GeneralCode.InvalidParams, "Thời gian vào phải nhỏ hơn thời gian ra");
                var eDetail = _mapper.Map<TimeSheetDetail>(detail);
                eDetail.TimeSheetId = timeSheetId;

                await _organizationDBContext.TimeSheetDetail.AddAsync(eDetail);
            }


            await _organizationDBContext.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteTimeSheet(long timeSheetId)
        {
            var timeSheet = await _organizationDBContext.TimeSheet.FirstOrDefaultAsync(x => x.TimeSheetId == timeSheetId);
            if (timeSheet == null)
                throw new BadRequestException(GeneralCode.ItemNotFound);
            var timeSheetDetails = await _organizationDBContext.TimeSheetDetail.Where(x => x.TimeSheetId == timeSheetId).ToListAsync();

            timeSheet.IsDeleted = true;
            timeSheetDetails.ForEach(x => x.IsDeleted = true);
            await _organizationDBContext.SaveChangesAsync();

            return true;
        }

        public async Task<TimeSheetModel> GetTimeSheet(long timeSheetId)
        {
            var timeSheet = await _organizationDBContext.TimeSheet.FirstOrDefaultAsync(x => x.TimeSheetId == timeSheetId);
            if (timeSheet == null)
                throw new BadRequestException(GeneralCode.ItemNotFound);

            return _mapper.Map<TimeSheetModel>(timeSheet);
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


        public async Task<CategoryNameModel> GetFieldDataForMapping()
        {
            var result = new CategoryNameModel()
            {
                CategoryCode = "TimeSheet",
                CategoryTitle = "Chấm công",
                IsTreeView = false,
                Fields = new List<CategoryFieldNameModel>()
            };

            var fields = Utils.GetFieldNameModels<TimeSheetImportFieldModel>();
            result.Fields = fields;
            return result;
        }

        public async Task<bool> ImportTimeSheetFromMapping(int month, int year, ImportExcelMapping mapping, Stream stream)
        {
            string timeKeepingDayPropPrefix = nameof(TimeSheetImportFieldModel.TimeKeepingDay1)[..^1];
            Type typeInfo = typeof(TimeSheetImportFieldModel);

            var reader = new ExcelReader(stream);

            var employees = await _organizationDBContext.Employee.ToListAsync();
            var absenceTypeSymbols = await _organizationDBContext.AbsenceTypeSymbol.ToListAsync();
            var absentSymbol = await _organizationDBContext.CountedSymbol.FirstOrDefaultAsync(x => x.CountedSymbolType == (int)EnumCountedSymbol.AbsentSymbol);

            var data = (reader.ReadSheetEntity<TimeSheetImportFieldModel>(mapping, (entity, propertyName, value) =>
            {
                return false;
            })).GroupBy(x => x.EmployeeCode).ToDictionary(k => k.Key, v => v.ToList());

            var timeSheetDetails = new List<TimeSheetDetail>();
            foreach (var (key, rows) in data)
            {
                var employeeCode = key.NormalizeAsInternalName();

                var employee = employees.Where(e => e.EmployeeCode.NormalizeAsInternalName() == employeeCode || e.Email.NormalizeAsInternalName() == employeeCode)
                                        .FirstOrDefault();

                if (employee == null) throw new BadRequestException(GeneralCode.InvalidParams, $"Nhân viên {employeeCode} không tồn tại");

                int maxDayNumberInMoth = 31;

                var rowIn = rows.First();
                var rowOut = rows.Last();

                for (int day = 1; day <= maxDayNumberInMoth; day++)
                {
                    var timeKeepingDayProp = $"{timeKeepingDayPropPrefix}{day}";

                    var timeInAsString = typeInfo.GetProperty(timeKeepingDayProp).GetValue(rowIn) as string;
                    var timeOutAsString = typeInfo.GetProperty(timeKeepingDayProp).GetValue(rowOut) as string;

                    if (timeInAsString == absentSymbol.SymbolCode) continue;

                    int? absenceTypeSymbolId = null;
                    if (!timeInAsString.Contains(':'))
                    {
                        var absenceTypeSymbolCode = timeInAsString;
                        var absenceType = absenceTypeSymbols.FirstOrDefault(x => x.SymbolCode == absenceTypeSymbolCode);
                        if (absenceType == null)
                            throw new BadRequestException(GeneralCode.InvalidParams, $"Không có ký hiệu loại vắng {absenceTypeSymbolCode} trong hệ thống");
                        absenceTypeSymbolId = absenceType.AbsenceTypeSymbolId;
                    }

                    var minsEarly = rowIn.MinsEarly;
                    var minsLate = rowIn.MinsLate;
                    var minsOvertime = rowIn.MinsOvertime;
                    var date = new DateTime(day, month, year);

                    var timeSheetDetail = absenceTypeSymbolId.HasValue ? new TimeSheetDetail(){
                        AbsenceTypeSymbolId = absenceTypeSymbolId,
                        Date = date,
                        MinsEarly = minsEarly,
                        MinsLate = minsLate,
                        MinsOvertime = minsOvertime,
                        EmployeeId = employee.UserId
                    } : new TimeSheetDetail()
                    {
                        Date = date,
                        MinsEarly = minsEarly,
                        MinsLate = minsLate,
                        MinsOvertime = minsOvertime,
                        TimeIn = TimeSpan.Parse(timeInAsString),
                        TimeOut = TimeSpan.Parse(timeOutAsString),
                        EmployeeId = employee.UserId
                    };

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


                foreach (var timeSheetDetail in timeSheetDetails)
                {
                    var oldTimeSheetDetail = existsTimeSheetDetails.FirstOrDefault(x => x.Date == timeSheetDetail.Date && x.EmployeeId == x.EmployeeId);
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

    }
}