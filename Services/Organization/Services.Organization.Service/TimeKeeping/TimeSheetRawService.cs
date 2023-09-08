using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Services.Organization.Model.TimeKeeping;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VErp.Commons.Constants;
using VErp.Commons.Enums.Organization.TimeKeeping;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.OrganizationDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Accountancy.Model.Input;
using VErp.Services.Organization.Service.HrConfig;
using VErp.Services.Organization.Service.Salary;

namespace VErp.Services.Organization.Service.TimeKeeping
{
    public interface ITimeSheetRawService
    {
        Task<long> AddTimeSheetRaw(TimeSheetRawModel model);
        Task<bool> DeleteTimeSheetRaw(long timeSheetRawId);
        Task<PageData<TimeSheetRawViewModel>> GetListTimeSheetRaw(TimeSheetRawFilterModel filter, int page, int size);
        Task<TimeSheetRawModel> GetTimeSheetRaw(long timeSheetRawId);
        Task<bool> UpdateTimeSheetRaw(long timeSheetRawId, TimeSheetRawModel model);


        CategoryNameModel GetFieldDataForMapping();

        Task<bool> ImportTimeSheetRawFromMapping(ImportExcelMapping mapping, Stream stream);
    }

    public class TimeSheetRawService : ITimeSheetRawService
    {
        private readonly OrganizationDBContext _organizationDBContext;
        private readonly IMapper _mapper;
        private readonly IHrDataService _hrDataService;

        public TimeSheetRawService(OrganizationDBContext organizationDBContext, IMapper mapper, IHrDataService hrDataService)
        {
            _organizationDBContext = organizationDBContext;
            _mapper = mapper;
            _hrDataService = hrDataService;
        }

        public async Task<long> AddTimeSheetRaw(TimeSheetRawModel model)
        {
            if(model.TimeKeepingRecorder == null)
            {
                model.TimeKeepingMethod = TimeKeepingMethodType.Machine;
                model.TimeKeepingRecorder = await _organizationDBContext.HrBill.Where(b => b.FId == model.EmployeeId)
                                                .Select(b => b.BillCode)
                                                .FirstOrDefaultAsync();
                if (model.Date == 0)
                    model.Date = DateTime.Now.GetUnix();

                if (model.Time == 0)
                    model.Time = DateTime.Now.TimeOfDay.TotalSeconds;
            }    
            var entity = _mapper.Map<TimeSheetRaw>(model);

            await _organizationDBContext.TimeSheetRaw.AddAsync(entity);
            await _organizationDBContext.SaveChangesAsync();

            return entity.TimeSheetRawId;
        }

        public async Task<bool> UpdateTimeSheetRaw(long timeSheetRawId, TimeSheetRawModel model)
        {
            var timeSheetRaw = await _organizationDBContext.TimeSheetRaw.FirstOrDefaultAsync(x => x.TimeSheetRawId == timeSheetRawId);
            if (timeSheetRaw == null)
                throw new BadRequestException(GeneralCode.ItemNotFound);

            model.TimeSheetRawId = timeSheetRawId;
            _mapper.Map(model, timeSheetRaw);

            await _organizationDBContext.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteTimeSheetRaw(long timeSheetRawId)
        {
            var timeSheetRaw = await _organizationDBContext.TimeSheetRaw.FirstOrDefaultAsync(x => x.TimeSheetRawId == timeSheetRawId);
            if (timeSheetRaw == null)
                throw new BadRequestException(GeneralCode.ItemNotFound);

            timeSheetRaw.IsDeleted = true;
            await _organizationDBContext.SaveChangesAsync();

            return true;
        }

        public async Task<TimeSheetRawModel> GetTimeSheetRaw(long timeSheetRawId)
        {
            var timeSheetRaw = await _organizationDBContext.TimeSheetRaw.FirstOrDefaultAsync(x => x.TimeSheetRawId == timeSheetRawId);
            if (timeSheetRaw == null)
                throw new BadRequestException(GeneralCode.ItemNotFound);

            return _mapper.Map<TimeSheetRawModel>(timeSheetRaw);
        }

        public async Task<PageData<TimeSheetRawViewModel>> GetListTimeSheetRaw(TimeSheetRawFilterModel filter, int page, int size)
        {
            var hrEmployeeTypeId = await _organizationDBContext.HrType.Where(t => t.HrTypeCode == OrganizationConstants.HR_EMPLOYEE_TYPE_CODE).Select(t => t.HrTypeId).FirstOrDefaultAsync();
            
            var employees = await _hrDataService.SearchHrV2(hrEmployeeTypeId, false, filter.HrTypeFilters, 0, 0);

            var employeeIds = employees.List.Select(e => (long)e["F_Id"]).ToList();

            var query = _organizationDBContext.TimeSheetRaw.Where(t => employeeIds.Contains(t.EmployeeId)).AsNoTracking();

            if (!String.IsNullOrWhiteSpace(filter.Keyword))
                query = query.Where(t => t.TimeKeepingRecorder.Contains(filter.Keyword));

            if (filter.FromDate.HasValue)
                query = query.Where(t => t.Date > filter.FromDate.UnixToDateTime());

            if (filter.ToDate.HasValue)
                query = query.Where(t => t.Date < filter.ToDate.UnixToDateTime());

            //if(filter.ColumnsFilters.fi)

            query = query.InternalFilter(filter.ColumnsFilters);

            query = query.InternalOrderBy(filter.OrderBy, filter.Asc);

            var timeSheetRaws = await query.ToListAsync();

            var result = new List<TimeSheetRawViewModel>();

            Parallel.ForEach(employees.List, e =>
            {
                var matchingTimeSheetRaws = timeSheetRaws.Where(t => t.EmployeeId == (long)e["F_Id"]);

                if(matchingTimeSheetRaws != null)
                {
                    foreach (var timeSheetRaw in matchingTimeSheetRaws)
                    {
                        result.Add(new TimeSheetRawViewModel()
                        {
                            EmployeeId = timeSheetRaw.EmployeeId,
                            TimeSheetRawId = timeSheetRaw.TimeSheetRawId,
                            Date = timeSheetRaw.Date.GetUnix(),
                            Time = timeSheetRaw.Time.TotalSeconds,
                            TimeKeepingMethod = (TimeKeepingMethodType)timeSheetRaw.TimeKeepingMethod,
                            TimeKeepingRecorder = timeSheetRaw.TimeKeepingRecorder,
                            Employee = e
                        });
                    }
                }
            });

            var data = size > 0 && page > 0 ? result.Skip((page - 1) * size).Take(size).ToList() : result;

            return (data, result.Count);
        }

        public CategoryNameModel GetFieldDataForMapping()
        {
            var result = new CategoryNameModel()
            {
                CategoryCode = "TimeSheet",
                CategoryTitle = "Chấm công",
                IsTreeView = false,
                Fields = new List<CategoryFieldNameModel>()
            };

            var fields = ExcelUtils.GetFieldNameModels<TimeSheetRawImportFieldModel>();
            result.Fields = fields;
            return result;
        }

        public async Task<bool> ImportTimeSheetRawFromMapping(ImportExcelMapping mapping, Stream stream)
        {
            var reader = new ExcelReader(stream);

            var employees = await _organizationDBContext.Employee.ToListAsync();

            var lstData = reader.ReadSheetEntity<TimeSheetRawImportFieldModel>(mapping, (entity, propertyName, value) =>
            {

                if (propertyName == nameof(TimeSheetRawImportFieldModel.EmployeeId))
                {
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        var val = value?.NormalizeAsInternalName();

                        var employee = employees
                        .Where(e => e.EmployeeCode.NormalizeAsInternalName() == val
                        || e.Email.NormalizeAsInternalName() == val)
                        .FirstOrDefault();

                        if (employee == null) throw new BadRequestException(GeneralCode.InvalidParams, $"Nhân viên {val} không tồn tại");

                        entity.SetPropertyValue(propertyName, employee.UserId);
                    }
                    return true;
                }

                if (propertyName == nameof(TimeSheetRawImportFieldModel.Time))
                {
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        var pattern = @"(?<hour>\d+):(?<min>\d+)";
                        Regex rx = new Regex(pattern);
                        MatchCollection match = rx.Matches(value);
                        if (match.Count != 1) throw new BadRequestException(GeneralCode.InvalidParams, $"Giờ chấm công sai định dạng hh:mm");

                        if (!int.TryParse(match[0].Groups["hour"].Value, out int hour) || !int.TryParse(match[0].Groups["min"].Value, out int min))
                            throw new BadRequestException(GeneralCode.InvalidParams, $"Giờ chấm công sai định dạng hh:mm");

                        if (hour >= 12 || hour < 0 || min >= 60 || min < 0) throw new BadRequestException(GeneralCode.InvalidParams, $"Giờ chấm công sai định dạng hh:mm");

                        TimeSpan time = TimeSpan.FromSeconds(hour * 60 * 60 + min * 60);

                        entity.SetPropertyValue(propertyName, time);
                    }
                    return true;
                }

                return false;
            });

            foreach (var item in lstData)
            {
                var ent = new TimeSheetRaw
                {
                    EmployeeId = item.EmployeeId,
                    Date = item.Date,
                    Time = item.Time

                };
                _organizationDBContext.TimeSheetRaw.Add(ent);
            }
            _organizationDBContext.SaveChanges();

            return true;
        }
    }
}