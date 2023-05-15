using AutoMapper;
using AutoMapper.QueryableExtensions;
using DocumentFormat.OpenXml.EMMA;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Ocsp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verp.Resources.Organization;
using Verp.Resources.Organization.Salary;
using Verp.Resources.Organization.Salary.Validation;
using VErp.Commons.Constants;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Commons.Library;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.OrganizationDB;
using VErp.Infrastructure.ServiceCore.Abstract;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Organization.Model.Salary;
using static NPOI.HSSF.UserModel.HeaderFooter;

namespace VErp.Services.Organization.Service.Salary.Implement
{
    public class SalaryPeriodAdditionBillService : BillDateValidateionServiceAbstract, ISalaryPeriodAdditionBillService
    {

        private readonly OrganizationDBContext _organizationDBContext;
        private readonly ICurrentContextService _currentContextService;
        private readonly IMapper _mapper;
        private readonly ObjectActivityLogFacade _billActivityLog;
        private readonly ICustomGenCodeHelperService _customGenCodeHelperService;
        private readonly ICategoryHelperService _categoryHelperService;
        private readonly ISalaryPeriodAdditionTypeService _salaryPeriodAdditionTypeService;
        public SalaryPeriodAdditionBillService(OrganizationDBContext organizationDBContext, ICurrentContextService currentContextService, IMapper mapper, IActivityLogService activityLogService, ICustomGenCodeHelperService customGenCodeHelperService, ICategoryHelperService categoryHelperService, ISalaryPeriodAdditionTypeService salaryPeriodAdditionTypeService)
            : base(organizationDBContext)
        {
            _organizationDBContext = organizationDBContext;
            _currentContextService = currentContextService;
            _mapper = mapper;
            _billActivityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.SalaryPeriodAdditionBill);
            _customGenCodeHelperService = customGenCodeHelperService;
            _categoryHelperService = categoryHelperService;
            _salaryPeriodAdditionTypeService = salaryPeriodAdditionTypeService;
        }



        public async Task<SalaryPeriodAdditionBillInfo> GetInfo(int salaryPeriodAdditionTypeId, long salaryPeriodAdditionBillId)
        {
            var entity = await QueryFullInfo()
                .Where(b => b.SalaryPeriodAdditionBillId == salaryPeriodAdditionBillId)
                .FirstOrDefaultAsync();

            if (entity == null)
            {
                throw GeneralCode.ItemNotFound.BadRequest();
            }

            return MapInfo(entity);
        }

        public IQueryable<SalaryPeriodAdditionBill> QueryFullInfo()
        {
            return _organizationDBContext.SalaryPeriodAdditionBill
                 .Include(b => b.SalaryPeriodAdditionBillEmployee)
                 .ThenInclude(e => e.SalaryPeriodAdditionBillEmployeeValue)
                 .ThenInclude(v => v.SalaryPeriodAdditionField);
        }


        public SalaryPeriodAdditionBillInfo MapInfo(SalaryPeriodAdditionBill fullInfo)
        {

            var info = _mapper.Map<SalaryPeriodAdditionBillInfo>(fullInfo);


            info.Details = new List<SalaryPeriodAdditionBillEmployeeModel>();

            foreach (var detail in fullInfo.SalaryPeriodAdditionBillEmployee)
            {
                var detailValues = detail.SalaryPeriodAdditionBillEmployeeValue.ToList();
                var detailInfo = _mapper.Map<SalaryPeriodAdditionBillEmployeeModel>(detail);
                detailInfo.Values = new NonCamelCaseDictionary<decimal?>();
                foreach (var value in detailValues)
                {
                    detailInfo.Values.Add(value.SalaryPeriodAdditionField.FieldName, value.Value);
                }

                info.Details.Add(detailInfo);
            }
            return info;
        }

        public async Task<PageData<SalaryPeriodAdditionBillList>> GetList(int salaryPeriodAdditionTypeId, int? year, int? month, string keyword, int page, int size)
        {
            var query = GetListQuery(salaryPeriodAdditionTypeId, year, month, keyword);

            var total = await query.CountAsync();
            var lst = await query.ProjectTo<SalaryPeriodAdditionBillList>(_mapper.ConfigurationProvider).Skip(page - 1).Take(size).ToListAsync();
            return (lst, total);
        }

        public IQueryable<SalaryPeriodAdditionBill> GetListQuery(int salaryPeriodAdditionTypeId, int? year, int? month, string keyword)
        {
            var query = _organizationDBContext.SalaryPeriodAdditionBill.Where(b => b.SalaryPeriodAdditionTypeId == salaryPeriodAdditionTypeId);
            if (year > 0)
            {
                query = query.Where(b => b.Year == year.Value);
            }

            if (month > 0)
            {
                query = query.Where(b => b.Month == month.Value);
            }
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(b => b.Content.Contains(keyword));
            }

            return query;
        }

        public async Task<long> Create(int salaryPeriodAdditionTypeId, SalaryPeriodAdditionBillModel model)
        {
            var typeInfo = await _salaryPeriodAdditionTypeService.GetFullEntityInfo(salaryPeriodAdditionTypeId);

            if (typeInfo == null)
            {
                throw GeneralCode.ItemNotFound.BadRequest();
            }

            if (!typeInfo.IsActived)
            {
                throw SalaryPeriodAdditionTypeValidationMessage.TypeInActived.BadRequestFormat(typeInfo.Title);
            }

            var ctx = _customGenCodeHelperService.CreateGenerateCodeContext();

            var date = new DateTime(model.Year.Value, model.Month.Value, 1);

            var code = await ctx
                .SetConfig(EnumObjectType.SalaryPeriodAdditionBill, EnumObjectType.SalaryPeriodAdditionType, salaryPeriodAdditionTypeId, typeInfo.Title)
                .SetConfigData(0, date.GetUnixUtc(_currentContextService.TimeZoneOffset))
                .TryValidateAndGenerateCode(_organizationDBContext.SalaryPeriodAdditionBill, model.BillCode, (s, code) => s.SalaryPeriodAdditionTypeId == salaryPeriodAdditionTypeId && s.BillCode == code);

            model.BillCode = code;

            var emplyees = await GetEmployees(model);

            using var trans = await _organizationDBContext.Database.BeginTransactionAsync();

            var info = await CreateToDb(typeInfo, model, emplyees);

            await trans.CommitAsync();

            await _billActivityLog.LogBuilder(() => SalaryPeriodAdditionBillActivityLogMessage.Create)
            .MessageResourceFormatDatas(info.BillCode, typeInfo.Title)
            .ObjectId(info.SalaryPeriodAdditionBillId)
            .JsonData(model.JsonSerialize())
            .CreateLog();

            await ctx.ConfirmCode();

            return info.SalaryPeriodAdditionBillId;
        }


        public async Task<SalaryPeriodAdditionBill> CreateToDb(SalaryPeriodAdditionType typFullInfo, SalaryPeriodAdditionBillModel model, List<NonCamelCaseDictionary> emplyees)
        {
            await ValidateModel(typFullInfo.SalaryPeriodAdditionTypeId, model, null, emplyees);


            var info = _mapper.Map<SalaryPeriodAdditionBill>(model);
            info.SalaryPeriodAdditionTypeId = typFullInfo.SalaryPeriodAdditionTypeId;
            await _organizationDBContext.SalaryPeriodAdditionBill.AddAsync(info);
            await _organizationDBContext.SaveChangesAsync();

            var dicDetails = new Dictionary<SalaryPeriodAdditionBillEmployee, SalaryPeriodAdditionBillEmployeeModel>();
            var lstDetails = new List<SalaryPeriodAdditionBillEmployee>();
            foreach (var detail in model.Details)
            {
                var entity = _mapper.Map<SalaryPeriodAdditionBillEmployee>(detail);
                entity.SalaryPeriodAdditionBillId = info.SalaryPeriodAdditionBillId;
                lstDetails.Add(entity);
                dicDetails.Add(entity, detail);
            }
            await _organizationDBContext.InsertByBatch(lstDetails, true, true);
            await _organizationDBContext.SaveChangesAsync();


            var fields = typFullInfo.SalaryPeriodAdditionTypeField.ToDictionary(f => f.SalaryPeriodAdditionField.FieldName, f => f.SalaryPeriodAdditionField);

            var fieldValues = new List<SalaryPeriodAdditionBillEmployeeValue>();
            foreach (var (detailEntity, detailModel) in dicDetails)
            {
                foreach (var (fileName, fieldValue) in detailModel.Values)
                {
                    if (fields.TryGetValue(fileName, out var fieldInfo))
                    {
                        fieldValues.Add(new SalaryPeriodAdditionBillEmployeeValue()
                        {
                            SalaryPeriodAdditionBillEmployeeId = detailEntity.SalaryPeriodAdditionBillEmployeeId,
                            SalaryPeriodAdditionFieldId = fieldInfo.SalaryPeriodAdditionFieldId,
                            Value = fieldValue
                        });
                    }
                }
            }
            await _organizationDBContext.InsertByBatch(fieldValues, false, false);

            await _organizationDBContext.SaveChangesAsync();
            return info;
        }


        public async Task<bool> Update(int salaryPeriodAdditionTypeId, long salaryPeriodAdditionBillId, SalaryPeriodAdditionBillModel model)
        {
            var typeInfo = await _salaryPeriodAdditionTypeService.GetFullEntityInfo(salaryPeriodAdditionTypeId);

            if (typeInfo == null)
            {
                throw GeneralCode.ItemNotFound.BadRequest();
            }

            if (!typeInfo.IsActived)
            {
                throw SalaryPeriodAdditionTypeValidationMessage.TypeInActived.BadRequestFormat(typeInfo.Title);
            }

            var emplyees = await GetEmployees(model);

            using var trans = await _organizationDBContext.Database.BeginTransactionAsync();

            var r = await UpdateToDb(typeInfo, salaryPeriodAdditionBillId, model, emplyees);

            await trans.CommitAsync();

            await _billActivityLog.LogBuilder(() => SalaryPeriodAdditionBillActivityLogMessage.Update)
            .MessageResourceFormatDatas(model.BillCode, typeInfo.Title)
            .ObjectId(salaryPeriodAdditionBillId)
            .JsonData(model.JsonSerialize())
            .CreateLog();


            return r;
        }

        public async Task<bool> UpdateToDb(SalaryPeriodAdditionType typFullInfo, long salaryPeriodAdditionBillId, SalaryPeriodAdditionBillModel model, List<NonCamelCaseDictionary> emplyees)
        {

            var info = await QueryFullInfo().FirstOrDefaultAsync(b => b.SalaryPeriodAdditionTypeId == typFullInfo.SalaryPeriodAdditionTypeId && b.SalaryPeriodAdditionBillId == salaryPeriodAdditionBillId);
            if (info == null)
            {
                throw GeneralCode.ItemNotFound.BadRequest();
            }

            await ValidateModel(typFullInfo.SalaryPeriodAdditionTypeId, model, info, emplyees);


            _mapper.Map(model, info);
            info.SalaryPeriodAdditionBillId = salaryPeriodAdditionBillId;
            info.SalaryPeriodAdditionTypeId = typFullInfo.SalaryPeriodAdditionTypeId;

            _organizationDBContext.SalaryPeriodAdditionBillEmployeeValue.RemoveRange(info.SalaryPeriodAdditionBillEmployee.SelectMany(e => e.SalaryPeriodAdditionBillEmployeeValue));
            _organizationDBContext.SalaryPeriodAdditionBillEmployee.RemoveRange(info.SalaryPeriodAdditionBillEmployee);
            await _organizationDBContext.SaveChangesAsync();

            var dicDetailEntityModel = new Dictionary<SalaryPeriodAdditionBillEmployee, SalaryPeriodAdditionBillEmployeeModel>();
            var lstDetailEntity = new List<SalaryPeriodAdditionBillEmployee>();
            foreach (var detailModel in model.Details)
            {
                var detailEntity = _mapper.Map<SalaryPeriodAdditionBillEmployee>(detailModel);
                detailEntity.SalaryPeriodAdditionBillId = info.SalaryPeriodAdditionBillId;
                detailEntity.SalaryPeriodAdditionBillEmployeeId = 0;
                lstDetailEntity.Add(detailEntity);
                dicDetailEntityModel.Add(detailEntity, detailModel);
            }
            await _organizationDBContext.InsertByBatch(lstDetailEntity, true, true);
            await _organizationDBContext.SaveChangesAsync();


            var fields = typFullInfo.SalaryPeriodAdditionTypeField.ToDictionary(f => f.SalaryPeriodAdditionField.FieldName, f => f.SalaryPeriodAdditionField);

            var fieldValues = new List<SalaryPeriodAdditionBillEmployeeValue>();
            foreach (var (detailEntity, detailModel) in dicDetailEntityModel)
            {
                foreach (var (fileName, fieldValue) in detailModel.Values)
                {
                    if (fields.TryGetValue(fileName, out var fieldInfo))
                    {
                        fieldValues.Add(new SalaryPeriodAdditionBillEmployeeValue()
                        {
                            SalaryPeriodAdditionBillEmployeeId = detailEntity.SalaryPeriodAdditionBillEmployeeId,
                            SalaryPeriodAdditionFieldId = fieldInfo.SalaryPeriodAdditionFieldId,
                            Value = fieldValue
                        });
                    }
                }
            }
            await _organizationDBContext.InsertByBatch(fieldValues, false, false);

            await _organizationDBContext.SaveChangesAsync();

            return true;
        }

        public async Task<bool> Delete(int salaryPeriodAdditionTypeId, long salaryPeriodAdditionBillId)
        {
            var typeInfo = await _organizationDBContext.SalaryPeriodAdditionType.FirstOrDefaultAsync();

            if (typeInfo == null)
            {
                throw GeneralCode.ItemNotFound.BadRequest();
            }

            if (!typeInfo.IsActived)
            {
                throw SalaryPeriodAdditionTypeValidationMessage.TypeInActived.BadRequestFormat(typeInfo.Title);
            }

            using var trans = await _organizationDBContext.Database.BeginTransactionAsync();

            var info = await _organizationDBContext.SalaryPeriodAdditionBill.FirstOrDefaultAsync(b => b.SalaryPeriodAdditionTypeId == salaryPeriodAdditionTypeId && b.SalaryPeriodAdditionBillId == salaryPeriodAdditionBillId);
            if (info == null)
            {
                throw GeneralCode.ItemNotFound.BadRequest();
            }

            await ValidateModel(salaryPeriodAdditionTypeId, null, info, null);

            info.IsDeleted = true;

            await _organizationDBContext.SaveChangesAsync();

            await trans.CommitAsync();

            await _billActivityLog.LogBuilder(() => SalaryPeriodAdditionBillActivityLogMessage.Delete)
            .MessageResourceFormatDatas(info.BillCode, typeInfo.Title)
            .ObjectId(info.SalaryPeriodAdditionBillId)
            .JsonData(info.JsonSerialize())
            .CreateLog();

            return true;
        }

        private async Task ValidateModel(int salaryPeriodAdditionTypeId, SalaryPeriodAdditionBillModel model, SalaryPeriodAdditionBill info, List<NonCamelCaseDictionary> emplyees)
        {

            DateTime? modelPeriodDate = null;
            DateTime? infoPeriodDate = null;
            if (model != null)
            {
                var emplyeesById = emplyees
                  .GroupBy(e =>
                  {
                      if (e.TryGetValue(CategoryFieldConstants.F_Id, out var id))
                      {
                          return Convert.ToInt64(id);
                      }
                      return 0;
                  })
                  .ToDictionary(e => e.Key, e => e.First());
                if (emplyeesById == null)
                {
                    emplyeesById = new Dictionary<long, NonCamelCaseDictionary>();
                }

                if (model.Details?.Count <= 0)
                {
                    throw SalaryPeriodAdditionTypeValidationMessage.RequireAtLeastOneDetail.BadRequestFormat(model.BillCode);
                }

                var currentSalaryPeriodAdditionBillId = (info?.SalaryPeriodAdditionBillId) ?? 0;
                var existedBillByCode = await _organizationDBContext.SalaryPeriodAdditionBill.Include(b => b.SalaryPeriodAdditionType)
                    .FirstOrDefaultAsync(b => b.BillCode == model.BillCode && b.SalaryPeriodAdditionBillId != currentSalaryPeriodAdditionBillId);

                if (existedBillByCode != null)
                {
                    if (existedBillByCode.SalaryPeriodAdditionTypeId != salaryPeriodAdditionTypeId)
                        throw SalaryPeriodAdditionTypeValidationMessage.BillCodeAlreadyExistedInAnOtherType.BadRequestFormat(model.BillCode, existedBillByCode.SalaryPeriodAdditionType.Title);
                    else
                        throw SalaryPeriodAdditionTypeValidationMessage.BillCodeAlreadyExisted.BadRequestFormat(model.BillCode);
                }

                if (string.IsNullOrWhiteSpace(model.BillCode))
                {
                    throw SalaryPeriodAdditionTypeValidationMessage.BillCodeIsRequired.BadRequest();
                }

                modelPeriodDate = new DateTime(model.Year ?? 0, model.Month ?? 0, 1).AddMinutes(_currentContextService.TimeZoneOffset ?? -420);

                var doesNotExistEmployee = model.Details.FirstOrDefault(d => !emplyeesById.ContainsKey(d.EmployeeId));
                if (doesNotExistEmployee != null)
                {
                    throw SalaryPeriodAdditionTypeValidationMessage.EmployeeDoseNotExisted.BadRequestFormat(model.Details.IndexOf(doesNotExistEmployee) + 1);
                }

                var duplicateEmployee = model.Details.GroupBy(d => d.EmployeeId).FirstOrDefault(g => g.Count() > 1);
                if (duplicateEmployee != null)
                {
                    var rowNumbers = duplicateEmployee.Select(e => model.Details.IndexOf(e) + 1).ToArray();
                    var employeeInfo = emplyeesById[duplicateEmployee.First().EmployeeId];
                    var strEmployee = "";
                    foreach (var (key, value) in employeeInfo)
                    {
                        strEmployee += " " + value;
                    }

                    throw SalaryPeriodAdditionTypeValidationMessage.EmployeeExistingMultitime.BadRequestFormat(strEmployee, string.Join(",", rowNumbers));

                }
            }
            if (info != null)
            {
                infoPeriodDate = new DateTime(info.Year, info.Month, 1).AddMinutes(_currentContextService.TimeZoneOffset ?? -420);
            }



            await ValidateDateOfBill(modelPeriodDate, infoPeriodDate);

            await ValidateDateOfBill(model?.Date.UnixToDateTime(), info?.Date);
        }

        private async Task<List<NonCamelCaseDictionary>> GetEmployees(SalaryPeriodAdditionBillModel model)
        {
            var employeeIds = model.Details.Select(d => d.EmployeeId).Distinct().ToList();

            var referTableNames = new List<string>() { OrganizationConstants.EMPLOYEE_CATEGORY_CODE };

            var referFields = await _categoryHelperService.GetReferFields(referTableNames, null);
            var refCategoryFields = referFields.GroupBy(f => f.CategoryCode).ToDictionary(f => f.Key, f => f.ToList());

            if (!refCategoryFields.TryGetValue(OrganizationConstants.EMPLOYEE_CATEGORY_CODE, out var refCategory))
            {
                throw HrDataValidationMessage.RefTableNotFound.BadRequestFormat(OrganizationConstants.EMPLOYEE_CATEGORY_CODE);
            }

            var selecFields = refCategory.Where(f => f.CategoryFieldName != CategoryFieldConstants.F_Id && !f.IsHidden)
                .OrderBy(f => f.SortOrder)
                .Take(2)
                .ToList();

            var selecFieldsString = string.Join(",", selecFields.Select(f => f.CategoryFieldName).ToArray());
            if (!string.IsNullOrWhiteSpace(selecFieldsString))
            {
                selecFieldsString = "," + selecFieldsString;
            }
            var clause = new SingleClause()
            {
                DataType = EnumDataType.BigInt,
                FieldName = CategoryFieldConstants.F_Id,
                Operator = EnumOperator.InList,
                Value = employeeIds
            };
            var employeeView = $"v{OrganizationConstants.EMPLOYEE_CATEGORY_CODE}";
            var condition = new StringBuilder();
            var sqlParams = new List<SqlParameter>();
            int prefix = 0;
            prefix = clause.FilterClauseProcess(employeeView, employeeView, condition, sqlParams, prefix, false, null, null);
            var employeeData = await _organizationDBContext.QueryDataTableRaw($"SELECT {CategoryFieldConstants.F_Id} {selecFieldsString} FROM {employeeView} WHERE {condition}", sqlParams.ToArray());
            return employeeData.ConvertData();


        }

    }
}
