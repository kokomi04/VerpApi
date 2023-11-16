using AutoMapper;
using AutoMapper.QueryableExtensions;
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
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.OrganizationDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Organization.Service.HrConfig;
using System.Text;
using Microsoft.Data.SqlClient;
using VErp.Services.Accountancy.Model.Input;

namespace VErp.Services.Organization.Service.TimeKeeping
{
    public interface IShiftScheduleService
    {
        Task<long> AddShiftSchedule(ShiftScheduleModel model);
        Task<bool> DeleteShiftSchedule(long shiftScheduleId);
        Task<PageData<ShiftScheduleModel>> GetListShiftSchedule(ShiftScheduleFilterModel filter, int page, int size);
        Task<ShiftScheduleModel> GetShiftSchedule(long shiftScheduleId);
        Task<bool> UpdateShiftSchedule(long shiftScheduleId, ShiftScheduleModel model);
        Task<IList<NonCamelCaseDictionary>> GetEmployeesByDepartments(List<int> departmentIds);
        Task<IList<NonCamelCaseDictionary>> GetNotAssignedEmployees(long? fromDate, long? toDate);
        Task<List<EmployeeScheduleModel>> GetListEmployeeViolations();
        Task<IList<EmployeeScheduleModel>> GetAssignedDateEmployees(long? fromDate, long? toDate);
        Task<CategoryNameModel> GetFieldDataForMapping();
        Task<List<ShiftScheduleDetailModel>> ImportShiftScheduleFromMapping(long shiftScheduleId, long fromDate, long toDate, ImportExcelMapping mapping, Stream stream);
    }

    public class ShiftScheduleService : IShiftScheduleService
    {
        private readonly OrganizationDBContext _organizationDBContext;
        private readonly IMapper _mapper;
        private readonly IHrDataService _hrDataService;
        private readonly ICurrentContextService _currentContextService;

        public ShiftScheduleService(
            OrganizationDBContext organizationDBContext,
            IMapper mapper,
            IHrDataService hrDataService,
            ICurrentContextService currentContextService)
        {
            _organizationDBContext = organizationDBContext;
            _mapper = mapper;
            _hrDataService = hrDataService;
            _currentContextService = currentContextService;
        }

        public async Task<long> AddShiftSchedule(ShiftScheduleModel model)
        {
            await using var trans = await _organizationDBContext.Database.BeginTransactionAsync();

            try
            {
                if (await _organizationDBContext.ShiftSchedule.AnyAsync(s => s.Title == model.Title))
                    throw new BadRequestException(GeneralCode.InvalidParams, "Tiêu đề đã tồn tại trong danh sách!");

                var entity = _mapper.Map<ShiftSchedule>(model);

                await _organizationDBContext.ShiftSchedule.AddAsync(entity);
                await _organizationDBContext.SaveChangesAsync();

                await trans.CommitAsync();

                return entity.ShiftScheduleId;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message, e);
            }
        }

        public async Task<bool> UpdateShiftSchedule(long shiftScheduleId, ShiftScheduleModel model)
        {
            await using var trans = await _organizationDBContext.Database.BeginTransactionAsync();

            try
            {
                var shiftSchedule = await _organizationDBContext.ShiftSchedule.FirstOrDefaultAsync(x => x.ShiftScheduleId == shiftScheduleId);
                if (shiftSchedule == null)
                    throw new BadRequestException(GeneralCode.ItemNotFound, "Không tìm thấy bảng phân ca");

                if (model.Title != shiftSchedule.Title && await _organizationDBContext.ShiftSchedule.AnyAsync(s => s.Title == model.Title))
                    throw new BadRequestException(GeneralCode.InvalidParams, "Tiêu đề đã tồn tại trong danh sách!");

                await RemoveShiftScheduleConfiguration(shiftScheduleId);
                await RemoveShiftScheduleDetail(shiftScheduleId);

                model.ShiftScheduleId = shiftScheduleId;

                _mapper.Map(model, shiftSchedule);

                await _organizationDBContext.SaveChangesAsync();

                await trans.CommitAsync();

                return true;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message, e);
            }
        }

        public async Task<bool> DeleteShiftSchedule(long shiftScheduleId)
        {
            await using var trans = await _organizationDBContext.Database.BeginTransactionAsync();

            try
            {
                var shiftSchedule = await _organizationDBContext.ShiftSchedule.FirstOrDefaultAsync(x => x.ShiftScheduleId == shiftScheduleId);
                if (shiftSchedule == null)
                    throw new BadRequestException(GeneralCode.ItemNotFound, "Không tìm thấy bảng phân ca");

                shiftSchedule.IsDeleted = true;

                await RemoveShiftScheduleConfiguration(shiftScheduleId);
                await RemoveShiftScheduleDetail(shiftScheduleId);

                await _organizationDBContext.SaveChangesAsync();

                await trans.CommitAsync();

                return true;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message, e);
            }
        }

        public async Task<ShiftScheduleModel> GetShiftSchedule(long shiftScheduleId)
        {
            var shiftSchedule = await _organizationDBContext.ShiftSchedule
                .Include(s => s.ShiftScheduleConfiguration)
                .Include(s => s.ShiftScheduleDetail)
                .FirstOrDefaultAsync(x => x.ShiftScheduleId == shiftScheduleId);
            if (shiftSchedule == null)
                throw new BadRequestException(GeneralCode.ItemNotFound, "Không tìm thấy bảng phân ca");

            return _mapper.Map<ShiftScheduleModel>(shiftSchedule);
        }

        public async Task<PageData<ShiftScheduleModel>> GetListShiftSchedule(ShiftScheduleFilterModel filter, int page, int size)
        {
            var query = _organizationDBContext.ShiftSchedule
                .Include(s => s.ShiftScheduleDetail)
                .AsQueryable();

            if (!String.IsNullOrWhiteSpace(filter.Keyword))
                query = query.Where(t => t.Title.Contains(filter.Keyword));

            if (filter.FromDate.HasValue)
                query = query.Where(t => t.FromDate > filter.FromDate.UnixToDateTime());

            if (filter.ToDate.HasValue)
                query = query.Where(t => t.ToDate < filter.ToDate.UnixToDateTime());

            if (filter.DepartmentIds != null && filter.DepartmentIds.Count > 0)
            {
                var employeeIds = (await GetEmployeesByDepartments(filter.DepartmentIds)).Select(e => e[EmployeeConstants.EMPLOYEE_ID]).ToList();

                query = query.Where(t => t.ShiftScheduleDetail.Any(e => employeeIds.Contains(e.EmployeeId)));
            }

            query = query.InternalFilter(filter.ColumnsFilters);

            query = query.InternalOrderBy(filter.OrderBy, filter.Asc);

            var total = query.Count();

            query = size > 0 && page > 0 ? query.Skip((page - 1) * size).Take(size) : query;

            var data = new List<ShiftScheduleModel>();

            await query.ForEachAsync(q => data.Add(_mapper.Map<ShiftScheduleModel>(q)));

            return (data, total);
        }
        public async Task<IList<NonCamelCaseDictionary>> GetEmployeesByDepartments(List<int> departmentIds)
        {
            var (query, fieldNames) = await _hrDataService.BuildHrQuery(OrganizationConstants.HR_EMPLOYEE_TYPE_CODE, false);

            var select = new StringBuilder();
            var join = new StringBuilder($"({query}) v");

            foreach (var f in fieldNames)
            {
                select.Append($"v.{f}");
                select.Append(",");
            }

            var queryData = $"SELECT * FROM (SELECT {select.ToString().TrimEnd().TrimEnd(',')} FROM {join}) v ";
            var dataTable = await _organizationDBContext.QueryDataTableRaw(queryData, new List<SqlParameter>());

            var lstData = dataTable.ConvertData(true);

            if (lstData.Count == 0)
                throw new BadRequestException(GeneralCode.ItemNotFound, $"Không tìm thấy nhân viên trong bộ phận");

            if (departmentIds != null && departmentIds.Count > 0)
                lstData = lstData.Where(x => departmentIds.Contains((int)(x[EmployeeConstants.DEPARTMENT]))).ToList();

            return lstData;
        }

        public async Task<IList<NonCamelCaseDictionary>> GetNotAssignedEmployees(long? fromDate, long? toDate)
        {
            var employees = await GetEmployeesByDepartments(new List<int>());

            var scheduleDetails = _organizationDBContext.ShiftScheduleDetail.AsNoTracking();

            if(fromDate.HasValue && toDate.HasValue)
            {
                scheduleDetails = scheduleDetails.Where(s => s.AssignedDate >= fromDate.UnixToDateTime() && s.AssignedDate <= toDate.UnixToDateTime());
            }

            var scheduledEmployeeIds = scheduleDetails.Select(d => (long)d.EmployeeId).Distinct().ToList();

            return employees.Where(e => !scheduledEmployeeIds.Contains((long)e[EmployeeConstants.EMPLOYEE_ID])).ToList();
        }

        public async Task<List<EmployeeScheduleModel>> GetListEmployeeViolations()
        {
            var query = _organizationDBContext.ShiftSchedule
                .Include(s => s.ShiftScheduleDetail)
                .AsNoTracking();
            var shiftSchedules = new List<ShiftScheduleModel>();
            await query.ForEachAsync(q => shiftSchedules.Add(_mapper.Map<ShiftScheduleModel>(q)));


            var shifts = await _organizationDBContext.ShiftConfiguration.ToDictionaryAsync(s => s.ShiftConfigurationId);
            var allViolations = new Dictionary<(long EmployeeId, long AssignedDate), EmployeeScheduleModel>();

            foreach (var schedule in shiftSchedules)
            {
                var assignedDateGroup = schedule.ShiftScheduleDetail
                    .GroupBy(d => new { d.AssignedDate, d.EmployeeId })
                    .ToDictionary(g => g.Key, g => g.ToList());

                foreach (var otherSchedule in shiftSchedules)
                {
                    if (otherSchedule.ShiftScheduleId == schedule.ShiftScheduleId) continue;

                    var otherAssignedDateGroup = otherSchedule.ShiftScheduleDetail
                        .GroupBy(d => new { d.AssignedDate, d.EmployeeId })
                        .ToDictionary(g => g.Key, g => g.ToList());

                    foreach (var key in assignedDateGroup.Keys)
                    {
                        if (!otherAssignedDateGroup.ContainsKey(key)) continue;

                        var currentDetails = assignedDateGroup[key];
                        var otherDetails = otherAssignedDateGroup[key];

                        var currentShifts = currentDetails.Select(d => shifts[d.ShiftConfigurationId]).ToList();
                        var otherShifts = otherDetails.Select(d => shifts[d.ShiftConfigurationId]).ToList();

                        foreach (var cs in currentShifts)
                        {
                            foreach (var os in otherShifts)
                            {
                                if (!(cs.EntryTime > os.ExitTime || cs.ExitTime < os.EntryTime))
                                {
                                    var violationKey = (key.EmployeeId, key.AssignedDate);
                                    if (!allViolations.ContainsKey(violationKey))
                                    {
                                        allViolations[violationKey] = new EmployeeScheduleModel
                                        {
                                            EmployeeId = key.EmployeeId,
                                            AssignedDate = key.AssignedDate,
                                            ShiftScheduleIds = new List<long> { schedule.ShiftScheduleId }
                                        };
                                    }
                                    if (!allViolations[violationKey].ShiftScheduleIds.Contains(otherSchedule.ShiftScheduleId))
                                    {
                                        allViolations[violationKey].ShiftScheduleIds.Add(otherSchedule.ShiftScheduleId);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return allViolations.Values.ToList();
        }

        public async Task<IList<EmployeeScheduleModel>> GetAssignedDateEmployees(long? fromDate, long? toDate)
        {
            var scheduleDetails = _organizationDBContext.ShiftScheduleDetail.AsNoTracking();

            if (fromDate.HasValue && toDate.HasValue)
            {
                scheduleDetails = scheduleDetails.Where(s => s.AssignedDate >= fromDate.UnixToDateTime() && s.AssignedDate <= toDate.UnixToDateTime());
            }

            var result = new List<EmployeeScheduleModel>();

            foreach (var detail in scheduleDetails)
            {
                result.Add( new EmployeeScheduleModel() {
                    EmployeeId = detail.EmployeeId, 
                    AssignedDate = detail.AssignedDate.GetUnix()
                    });
            }

            return result;
        }

        //private async Task<List<EmployeeViolationModel>> CheckForViolations(ShiftScheduleModel model)
        //{
        //    var shifts = await _organizationDBContext.ShiftConfiguration.ToDictionaryAsync(s => s.ShiftConfigurationId);

        //    var currentAssignedDateGroup = model.ShiftScheduleDetail
        //        .GroupBy(d => new { d.AssignedDate, d.EmployeeId })
        //        .ToDictionary(g => g.Key, g => g.ToList());

        //    var query = _organizationDBContext.ShiftSchedule.Where(s => s.ShiftScheduleId != model.ShiftScheduleId).Include(s => s.ShiftScheduleDetail).AsQueryable();
        //    var otherShiftSchedules = new List<ShiftScheduleModel>();
        //    await query.ForEachAsync(q => otherShiftSchedules.Add(_mapper.Map<ShiftScheduleModel>(q)));

        //    var otherAssignedDateGroup = otherShiftSchedules.SelectMany(s => s.ShiftScheduleDetail)
        //        .GroupBy(d => new { d.AssignedDate, d.EmployeeId })
        //        .ToDictionary(g => g.Key, g => g.ToList());

        //    var violations = new List<EmployeeViolationModel>();

        //    foreach (var otherSchedule in otherShiftSchedules)
        //    {
        //        var assignedDateGroup = otherSchedule.ShiftScheduleDetail.GroupBy(d => new { d.AssignedDate, d.EmployeeId })
        //            .ToDictionary(g => g.Key, g => g.ToList());

        //        foreach (var key in currentAssignedDateGroup.Keys)
        //        {
        //            if (!assignedDateGroup.ContainsKey(key)) continue;

        //            var currentDetails = currentAssignedDateGroup[key];
        //            var otherDetails = assignedDateGroup[key];

        //            var currentShifts = currentDetails.Select(d => shifts[d.ShiftConfigurationId]).ToList();
        //            var otherShifts = otherDetails.Select(d => shifts[d.ShiftConfigurationId]).ToList();

        //            foreach (var cs in currentShifts)
        //            {
        //                foreach (var os in otherShifts)
        //                {
        //                    if (!(cs.EntryTime > os.ExitTime || cs.ExitTime < os.EntryTime))
        //                    {
        //                        var violation = violations.FirstOrDefault(v => v.EmployeeId == key.EmployeeId && v.AssignedDate == key.AssignedDate);
        //                        if (violation == null)
        //                        {
        //                            violation = new EmployeeViolationModel
        //                            {
        //                                EmployeeId = key.EmployeeId,
        //                                AssignedDate = key.AssignedDate,
        //                                ShiftScheduleIds = new List<long> { model.ShiftScheduleId }
        //                            };
        //                            violations.Add(violation);
        //                        }
        //                        if (!violation.ShiftScheduleIds.Contains(otherSchedule.ShiftScheduleId))
        //                        {
        //                            violation.ShiftScheduleIds.Add(otherSchedule.ShiftScheduleId);
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    return violations;
        //}

        public async Task<CategoryNameModel> GetFieldDataForMapping()
        {
            var result = new CategoryNameModel()
            {
                CategoryCode = "ShiftSchedule",
                CategoryTitle = "Phân ca",
                IsTreeView = false,
                Fields = new List<CategoryFieldNameModel>()
            };

            var timeSheetFields = ExcelUtils.GetFieldNameModels<ShiftScheduleImportModel>(null, true);

            result.Fields = timeSheetFields.DistinctBy(f => f.FieldName).ToList();
            return result;
        }

        public async Task<List<ShiftScheduleDetailModel>> ImportShiftScheduleFromMapping(long shiftScheduleId, long fromDate, long toDate, ImportExcelMapping mapping, Stream stream)
        {
            if (shiftScheduleId != 0)
            {
                var shiftSchedule = await _organizationDBContext.ShiftSchedule.FirstOrDefaultAsync(s => s.ShiftScheduleId == shiftScheduleId);
                if (shiftSchedule == null)
                {
                    throw new BadRequestException(GeneralCode.InvalidParams, $"Không tìm thấy bảng phân ca có Id \"{shiftScheduleId}\"");
                }
            }

            var hrEmployeeTypeId = await _organizationDBContext.HrType.Where(t => t.HrTypeCode == OrganizationConstants.HR_EMPLOYEE_TYPE_CODE).Select(t => t.HrTypeId).FirstOrDefaultAsync();
            var employees = await _hrDataService.SearchHrV2(hrEmployeeTypeId, false, new HrTypeBillsFilterModel(), 0, 0);

            var shifts = await _organizationDBContext.ShiftConfiguration.ToListAsync();

            var reader = new ExcelReader(stream);

            var lstData = reader.ReadSheetEntity<ShiftScheduleImportModel>(mapping, (entity, propertyName, value) =>
            {
                if (propertyName == nameof(ShiftScheduleImportModel.EmployeeCode))
                {
                    if (string.IsNullOrWhiteSpace(value))
                        throw new BadRequestException(GeneralCode.InvalidParams, "Mã nhân viên không được để trống");

                    if (!employees.List.Any(e => e[EmployeeConstants.EMPLOYEE_CODE].ToString() == value))
                        throw new BadRequestException(GeneralCode.ItemNotFound, $"Mã nhân viên {value} không tồn tại!");
                }

                if (propertyName == nameof(ShiftScheduleImportModel.AssignedDate))
                {
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        throw new BadRequestException(GeneralCode.InvalidParams, "Ngày phân ca không được để trống");
                    }
                    if (DateTime.TryParse(value, out DateTime date))
                    {
                        var dateUnix = date.Date.GetUnixUtc(_currentContextService.TimeZoneOffset);
                        if (dateUnix < fromDate || dateUnix > toDate)
                        {
                            throw new BadRequestException(GeneralCode.InvalidParams, "Ngày phân ca phải nằm trong khoảng ngày của bảng phân ca hiện tại");
                        }

                        entity.SetPropertyValue(propertyName, dateUnix);
                    }
                    else
                    {
                        throw new BadRequestException(GeneralCode.InvalidParams, $"Ngày phân ca sai định dạng");
                    }
                    return true;
                }

                if (propertyName == nameof(ShiftScheduleImportModel.ShiftCodes))
                {
                    if (string.IsNullOrWhiteSpace(value))
                        throw new BadRequestException(GeneralCode.InvalidParams, "Mã ca không được để trống");

                    var lstShiftCode = value.Split(",").Where(item => !string.IsNullOrWhiteSpace(item)).Select(item => item.Trim().ToLower()).ToArray();

                    foreach (var shiftCode in lstShiftCode)
                    {
                        if (!shifts.Any(s => s.ShiftCode.ToLower() == shiftCode))
                            throw new BadRequestException(GeneralCode.ItemNotFound, $"Mã ca {shiftCode} không tồn tại!");
                    }

                }

                return false;
            });

            if (!lstData.Any())
                throw new BadRequestException(GeneralCode.ItemNotFound, "Không tìm thấy bản ghi nào!");

            var result = new List<ShiftScheduleDetailModel>();

            foreach (var item in lstData)
            {
                var employee = employees.List.FirstOrDefault(e => e[EmployeeConstants.EMPLOYEE_CODE].ToString() == item.EmployeeCode);

                var lstShiftCode = item.ShiftCodes.Split(",").Select(item => item.Trim().ToLower()).ToArray();

                var shiftIds = shifts.Where(s => lstShiftCode.Contains(s.ShiftCode.ToLower())).Select(s => s.ShiftConfigurationId).ToList();

                foreach (var shiftId in shiftIds)
                {
                    var detail = new ShiftScheduleDetailModel
                    {
                        ShiftScheduleId = shiftScheduleId,
                        EmployeeId = (long)employee[EmployeeConstants.EMPLOYEE_ID],
                        AssignedDate = item.AssignedDate,
                        ShiftConfigurationId = shiftId,
                        HasOvertimePlan = true
                    };
                    result.Add(detail);
                }
            }

            return result;
        }


        private async Task RemoveShiftScheduleConfiguration(long shiftScheduleId)
        {
            var entity = _organizationDBContext.ShiftScheduleConfiguration
                    .Where(m => m.ShiftScheduleId == shiftScheduleId).AsNoTracking();
            _organizationDBContext.ShiftScheduleConfiguration.RemoveRange(entity);
            await _organizationDBContext.SaveChangesAsync();
        }
        private async Task RemoveShiftScheduleDetail(long shiftScheduleId)
        {
            var entity = _organizationDBContext.ShiftScheduleDetail
                    .Where(m => m.ShiftScheduleId == shiftScheduleId).AsNoTracking();
            _organizationDBContext.ShiftScheduleDetail.RemoveRange(entity);
            await _organizationDBContext.SaveChangesAsync();
        }
    }
}