using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verp.Resources.Organization.Salary;
using Verp.Resources.Organization.Salary.Validation;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.OrganizationDB;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Organization.Model.Salary;

namespace VErp.Services.Organization.Service.Salary.Implement
{
    public class SalaryPeriodAdditionFieldService : ISalaryPeriodAdditionFieldService
    {
        private readonly OrganizationDBContext _organizationDBContext;
        private readonly ICurrentContextService _currentContextService;
        private readonly IMapper _mapper;
        private readonly ObjectActivityLogFacade _fieldActivityLog;

        public SalaryPeriodAdditionFieldService(OrganizationDBContext organizationDBContext, ICurrentContextService currentContextService, IMapper mapper, IActivityLogService activityLogService)
        {
            _organizationDBContext = organizationDBContext;
            _currentContextService = currentContextService;
            _mapper = mapper;
            _fieldActivityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.SalaryPeriodAdditionField);
        }

        public async Task<IList<SalaryPeriodAdditionFieldInfo>> List()
        {
            return await _organizationDBContext.SalaryPeriodAdditionField.ProjectTo<SalaryPeriodAdditionFieldInfo>(_mapper.ConfigurationProvider)
               .OrderBy(s => s.FieldName)
               .ToListAsync();
        }


        public async Task<int> Create(SalaryPeriodAdditionFieldModel model)
        {
            await ValidateSalaryPeriodAdditionField(0, model);
            var info = _mapper.Map<SalaryPeriodAdditionField>(model);
            await _organizationDBContext.SalaryPeriodAdditionField.AddAsync(info);
            await _organizationDBContext.SaveChangesAsync();

            await _fieldActivityLog.LogBuilder(() => SalaryPeriodAdditionFieldActivityLogMessage.Create)
               .MessageResourceFormatDatas(info.FieldName, info.Title)
               .ObjectId(info.SalaryPeriodAdditionFieldId)
               .JsonData(info.JsonSerialize())
               .CreateLog();

            return info.SalaryPeriodAdditionFieldId;
        }

        public async Task<bool> Update(int salaryPeriodAdditionFieldId, SalaryPeriodAdditionFieldModel model)
        {
            await ValidateSalaryPeriodAdditionField(salaryPeriodAdditionFieldId, model);

            var info = await _organizationDBContext.SalaryPeriodAdditionField.FirstOrDefaultAsync(s => s.SalaryPeriodAdditionFieldId == salaryPeriodAdditionFieldId);
            if (info == null)
            {
                throw GeneralCode.ItemNotFound.BadRequest();
            }

            _mapper.Map(model, info);
            info.SalaryPeriodAdditionFieldId = salaryPeriodAdditionFieldId;
            await _organizationDBContext.SaveChangesAsync();

            await _fieldActivityLog.LogBuilder(() => SalaryPeriodAdditionFieldActivityLogMessage.Update)
              .MessageResourceFormatDatas(info.FieldName, info.Title)
              .ObjectId(info.SalaryPeriodAdditionFieldId)
              .JsonData(info.JsonSerialize())
              .CreateLog();

            return true;
        }

        public async Task<bool> Delete(int salaryPeriodAdditionFieldId)
        {
            var info = await _organizationDBContext.SalaryPeriodAdditionField.FirstOrDefaultAsync(s => s.SalaryPeriodAdditionFieldId == salaryPeriodAdditionFieldId);
            if (info == null)
            {
                throw GeneralCode.ItemNotFound.BadRequest();
            }

            var existedData = await (from v in _organizationDBContext.SalaryPeriodAdditionBillEmployeeValue
                                     join e in _organizationDBContext.SalaryPeriodAdditionBillEmployee on v.SalaryPeriodAdditionBillEmployeeId equals e.SalaryPeriodAdditionBillEmployeeId
                                     join b in _organizationDBContext.SalaryPeriodAdditionBill on e.SalaryPeriodAdditionBillId equals b.SalaryPeriodAdditionBillId
                                     where v.SalaryPeriodAdditionFieldId == salaryPeriodAdditionFieldId
                                     select new
                                     {
                                         v.Value,
                                         v.SalaryPeriodAdditionFieldId,
                                         e.SalaryPeriodAdditionBillId,
                                         e.EmployeeId,
                                         b.BillCode
                                     }
                                      ).FirstOrDefaultAsync();

            if (existedData != null)
            {
                throw SalaryPeriodAdditionFieldValiationMessage.FieldInUsed.BadRequestFormat(info.FieldName);
            }

            var groupField = await _organizationDBContext.SalaryPeriodAdditionTypeField.Where(g => g.SalaryPeriodAdditionFieldId == salaryPeriodAdditionFieldId).FirstOrDefaultAsync();

            if (groupField != null)
            {
                throw SalaryPeriodAdditionFieldValiationMessage.FieldInUsed.BadRequestFormat(info.FieldName);
            }

            //_organizationDBContext.SalaryPeriodAdditionTypeField.RemoveRange(groupFields);
            //await _organizationDBContext.SaveChangesAsync();
            info.IsDeleted = true;
            //_organizationDBContext.SalaryPeriodAdditionField.Remove(info);

            await _organizationDBContext.SaveChangesAsync();

            await _fieldActivityLog.LogBuilder(() => SalaryPeriodAdditionFieldActivityLogMessage.Delete)
              .MessageResourceFormatDatas(info.FieldName, info.Title)
              .ObjectId(info.SalaryPeriodAdditionFieldId)
              .JsonData(info.JsonSerialize())
              .CreateLog();
            return true;
        }


        private async Task ValidateSalaryPeriodAdditionField(int salaryPeriodAdditionFieldId, SalaryPeriodAdditionFieldModel model)
        {
            if (await _organizationDBContext.SalaryPeriodAdditionField.AnyAsync(f => f.SalaryPeriodAdditionFieldId != salaryPeriodAdditionFieldId && f.FieldName == model.FieldName))
            {
                throw SalaryPeriodAdditionFieldValiationMessage.FieldNameAlreadyExisted.BadRequestFormat(model.FieldName);
            }


            var invalidFieldNames = new HashSet<string>()
            {
                nameof(SalaryPeriodAdditionBillEmployeeModel.EmployeeId),
                nameof(SalaryPeriodAdditionBillEmployeeModel.Description),
            };
            var fields = ExcelUtils.GetFieldNameModels<SalaryPeriodAdditionBillEmployeeModel>();
            foreach (var field in fields)
            {
                invalidFieldNames.Add(field.FieldName);
            }
            invalidFieldNames = invalidFieldNames.Select(f => f.ToLower()).ToHashSet();

            if (invalidFieldNames.Contains(model.FieldName.ToLower()))
            {
                throw SalaryPeriodAdditionFieldValiationMessage.FieldInUsed.BadRequestFormat(model.FieldName);
            }
        }
    }
}
