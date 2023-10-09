using AutoMapper;
using DocumentFormat.OpenXml.EMMA;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verp.Resources.Organization.Salary;
using Verp.Resources.Organization.Salary.Validation;
using VErp.Commons.Constants;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.Organization.Salary;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.OrganizationDB;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Organization.Model.Salary;
using static VErp.Services.Organization.Service.Salary.Implement.SalaryFieldService;

namespace VErp.Services.Organization.Service.Salary.Implement
{
    public class SalaryGroupService : ISalaryGroupService
    {
        private readonly OrganizationDBContext _organizationDBContext;
        private readonly ICurrentContextService _currentContextService;
        private readonly IMapper _mapper;
        private readonly ObjectActivityLogFacade _salaryGroupActivityLog;

        public SalaryGroupService(OrganizationDBContext organizationDBContext, ICurrentContextService currentContextService, IMapper mapper, IActivityLogService activityLogService)
        {
            _organizationDBContext = organizationDBContext;
            _currentContextService = currentContextService;
            _mapper = mapper;
            _salaryGroupActivityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.SalaryGroup);
        }

        public async Task<int> Create(SalaryGroupModel model)
        {
            var info = _mapper.Map<SalaryGroup>(model);

            var lstFields = _mapper.Map<List<SalaryGroupField>>(model.TableFields);

            using (var trans = await _organizationDBContext.Database.BeginTransactionAsync())
            {
                await _organizationDBContext.SalaryGroup.AddAsync(info);
                await _organizationDBContext.SaveChangesAsync();

                foreach (var f in lstFields)
                {
                    f.SalaryGroupId = info.SalaryGroupId;
                }

                await _organizationDBContext.SalaryGroupField.AddRangeAsync(lstFields);
                await _organizationDBContext.SaveChangesAsync();

                await trans.CommitAsync();
            }

            await _salaryGroupActivityLog.LogBuilder(() => SalaryGroupActivityLogMessage.Create)
                .MessageResourceFormatDatas(model.Title)
                .ObjectId(info.SalaryGroupId)
                .JsonData(model)
                .CreateLog();
            return info.SalaryGroupId;
        }

        public async Task<bool> Delete(int salaryGroupId)
        {
            var info = await _organizationDBContext.SalaryGroup.FirstOrDefaultAsync(s => s.SalaryGroupId == salaryGroupId);
            if (info == null)
            {
                throw GeneralCode.ItemNotFound.BadRequest();
            }

            if (await _organizationDBContext.SalaryPeriodGroup.AnyAsync(g => g.SalaryGroupId == salaryGroupId))
            {
                throw SalaryGroupValidationMessage.SalaryGroupInUsed.BadRequestFormat(info.Title);
            }

            info.IsDeleted = true;

            await _organizationDBContext.SaveChangesAsync();
            await _salaryGroupActivityLog.LogBuilder(() => SalaryGroupActivityLogMessage.Delete)
                .MessageResourceFormatDatas(info.Title)
                .ObjectId(info.SalaryGroupId)
                .JsonData(info)
                .CreateLog();
            return true;
        }

        public async Task<IList<SalaryGroupInfo>> GetList()
        {
            var lst = await _organizationDBContext.SalaryGroup.Include(t => t.SalaryGroupField).ToListAsync();

            var result = new List<SalaryGroupInfo>();
            foreach (var item in lst)
            {
                var model = _mapper.Map<SalaryGroupInfo>(item);
                model.TableFields = _mapper.Map<List<SalaryGroupFieldModel>>(item.SalaryGroupField).OrderBy(f => f.SortOrder).ToList();
                result.Add(model);
            }

            return result;
        }


        public async Task<SalaryGroupInfo> GetInfo(int salaryGroupId)
        {
            var info = await _organizationDBContext.SalaryGroup.Include(t => t.SalaryGroupField).FirstOrDefaultAsync(s => s.SalaryGroupId == salaryGroupId);
            if (info == null)
            {
                throw GeneralCode.ItemNotFound.BadRequest();
            }

            var model = _mapper.Map<SalaryGroupInfo>(info);
            model.TableFields = _mapper.Map<List<SalaryGroupFieldModel>>(info.SalaryGroupField).OrderBy(f => f.SortOrder).ToList();
            return model;
        }

        public async Task<bool> Update(int salaryGroupId, SalaryGroupModel model)
        {
            var info = await _organizationDBContext.SalaryGroup.Include(t => t.SalaryGroupField).FirstOrDefaultAsync(s => s.SalaryGroupId == salaryGroupId);
            if (info == null)
            {
                throw GeneralCode.ItemNotFound.BadRequest();
            }

            var lstFields = _mapper.Map<List<SalaryGroupField>>(model.TableFields);

            var toRemoveFieldIdsFromGroup = new List<int>();
            foreach (var field in info.SalaryGroupField)
            {
                if (!lstFields.Any(f => f.SalaryFieldId == field.SalaryFieldId))
                {
                    toRemoveFieldIdsFromGroup.Add(field.SalaryFieldId);
                }
            }

            var toRemoveFieldsFromGroup = await _organizationDBContext.SalaryField.Where(f => toRemoveFieldIdsFromGroup.Contains(f.SalaryFieldId)).ToListAsync();
            if (toRemoveFieldsFromGroup.Count > 0)
            {
                var subsidiaryInfo = await _organizationDBContext.Subsidiary.FirstOrDefaultAsync(s => s.SubsidiaryId == _currentContextService.SubsidiaryId);
                if (subsidiaryInfo == null)
                {
                    throw GeneralCode.NotYetSupported.BadRequest();
                }


                foreach (var field in toRemoveFieldsFromGroup)
                {
                    var sql = $"SELECT TOP(1) e.SalaryGroupId, g.Title SalaryGroupTitle, e.SalaryPeriodId, p.Year, p.Month " +
                       $"FROM {OrganizationConstants.GetEmployeeSalaryTableName(subsidiaryInfo.SubsidiaryCode)} e " +
                       $"JOIN SalaryPeriod p ON e.SalaryPeriodId = p.SalaryPeriodId " +
                       $"JOIN SalaryGroup g ON e.SalaryGroupId = g.SalaryGroupId " +
                       $"WHERE e.IsDeleted = 0 e.SalaryGroupId = @SalaryGroupId AND e.[{field.SalaryFieldName}] <> NULL AND e.[{field.SalaryFieldName}] <> '' AND e.[{field.SalaryFieldName}] <> 0";

                    var usingEmployeeValue = (await _organizationDBContext.QueryListRaw<InUsedEmployeeSalaryFieldModel>(sql, new[] { new SqlParameter("@SalaryGroupId", salaryGroupId) })).FirstOrDefault();


                    if (usingEmployeeValue != null)
                    {
                        throw SalaryFieldValidationMessage.SalaryFieldInUsed.BadRequestFormat(field.SalaryFieldName + " (" + field.Title + ")", usingEmployeeValue.SalaryGroupTitle, usingEmployeeValue.Month, usingEmployeeValue.Year);
                    }
                }


            }

            _mapper.Map(model, info);
            info.SalaryGroupId = salaryGroupId;

            using (var trans = await _organizationDBContext.Database.BeginTransactionAsync())
            {
                foreach (var f in lstFields)
                {
                    f.SalaryGroupId = info.SalaryGroupId;
                }

                _organizationDBContext.SalaryGroupField.RemoveRange(info.SalaryGroupField);
                await _organizationDBContext.SaveChangesAsync();

                await _organizationDBContext.SalaryGroupField.AddRangeAsync(lstFields);
                await _organizationDBContext.SaveChangesAsync();
                await trans.CommitAsync();
            }

            await _salaryGroupActivityLog.LogBuilder(() => SalaryGroupActivityLogMessage.Update)
                .MessageResourceFormatDatas(model.Title)
                .ObjectId(info.SalaryGroupId)
                .JsonData(model)
                .CreateLog();
            return true;
        }
    }
}
