using AutoMapper;
using DocumentFormat.OpenXml.EMMA;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verp.Resources.Organization.Salary;
using Verp.Resources.Organization.Salary.Validation;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.Organization.Salary;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.OrganizationDB;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Organization.Model.Salary;

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
                .JsonData(model.JsonSerialize())
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
                .JsonData(info.JsonSerialize())
                .CreateLog();
            return true;
        }

        public async Task<IList<SalaryGroupModel>> GetList()
        {
            var lst = await _organizationDBContext.SalaryGroup.Include(t => t.SalaryGroupField).ToListAsync();

            var result = new List<SalaryGroupModel>();
            foreach (var item in lst)
            {
                var model = _mapper.Map<SalaryGroupModel>(item);
                model.TableFields = _mapper.Map<List<SalaryGroupFieldModel>>(item.SalaryGroupField).OrderBy(f => f.SortOrder).ToList();
                result.Add(model);
            }

            return result;
        }


        public async Task<SalaryGroupModel> GetInfo(int salaryGroupId)
        {
            var info = await _organizationDBContext.SalaryGroup.Include(t => t.SalaryGroupField).FirstOrDefaultAsync(s => s.SalaryGroupId == salaryGroupId);
            if (info == null)
            {
                throw GeneralCode.ItemNotFound.BadRequest();
            }

            var model = _mapper.Map<SalaryGroupModel>(info);
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

            _mapper.Map(model, info);
            info.SalaryGroupId = salaryGroupId;

            var lstFields = _mapper.Map<List<SalaryGroupField>>(model.TableFields);

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
                .JsonData(model.JsonSerialize())
                .CreateLog();
            return true;
        }
    }
}
