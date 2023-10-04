using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.Data.SqlClient;
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
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.OrganizationDB;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Organization.Model.Salary;

namespace VErp.Services.Organization.Service.Salary.Implement
{
    public class SalaryFieldService : ISalaryFieldService
    {
        private readonly OrganizationDBContext _organizationDBContext;
        private readonly ICurrentContextService _currentContextService;
        private readonly IMapper _mapper;
        private readonly ObjectActivityLogFacade _salaryFieldActivityLog;

        public SalaryFieldService(OrganizationDBContext organizationDBContext, ICurrentContextService currentContextService, IMapper mapper, IActivityLogService activityLogService)
        {
            _organizationDBContext = organizationDBContext;
            _currentContextService = currentContextService;
            _mapper = mapper;
            _salaryFieldActivityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.SalaryField);
        }
        public async Task<int> Create(SalaryFieldModel model)
        {
            await ValidateSalaryField(0, model);
            var info = _mapper.Map<SalaryField>(model);
            using (var trans = await _organizationDBContext.Database.BeginTransactionAsync())
            {
                await _organizationDBContext.SalaryField.AddAsync(info);
                await _organizationDBContext.SaveChangesAsync();


                if (!info.IsDisplayRefData)
                {
                    await ModifyColumn(null, info.SalaryFieldName, (EnumDataType)info.DataTypeId);
                }

                await DropAndCreateSalaryFlatData();

                await trans.CommitAsync();
            }
           

            await _salaryFieldActivityLog.LogBuilder(() => SalaryFieldActivityLogMessage.Create)
             .MessageResourceFormatDatas(model.SalaryFieldName, model.Title)
             .ObjectId(info.SalaryFieldId)
             .JsonData(model)
             .CreateLog();

            return info.SalaryFieldId;
        }

        public async Task<bool> Delete(int salaryFieldId)
        {
            var info = await _organizationDBContext.SalaryField.FirstOrDefaultAsync(s => s.SalaryFieldId == salaryFieldId);
            if (info == null)
            {
                throw GeneralCode.ItemNotFound.BadRequest();
            }
            var usingEmployeeValue = await (
                 from v in _organizationDBContext.SalaryEmployeeValue
                 join e in _organizationDBContext.SalaryEmployee on v.SalaryEmployeeId equals e.SalaryEmployeeId
                 join p in _organizationDBContext.SalaryPeriod on e.SalaryPeriodId equals p.SalaryPeriodId
                 join g in _organizationDBContext.SalaryGroup on e.SalaryGroupId equals g.SalaryGroupId
                 where v.SalaryFieldId == salaryFieldId && v.Value != null
                 select new
                 {
                     e.SalaryGroupId,
                     g.Title,
                     e.SalaryPeriodId,
                     p.Year,
                     p.Month
                 }
                ).FirstOrDefaultAsync();

            if (usingEmployeeValue != null)
            {
                throw SalaryFieldValidationMessage.SalaryFieldInUsed.BadRequestFormat(info.SalaryFieldName + " (" + info.Title + ")", usingEmployeeValue.Title, usingEmployeeValue.Month, usingEmployeeValue.Year);
            }

            var nullEmployeeValues = await (
                from v in _organizationDBContext.SalaryEmployeeValue
                join e in _organizationDBContext.SalaryEmployee on v.SalaryEmployeeId equals e.SalaryEmployeeId
                where v.SalaryFieldId == salaryFieldId && v.Value == null
                select v
               ).ToListAsync();

            using (var trans = await _organizationDBContext.Database.BeginTransactionAsync())
            {
                _organizationDBContext.SalaryEmployeeValue.RemoveRange(nullEmployeeValues);
                await _organizationDBContext.SaveChangesAsync();

                var groupFields = await _organizationDBContext.SalaryGroupField.Where(g => g.SalaryFieldId == salaryFieldId).ToListAsync();
                _organizationDBContext.SalaryGroupField.RemoveRange(groupFields);
                await _organizationDBContext.SaveChangesAsync();

                info.IsDeleted = true;
                await _organizationDBContext.SaveChangesAsync();


                if (!info.IsDisplayRefData)
                {
                    await ModifyColumn(info.SalaryFieldName, null, (EnumDataType)info.DataTypeId);
                }

                await DropAndCreateSalaryFlatData();

                await trans.CommitAsync();
            }

          

            await _salaryFieldActivityLog.LogBuilder(() => SalaryFieldActivityLogMessage.Delete)
             .MessageResourceFormatDatas(info.SalaryFieldName, info.Title)
             .ObjectId(info.SalaryFieldId)
             .JsonData(info)
             .CreateLog();

            return true;
        }

        public async Task<IList<SalaryFieldModel>> GetList()
        {
            return await _organizationDBContext.SalaryField.ProjectTo<SalaryFieldModel>(_mapper.ConfigurationProvider)
                .OrderBy(s => s.SortOrder)
                .ToListAsync();
        }

        public async Task<bool> Update(int salaryFieldId, SalaryFieldModel model)
        {
            await ValidateSalaryField(salaryFieldId, model);

            var info = await _organizationDBContext.SalaryField.FirstOrDefaultAsync(s => s.SalaryFieldId == salaryFieldId);
            if (info == null)
            {
                throw GeneralCode.ItemNotFound.BadRequest();
            }

            //Can not use any here because EF 5 not generate global filter IsDeleted for SalaryEmployee
            var existedValue = await _organizationDBContext.SalaryEmployeeValue.Include(v => v.SalaryEmployee).FirstOrDefaultAsync(f => f.SalaryFieldId == salaryFieldId);

            if (existedValue != null && (int)model.DataTypeId != info.DataTypeId)
            {
                throw SalaryFieldValidationMessage.CannotChangeDataTypeOfSalaryField.BadRequestFormat(info.SalaryFieldName);
            }

            using (var trans = await _organizationDBContext.Database.BeginTransactionAsync())
            {
                var oldFieldName = "";
                if (!info.IsDisplayRefData)
                {
                    oldFieldName = info.SalaryFieldName;
                }

                _mapper.Map(model, info);
                info.SalaryFieldId = salaryFieldId;
                await _organizationDBContext.SaveChangesAsync();

                if (!model.IsEditable)
                {
                    await _organizationDBContext.SalaryGroupField.Where(f => f.SalaryFieldId == salaryFieldId)
                       .UpdateByBatch(f => new SalaryGroupField { IsEditable = model.IsEditable });
                }

                if (!model.IsDisplayRefData)
                    await ModifyColumn(oldFieldName, model.SalaryFieldName, model.DataTypeId);

                await DropAndCreateSalaryFlatData();

                await trans.CommitAsync();
            }
            

            await _salaryFieldActivityLog.LogBuilder(() => SalaryFieldActivityLogMessage.Update)
               .MessageResourceFormatDatas(info.SalaryFieldName, info.Title)
               .ObjectId(info.SalaryFieldId)
               .JsonData(info)
               .CreateLog();

            return true;
        }

        private async Task ValidateSalaryField(int salaryFieldId, SalaryFieldModel model)
        {
            if (await _organizationDBContext.SalaryField.AnyAsync(f => f.SalaryFieldId != salaryFieldId && f.SalaryFieldName == model.SalaryFieldName))
            {
                throw SalaryFieldValidationMessage.SalaryFieldAlreadyExisted.BadRequestFormat(model.SalaryFieldName);
            }

            if (model.IsDisplayRefData && model.IsEditable)
            {
                throw SalaryFieldValidationMessage.RefDataFieldCanNotEditable.BadRequestFormat(model.SalaryFieldName);
            }
        }

        private async Task DropAndCreateSalaryFlatData()
        {
            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@SubsidiaryId", _currentContextService.SubsidiaryId)
            };
            await _organizationDBContext.ExecuteStoreProcedure("usp_DropAndCreateSalaryFlatData", parameters);
        }

        private async Task ModifyColumn(string oldFieldName, string newFieldName, EnumDataType dataTypeId)
        {

            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@SubId", _currentContextService.SubsidiaryId),
                new SqlParameter("@OldFieldName", oldFieldName),
                new SqlParameter("@NewFieldName", newFieldName),
                new SqlParameter("@DataTypeId",(int)dataTypeId)
            };
            await _organizationDBContext.ExecuteStoreProcedure("asp_SalaryEmployeeTable_UpdateField", parameters);
        }
    }
}
