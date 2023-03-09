using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            var info = _mapper.Map<SalaryField>(model);
            await _organizationDBContext.SalaryField.AddAsync(info);
            await _organizationDBContext.SaveChangesAsync();
            await _salaryFieldActivityLog.CreateLog(info.SalaryFieldId, $"Thêm mới trường dữ liệu {info.SalaryFieldName} vào bảng lương", model.JsonSerialize());
            return info.SalaryFieldId;
        }

        public async Task<bool> Delete(int salaryFieldId)
        {
            var info = await _organizationDBContext.SalaryField.FirstOrDefaultAsync(s => s.SalaryFieldId == salaryFieldId);
            if (info == null)
            {
                throw GeneralCode.ItemNotFound.BadRequest();
            }
            if (await _organizationDBContext.SalaryEmployeeValue.AnyAsync(v => v.SalaryFieldId == salaryFieldId))
            {
                throw SalaryFieldValidationMessage.SalaryFieldInUsed.BadRequestFormat(info.Title);
            }

            _organizationDBContext.SalaryField.Remove(info);
            await _organizationDBContext.SaveChangesAsync();
            await _salaryFieldActivityLog.CreateLog(info.SalaryFieldId, $"Xóa trường dữ liệu {info.SalaryFieldName} khỏi bảng lương", info.JsonSerialize());
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
            var info = await _organizationDBContext.SalaryField.FirstOrDefaultAsync(s => s.SalaryFieldId == salaryFieldId);
            if (info == null)
            {
                throw GeneralCode.ItemNotFound.BadRequest();
            }

            if ((int)model.DataTypeId != info.DataTypeId)
            {
                throw SalaryFieldValidationMessage.CannotChangeDataTypeOfSalaryField.BadRequestFormat(info.Title);
            }

            _mapper.Map(model, info);
            info.SalaryFieldId = salaryFieldId;
            await _organizationDBContext.SaveChangesAsync();
            await _salaryFieldActivityLog.CreateLog(info.SalaryFieldId, $"Cập nhật trường dữ liệu {model.SalaryFieldName} bảng lương", model.JsonSerialize());
            return true;
        }
    }
}
