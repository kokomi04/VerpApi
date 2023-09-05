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
    public class SalaryPeriodAdditionTypeService : ISalaryPeriodAdditionTypeService
    {
        private readonly OrganizationDBContext _organizationDBContext;
        private readonly ICurrentContextService _currentContextService;
        private readonly IMapper _mapper;
        private readonly ObjectActivityLogFacade _typeActivityLog;

        public SalaryPeriodAdditionTypeService(OrganizationDBContext organizationDBContext, ICurrentContextService currentContextService, IMapper mapper, IActivityLogService activityLogService)
        {
            _organizationDBContext = organizationDBContext;
            _currentContextService = currentContextService;
            _mapper = mapper;
            _typeActivityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.SalaryPeriodAdditionType);
        }


        public async Task<int> Create(SalaryPeriodAdditionTypeModel model)
        {
            var info = _mapper.Map<SalaryPeriodAdditionType>(model);

            var lstFields = _mapper.Map<List<SalaryPeriodAdditionTypeField>>(model.Fields);

            using (var trans = await _organizationDBContext.Database.BeginTransactionAsync())
            {
                await _organizationDBContext.SalaryPeriodAdditionType.AddAsync(info);
                await _organizationDBContext.SaveChangesAsync();

                foreach (var f in lstFields)
                {
                    f.SalaryPeriodAdditionTypeId = info.SalaryPeriodAdditionTypeId;
                }

                await _organizationDBContext.SalaryPeriodAdditionTypeField.AddRangeAsync(lstFields);
                await _organizationDBContext.SaveChangesAsync();

                await trans.CommitAsync();
            }

            await _typeActivityLog.LogBuilder(() => SalaryPeriodAdditionTypeActivityLogMessage.Create)
                .MessageResourceFormatDatas(model.Title)
                .ObjectId(info.SalaryPeriodAdditionTypeId)
                .JsonData(model)
                .CreateLog();
            return info.SalaryPeriodAdditionTypeId;
        }


        public async Task<bool> Delete(int salaryPeriodAdditionTypeId)
        {
            var info = await _organizationDBContext.SalaryPeriodAdditionType.FirstOrDefaultAsync(s => s.SalaryPeriodAdditionTypeId == salaryPeriodAdditionTypeId);
            if (info == null)
            {
                throw GeneralCode.ItemNotFound.BadRequest();
            }

            if (await _organizationDBContext.SalaryPeriodAdditionBill.AnyAsync(g => g.SalaryPeriodAdditionTypeId == salaryPeriodAdditionTypeId))
            {
                throw SalaryPeriodAdditionTypeValidationMessage.TypeInUsed.BadRequestFormat(info.Title);
            }

            info.IsDeleted = true;

            await _organizationDBContext.SaveChangesAsync();
            await _typeActivityLog.LogBuilder(() => SalaryPeriodAdditionTypeActivityLogMessage.Delete)
                .MessageResourceFormatDatas(info.Title)
                .ObjectId(info.SalaryPeriodAdditionTypeId)
                .JsonData(info)
                .CreateLog();
            return true;
        }

        public async Task<SalaryPeriodAdditionTypeInfo> GetInfo(int salaryPeriodAdditionTypeId)
        {
            var info = await _organizationDBContext.SalaryPeriodAdditionType.Include(t => t.SalaryPeriodAdditionTypeField).FirstOrDefaultAsync(t => t.SalaryPeriodAdditionTypeId == salaryPeriodAdditionTypeId);
            if (info == null)
            {
                throw GeneralCode.ItemNotFound.BadRequest();
            }

            var model = _mapper.Map<SalaryPeriodAdditionTypeInfo>(info);
            model.Fields = _mapper.Map<List<SalaryPeriodAdditionTypeFieldModel>>(info.SalaryPeriodAdditionTypeField).OrderBy(f => f.SortOrder).ToList();


            return model;
        }

        public async Task<SalaryPeriodAdditionType> GetFullEntityInfo(int salaryPeriodAdditionTypeId)
        {
            var info = await _organizationDBContext.SalaryPeriodAdditionType
              .Include(t => t.SalaryPeriodAdditionTypeField)
              .ThenInclude(tf => tf.SalaryPeriodAdditionField)
              .Where(t => t.SalaryPeriodAdditionTypeId == salaryPeriodAdditionTypeId)
              .FirstOrDefaultAsync();

            if (info != null)
            {
                info.SalaryPeriodAdditionTypeField = info.SalaryPeriodAdditionTypeField.OrderBy(f => f.SortOrder).ToList();
            }

            return info;

        }
        public async Task<IEnumerable<SalaryPeriodAdditionTypeInfo>> List()
        {
            var lst = await _organizationDBContext.SalaryPeriodAdditionType.Include(t => t.SalaryPeriodAdditionTypeField).ToListAsync();
            var result = new List<SalaryPeriodAdditionTypeInfo>();
            foreach (var item in lst)
            {
                var model = _mapper.Map<SalaryPeriodAdditionTypeInfo>(item);
                model.Fields = _mapper.Map<List<SalaryPeriodAdditionTypeFieldModel>>(item.SalaryPeriodAdditionTypeField).OrderBy(f => f.SortOrder).ToList();
                result.Add(model);
            }

            return result;
        }


        public async Task<bool> Update(int salaryPeriodAdditionTypeId, SalaryPeriodAdditionTypeModel model)
        {
            var info = await _organizationDBContext.SalaryPeriodAdditionType.Include(t => t.SalaryPeriodAdditionTypeField).FirstOrDefaultAsync(s => s.SalaryPeriodAdditionTypeId == salaryPeriodAdditionTypeId);
            if (info == null)
            {
                throw GeneralCode.ItemNotFound.BadRequest();
            }

            _mapper.Map(model, info);
            info.SalaryPeriodAdditionTypeId = salaryPeriodAdditionTypeId;

            var lstFields = _mapper.Map<List<SalaryPeriodAdditionTypeField>>(model.Fields);

            var deleteFields = info.SalaryPeriodAdditionTypeField.Select(f => f.SalaryPeriodAdditionFieldId)
                .ToList()
                .Except(lstFields.Select(f => f.SalaryPeriodAdditionFieldId))
                .ToList();
            if (deleteFields.Count > 0)
            {
                var usedBill = await (from bill in _organizationDBContext.SalaryPeriodAdditionBill
                                      join em in _organizationDBContext.SalaryPeriodAdditionBillEmployee on bill.SalaryPeriodAdditionBillId equals em.SalaryPeriodAdditionBillId
                                      join fv in _organizationDBContext.SalaryPeriodAdditionBillEmployeeValue on em.SalaryPeriodAdditionBillEmployeeId equals fv.SalaryPeriodAdditionBillEmployeeId
                                      join f in _organizationDBContext.SalaryPeriodAdditionField on fv.SalaryPeriodAdditionFieldId equals f.SalaryPeriodAdditionFieldId
                                      join t in _organizationDBContext.SalaryPeriodAdditionType on bill.SalaryPeriodAdditionTypeId equals t.SalaryPeriodAdditionTypeId
                                      where bill.SalaryPeriodAdditionTypeId == salaryPeriodAdditionTypeId
                                      && fv.SalaryPeriodAdditionFieldId == deleteFields[0]
                                      select new
                                      {
                                          bill.SalaryPeriodAdditionBillId,
                                          bill.SalaryPeriodAdditionTypeId,
                                          TypeTitle = t.Title,
                                          em.EmployeeId,
                                          fv.SalaryPeriodAdditionFieldId,
                                          fv.Value,
                                          f.FieldName,
                                          FieldTitle = f.Title
                                      }).FirstOrDefaultAsync();

                if (usedBill != null)
                {
                    throw SalaryPeriodAdditionTypeValidationMessage.FieldInTypeInUsed.BadRequestFormat(usedBill.FieldName, usedBill.FieldTitle, usedBill.TypeTitle);
                }
            }

            using (var trans = await _organizationDBContext.Database.BeginTransactionAsync())
            {
                foreach (var f in lstFields)
                {
                    f.SalaryPeriodAdditionTypeId = info.SalaryPeriodAdditionTypeId;
                }

                _organizationDBContext.SalaryPeriodAdditionTypeField.RemoveRange(info.SalaryPeriodAdditionTypeField);
                await _organizationDBContext.SaveChangesAsync();

                await _organizationDBContext.SalaryPeriodAdditionTypeField.AddRangeAsync(lstFields);
                await _organizationDBContext.SaveChangesAsync();
                await trans.CommitAsync();
            }

            await _typeActivityLog.LogBuilder(() => SalaryPeriodAdditionTypeActivityLogMessage.Update)
                .MessageResourceFormatDatas(model.Title)
                .ObjectId(info.SalaryPeriodAdditionTypeId)
                .JsonData(model)
                .CreateLog();
            return true;
        }
    }
}
