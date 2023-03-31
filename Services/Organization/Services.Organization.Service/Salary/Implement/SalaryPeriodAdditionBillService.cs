using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Ocsp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verp.Resources.Organization.Salary;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.OrganizationDB;
using VErp.Infrastructure.ServiceCore.Abstract;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Organization.Model.Salary;

namespace VErp.Services.Organization.Service.Salary.Implement
{
    public class SalaryPeriodAdditionBillService : BillDateValidateionServiceAbstract, ISalaryPeriodAdditionBillService
    {

        private readonly OrganizationDBContext _organizationDBContext;
        private readonly ICurrentContextService _currentContextService;
        private readonly IMapper _mapper;
        private readonly ObjectActivityLogFacade _billActivityLog;
        private readonly ICustomGenCodeHelperService _customGenCodeHelperService;

        public SalaryPeriodAdditionBillService(OrganizationDBContext organizationDBContext, ICurrentContextService currentContextService, IMapper mapper, IActivityLogService activityLogService, ICustomGenCodeHelperService customGenCodeHelperService)
            : base(organizationDBContext)
        {
            _organizationDBContext = organizationDBContext;
            _currentContextService = currentContextService;
            _mapper = mapper;
            _billActivityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.SalaryPeriodAdditionBill);
            _customGenCodeHelperService = customGenCodeHelperService;
        }


        public async Task<long> Create(int salaryPeriodAdditionTypeId, SalaryPeriodAdditionBillModel model)
        {
            var typeInfo = await _organizationDBContext.SalaryPeriodAdditionType.Include(t => t.SalaryPeriodAdditionTypeField)
               .ThenInclude(tf => tf.SalaryPeriodAdditionField)
               .Where(t => t.SalaryPeriodAdditionTypeId == salaryPeriodAdditionTypeId)
               .FirstOrDefaultAsync();

            if (typeInfo == null)
            {
                throw GeneralCode.ItemNotFound.BadRequest();
            }


            var ctx = _customGenCodeHelperService.CreateGenerateCodeContext();

            var date = new DateTime(model.Year, model.Month, 1);

            await ValidateDateOfBill(date, model.Date.UnixToDateTime());

            var code = await ctx
                .SetConfig(EnumObjectType.SalaryPeriodAdditionBill, EnumObjectType.SalaryPeriodAdditionType, salaryPeriodAdditionTypeId)
                .SetConfigData(0, date.GetUnixUtc(_currentContextService.TimeZoneOffset))
                .TryValidateAndGenerateCode(_organizationDBContext.SalaryPeriodAdditionBill, model.BillCode, (s, code) => s.SalaryPeriodAdditionTypeId == salaryPeriodAdditionTypeId && s.BillCode == code);

            model.BillCode = code;


            using var trans = await _organizationDBContext.Database.BeginTransactionAsync();

            var info = _mapper.Map<SalaryPeriodAdditionBill>(model);
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


            var fields = typeInfo.SalaryPeriodAdditionTypeField.ToDictionary(f => f.SalaryPeriodAdditionField.FieldName, f => f.SalaryPeriodAdditionField);

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

            await trans.CommitAsync();

            await _billActivityLog.LogBuilder(() => SalaryPeriodAdditionBillActivityLogMessage.Create)
            .MessageResourceFormatDatas(info.BillCode, typeInfo.Title)
            .ObjectId(info.SalaryPeriodAdditionBillId)
            .JsonData(model.JsonSerialize())
            .CreateLog();

            await ctx.ConfirmCode();

            return info.SalaryPeriodAdditionBillId;
        }

        public async Task<bool> Delete(int salaryPeriodAdditionTypeId, long salaryPeriodAdditionBillId)
        {
            var typeInfo = await _organizationDBContext.SalaryPeriodAdditionType.FirstOrDefaultAsync();

            if (typeInfo == null)
            {
                throw GeneralCode.ItemNotFound.BadRequest();
            }

            using var trans = await _organizationDBContext.Database.BeginTransactionAsync();

            var info = await _organizationDBContext.SalaryPeriodAdditionBill.FirstOrDefaultAsync(b => b.SalaryPeriodAdditionTypeId == salaryPeriodAdditionTypeId && b.SalaryPeriodAdditionBillId == salaryPeriodAdditionBillId);
            if (info == null)
            {
                throw GeneralCode.ItemNotFound.BadRequest();
            }

            var date = new DateTime(info.Year, info.Month, 1);

            await ValidateDateOfBill(date, info.Date);

            info.IsDeleted = true;

            await trans.CommitAsync();

            await _billActivityLog.LogBuilder(() => SalaryPeriodAdditionBillActivityLogMessage.Delete)
            .MessageResourceFormatDatas(info.BillCode, typeInfo.Title)
            .ObjectId(info.SalaryPeriodAdditionBillId)
            .JsonData(info.JsonSerialize())
            .CreateLog();

            return true;
        }

        public async Task<SalaryPeriodAdditionBillInfo> GetInfo(int salaryPeriodAdditionTypeId, long salaryPeriodAdditionBillId)
        {
            var entity = await _organizationDBContext.SalaryPeriodAdditionBill.FirstOrDefaultAsync(b => b.SalaryPeriodAdditionTypeId == salaryPeriodAdditionTypeId && b.SalaryPeriodAdditionBillId == salaryPeriodAdditionBillId);
            if (entity == null)
            {
                throw GeneralCode.ItemNotFound.BadRequest();
            }

            var info = _mapper.Map<SalaryPeriodAdditionBillInfo>(entity);

            var values = await _organizationDBContext.SalaryPeriodAdditionBillEmployeeValue
                .Include(e => e.SalaryPeriodAdditionBillEmployee)
                .Include(e => e.SalaryPeriodAdditionField)
                .Where(e => e.SalaryPeriodAdditionBillEmployee.SalaryPeriodAdditionBillId == salaryPeriodAdditionBillId).ToListAsync();

            //for case no values
            var details = await _organizationDBContext.SalaryPeriodAdditionBillEmployee.Where(e => e.SalaryPeriodAdditionBillId == salaryPeriodAdditionBillId).ToListAsync();

            info.Details = new List<SalaryPeriodAdditionBillEmployeeModel>();

            foreach (var detail in details)
            {
                var detailValues = values.Where(v => v.SalaryPeriodAdditionBillEmployeeId == detail.SalaryPeriodAdditionBillEmployeeId).ToList();
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

        public async Task<PageData<SalaryPeriodAdditionBillList>> GetList(int salaryPeriodAdditionTypeId, int? year, int? month, int page, int size)
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
            var total = await query.CountAsync();
            var lst = await query.ProjectTo<SalaryPeriodAdditionBillList>(_mapper.ConfigurationProvider).Skip(page - 1).Take(size).ToListAsync();
            return (lst, total);
        }

        public async Task<bool> Update(int salaryPeriodAdditionTypeId, long salaryPeriodAdditionBillId, SalaryPeriodAdditionBillModel model)
        {
            var typeInfo = await _organizationDBContext.SalaryPeriodAdditionType.Include(t => t.SalaryPeriodAdditionTypeField)
               .ThenInclude(tf => tf.SalaryPeriodAdditionField)
               .Where(t => t.SalaryPeriodAdditionTypeId == salaryPeriodAdditionTypeId)
               .FirstOrDefaultAsync();

            if (typeInfo == null)
            {
                throw GeneralCode.ItemNotFound.BadRequest();
            }

            var info = await _organizationDBContext.SalaryPeriodAdditionBill.FirstOrDefaultAsync(b => b.SalaryPeriodAdditionTypeId == salaryPeriodAdditionTypeId && b.SalaryPeriodAdditionBillId == salaryPeriodAdditionBillId);
            if (info == null)
            {
                throw GeneralCode.ItemNotFound.BadRequest();
            }

            var date = new DateTime(info.Year, info.Month, 1);

            await ValidateDateOfBill(date, info.Date);

            using var trans = await _organizationDBContext.Database.BeginTransactionAsync();

            _mapper.Map(model, info);
            info.SalaryPeriodAdditionBillId = salaryPeriodAdditionBillId;
            info.SalaryPeriodAdditionTypeId = salaryPeriodAdditionTypeId;

            var oldValues = await _organizationDBContext.SalaryPeriodAdditionBillEmployeeValue
                .Include(e => e.SalaryPeriodAdditionBillEmployee)
                .Where(e => e.SalaryPeriodAdditionBillEmployee.SalaryPeriodAdditionBillId == salaryPeriodAdditionBillId).ToListAsync();

            //for case no values
            var oldDetails = await _organizationDBContext.SalaryPeriodAdditionBillEmployee.Where(e => e.SalaryPeriodAdditionBillId == salaryPeriodAdditionBillId).ToListAsync();

            _organizationDBContext.SalaryPeriodAdditionBillEmployeeValue.RemoveRange(oldValues);
            _organizationDBContext.SalaryPeriodAdditionBillEmployee.RemoveRange(oldDetails);
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


            var fields = typeInfo.SalaryPeriodAdditionTypeField.ToDictionary(f => f.SalaryPeriodAdditionField.FieldName, f => f.SalaryPeriodAdditionField);

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

            await trans.CommitAsync();

            await _billActivityLog.LogBuilder(() => SalaryPeriodAdditionBillActivityLogMessage.Update)
            .MessageResourceFormatDatas(info.BillCode, typeInfo.Title)
            .ObjectId(info.SalaryPeriodAdditionBillId)
            .JsonData(model.JsonSerialize())
            .CreateLog();


            return true;
        }
    }
}
