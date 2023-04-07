using AutoMapper;
using AutoMapper.QueryableExtensions;
using DocumentFormat.OpenXml.EMMA;
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
                .SetConfig(EnumObjectType.SalaryPeriodAdditionBill, EnumObjectType.SalaryPeriodAdditionType, salaryPeriodAdditionTypeId)
                .SetConfigData(0, date.GetUnixUtc(_currentContextService.TimeZoneOffset))
                .TryValidateAndGenerateCode(_organizationDBContext.SalaryPeriodAdditionBill, model.BillCode, (s, code) => s.SalaryPeriodAdditionTypeId == salaryPeriodAdditionTypeId && s.BillCode == code);

            model.BillCode = code;

            using var trans = await _organizationDBContext.Database.BeginTransactionAsync();

            var info = await CreateToDb(typeInfo, model);

            await trans.CommitAsync();

            await _billActivityLog.LogBuilder(() => SalaryPeriodAdditionBillActivityLogMessage.Create)
            .MessageResourceFormatDatas(info.BillCode, typeInfo.Title)
            .ObjectId(info.SalaryPeriodAdditionBillId)
            .JsonData(model.JsonSerialize())
            .CreateLog();

            await ctx.ConfirmCode();

            return info.SalaryPeriodAdditionBillId;
        }


        public async Task<SalaryPeriodAdditionBill> CreateToDb(SalaryPeriodAdditionType typFullInfo, SalaryPeriodAdditionBillModel model)
        {
            await ValidateModel(model, null);


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

            using var trans = await _organizationDBContext.Database.BeginTransactionAsync();

            var r = await UpdateToDb(typeInfo, salaryPeriodAdditionBillId, model);

            await trans.CommitAsync();

            await _billActivityLog.LogBuilder(() => SalaryPeriodAdditionBillActivityLogMessage.Update)
            .MessageResourceFormatDatas(model.BillCode, typeInfo.Title)
            .ObjectId(salaryPeriodAdditionBillId)
            .JsonData(model.JsonSerialize())
            .CreateLog();


            return r;
        }

        public async Task<bool> UpdateToDb(SalaryPeriodAdditionType typFullInfo, long salaryPeriodAdditionBillId, SalaryPeriodAdditionBillModel model)
        {

            var info = await QueryFullInfo().FirstOrDefaultAsync(b => b.SalaryPeriodAdditionTypeId == typFullInfo.SalaryPeriodAdditionTypeId && b.SalaryPeriodAdditionBillId == salaryPeriodAdditionBillId);
            if (info == null)
            {
                throw GeneralCode.ItemNotFound.BadRequest();
            }

            await ValidateModel(model, info);


            _mapper.Map(model, info);
            info.SalaryPeriodAdditionBillId = salaryPeriodAdditionBillId;
            info.SalaryPeriodAdditionTypeId = typFullInfo.SalaryPeriodAdditionTypeId;

            _organizationDBContext.SalaryPeriodAdditionBillEmployeeValue.RemoveRange(info.SalaryPeriodAdditionBillEmployee.SelectMany(e => e.SalaryPeriodAdditionBillEmployeeValue));
            _organizationDBContext.SalaryPeriodAdditionBillEmployee.RemoveRange(info.SalaryPeriodAdditionBillEmployee);
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

            await ValidateModel(null, info);

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

        private async Task ValidateModel(SalaryPeriodAdditionBillModel model, SalaryPeriodAdditionBill info)
        {
            DateTime? modelPeriodDate = null;
            DateTime? infoPeriodDate = null;
            if (model != null)
            {
                if (model.Details?.Count <= 0)
                {
                    throw SalaryPeriodAdditionTypeValidationMessage.RequireAtLeastOneDetail.BadRequestFormat(model.BillCode);
                }

                if (string.IsNullOrWhiteSpace(model.BillCode))
                {
                    throw SalaryPeriodAdditionTypeValidationMessage.BillCodeIsRequired.BadRequest();
                }

                modelPeriodDate= new DateTime(model.Year ?? 0, model.Month ?? 0, 1).AddMinutes(_currentContextService.TimeZoneOffset ?? -420);
            }
            if (info != null)
            {
                infoPeriodDate = new DateTime(info.Year, info.Month, 1).AddMinutes(_currentContextService.TimeZoneOffset ?? -420);
            }
           

            await ValidateDateOfBill(modelPeriodDate, infoPeriodDate);

            await ValidateDateOfBill(model.Date.UnixToDateTime(), info?.Date);
        }

    }
}
