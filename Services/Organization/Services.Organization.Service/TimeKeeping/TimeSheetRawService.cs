using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Services.Organization.Model.TimeKeeping;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
using VErp.Services.Accountancy.Model.Input;
using VErp.Services.Master.Service.Users;
using VErp.Services.Organization.Service.HrConfig;
using VErp.Services.Organization.Service.HrConfig.Facade;
using VErp.Services.Organization.Service.Salary;
using static VErp.Services.Organization.Service.HrConfig.HrDataService;

namespace VErp.Services.Organization.Service.TimeKeeping
{
    public interface ITimeSheetRawService
    {
        Task<long> AddTimeSheetRaw(TimeSheetRawModel model);
        Task<bool> DeleteTimeSheetRaw(long timeSheetRawId);
        Task<PageData<TimeSheetRawViewModel>> GetListTimeSheetRaw(TimeSheetRawFilterModel filter, int page, int size);
        Task<TimeSheetRawModel> GetTimeSheetRaw(long timeSheetRawId);
        Task<bool> UpdateTimeSheetRaw(long timeSheetRawId, TimeSheetRawModel model);
        Task<CategoryNameModel> GetFieldDataForMapping();
        Task<(Stream stream, string fileName, string contentType)> Export([FromBody] TimeSheetRawExportModel req);
        Task<bool> ImportTimeSheetRawFromMapping(ImportExcelMapping mapping, Stream stream);
    }

    public class TimeSheetRawService : ITimeSheetRawService
    {
        private const string F_Id = "F_Id";
        private const string so_ct = "so_ct";

        private readonly OrganizationDBContext _organizationDBContext;
        private readonly IMapper _mapper;
        private readonly IHrDataService _hrDataService;
        private readonly IHrDataImportDIService _hrDataImportDIService;
        private readonly ICurrentContextService _currentContextService;
        private readonly IUserService _userService;

        public TimeSheetRawService(
            OrganizationDBContext organizationDBContext,
            IMapper mapper, IHrDataService hrDataService,
            IHrDataImportDIService hrDataImportDIService,
            ICurrentContextService currentContextService,
            IUserService userService)
        {
            _organizationDBContext = organizationDBContext;
            _mapper = mapper;
            _hrDataService = hrDataService;
            _hrDataImportDIService = hrDataImportDIService;
            _currentContextService = currentContextService;
            _userService = userService;
        }

        public async Task<long> AddTimeSheetRaw(TimeSheetRawModel model)
        {
            if (model.TimeKeepingRecorder == null)
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

                if (matchingTimeSheetRaws != null)
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

            data = data.OrderByDescending(x => x.Date).ThenByDescending(x => x.Time).ToList();

            return (data, result.Count);
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

            var timeSheetFields = ExcelUtils.GetFieldNameModels<TimeSheetRawModel>().Where(f => f.GroupName == TimeSheetRawModel.GroupName);

            var hrType = await _organizationDBContext.HrType.Where(t => t.HrTypeCode == OrganizationConstants.HR_EMPLOYEE_TYPE_CODE).FirstOrDefaultAsync();

            var fields = await _hrDataService.GetHrFields(hrType.HrTypeId, null, true);
            fields = fields.Where(f => f.FormTypeId != EnumFormType.ImportFile && f.FormTypeId != EnumFormType.MultiSelect && f.IsMultiRow == false).ToList();
            var importFacade = new HrDataImportFacade(hrType, fields, _hrDataImportDIService);
            var employeeFields = (await importFacade.GetFieldDataForMapping()).Fields;

            result.Fields = timeSheetFields.Concat(employeeFields).DistinctBy(f => f.FieldName).ToList();
            return result;
        }

        public async Task<(Stream stream, string fileName, string contentType)> Export([FromBody] TimeSheetRawExportModel req)
        {
            var hrEmployeeTypeId = await _organizationDBContext.HrType.Where(t => t.HrTypeCode == OrganizationConstants.HR_EMPLOYEE_TYPE_CODE).Select(t => t.HrTypeId).FirstOrDefaultAsync();

            var fields = await _hrDataService.GetHrFields(hrEmployeeTypeId, null, true);
            fields = fields.Where(f => req.FieldNames == null || req.FieldNames.Contains(f.FieldName))
                .Where(f => f.FormTypeId != EnumFormType.ImportFile && f.FormTypeId != EnumFormType.MultiSelect && f.IsMultiRow == false)
                .ToList();

            Type type = typeof(TimeSheetRawModel);

            foreach (var prop in type.GetProperties())
            {
                var nameAttribute = prop.GetCustomAttribute<DisplayAttribute>();
                var dataTypeAttribute = prop.GetCustomAttribute<AllowedDataTypeAttribute>();

                if (nameAttribute != null && dataTypeAttribute != null)
                {
                    fields.Add(new HrValidateField()
                    {
                        FieldName = prop.Name,
                        Title = nameAttribute.Name,
                        DataTypeId = dataTypeAttribute.DataType,
                        IsMultiRow = true
                    });
                }
            }

            fields = fields.Where(f => req.FieldNames.Contains(f.FieldName)).ToList();

            var timeSheetRaws = (await GetListTimeSheetRaw(req, 0, 0)).List;

            var flatDatas = new List<NonCamelCaseDictionary>();

            foreach (var item in timeSheetRaws)
            {
                var f = new NonCamelCaseDictionary();

                foreach (var field in req.FieldNames)
                {
                    foreach(var matchingKey in item.Employee.Keys.Where(k => k.Contains(field)).ToList())
                    {
                        if(!f.Keys.Any(k => k.Equals(matchingKey)))
                            f.Add(matchingKey, item.Employee[matchingKey]);
                    }

                    var property = item.GetType().GetProperty(char.ToUpper(field[0]) + field.Substring(1));
                    if (property != null)
                    {
                        object value = property.GetValue(item);
                        f.Add(field, value);
                    }
                }

                flatDatas.Add(f);
            }

            var exportFacade = new TimeSheetRawExportFacade(fields, _currentContextService);

            return exportFacade.Export(flatDatas);
        }

        public async Task<bool> ImportTimeSheetRawFromMapping(ImportExcelMapping mapping, Stream stream)
        {
            var reader = new ExcelReader(stream);

            var lstData = reader.ReadSheetEntity<TimeSheetRawImportFieldModel>(mapping, (entity, propertyName, value) =>
            {
                if(propertyName == nameof(TimeSheetRawImportFieldModel.so_ct) && string.IsNullOrWhiteSpace(value))
                    throw new BadRequestException(GeneralCode.InvalidParams, "Mã nhân viên không được để trống");

                if (propertyName == nameof(TimeSheetRawModel.Date))
                {
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        throw new BadRequestException(GeneralCode.InvalidParams, "Ngày chấm công không được để trống");
                    }
                    if (DateTime.TryParse(value, out DateTime date))
                        {
                            entity.SetPropertyValue(propertyName, date.Date.GetUnix());
                        }
                        else
                        {
                            throw new BadRequestException(GeneralCode.InvalidParams, $"Ngày chấm công sai định dạng");
                        }
                    return true;
                }

                if (propertyName == nameof(TimeSheetRawModel.Time))
                {
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        throw new BadRequestException(GeneralCode.InvalidParams, "Giờ chấm công không được để trống");
                    }
                    if (!DateTime.TryParse(value, out DateTime date))
                        {
                            throw new BadRequestException(GeneralCode.InvalidParams, $"Giờ chấm công sai định dạng HH:mm");
                        }    

                        double time = date.Hour * 60 * 60 + date.Minute * 60;

                        entity.SetPropertyValue(propertyName, time);

                    return true;
                }

                return false;
            });

            var hrEmployeeTypeId = await _organizationDBContext.HrType.Where(t => t.HrTypeCode == OrganizationConstants.HR_EMPLOYEE_TYPE_CODE).Select(t => t.HrTypeId).FirstOrDefaultAsync();

            var employees = await _hrDataService.SearchHrV2(hrEmployeeTypeId, false, new HrTypeBillsFilterModel(), 0, 0);

            if (!lstData.Any())
                throw new BadRequestException(GeneralCode.ItemNotFound, "Không tìm thấy bản ghi nào!");

            foreach (var item in lstData)
            {
                var employee = employees.List.FirstOrDefault(e => e[so_ct].ToString() == item.so_ct);

                if(employee == null)
                    throw new BadRequestException(GeneralCode.ItemNotFound, $"Mã nhân viên {item.so_ct} không tồn tại!");
                
                if (await _organizationDBContext.TimeSheetRaw.AnyAsync(t => t.EmployeeId == (long)employee[F_Id] && t.Date == item.Date.UnixToDateTime().Value && t.Time == TimeSpan.FromSeconds(item.Time)))
                {
                    if (mapping.ImportDuplicateOptionId == EnumImportDuplicateOption.Denied)
                    {
                        throw new BadRequestException(GeneralCode.InvalidParams, $"Đã tồn tại mã nhân viên {item.so_ct} với ngày chấm công {item.Date.UnixToDateTime().Value.ToShortDateString()} và giờ chấm công {TimeSpan.FromSeconds(item.Time).ToString()}");
                    }
                    else if(mapping.ImportDuplicateOptionId == EnumImportDuplicateOption.Update)
                    {
                        continue;
                    }
                } 

                var ent = new TimeSheetRaw
                {
                    EmployeeId = (long)employee[F_Id],
                    Date = item.Date.UnixToDateTime().Value,
                    Time = TimeSpan.FromSeconds(item.Time),
                    TimeKeepingMethod = (int)TimeKeepingMethodType.Software,
                    TimeKeepingRecorder = (await _userService.GetInfo(_currentContextService.UserId)).EmployeeCode
                };
                await _organizationDBContext.TimeSheetRaw.AddAsync(ent);
            }
            await _organizationDBContext.SaveChangesAsync();

            return true;
        }
    }
}