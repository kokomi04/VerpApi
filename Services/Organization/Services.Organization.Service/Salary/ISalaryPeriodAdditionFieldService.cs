using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Infrastructure.EF.OrganizationDB;
using VErp.Services.Organization.Model.Salary;

namespace VErp.Services.Organization.Service.Salary
{
    public interface ISalaryPeriodAdditionTypeService
    {     

        Task<IEnumerable<SalaryPeriodAdditionTypeInfo>> List();
        Task<SalaryPeriodAdditionTypeInfo> GetInfo(int salaryPeriodAdditionTypeId);
        Task<SalaryPeriodAdditionType> GetFullEntityInfo(int salaryPeriodAdditionTypeId);
        Task<int> Create(SalaryPeriodAdditionTypeModel model);
        Task<bool> Update(int salaryPeriodAdditionTypeId, SalaryPeriodAdditionTypeModel model);
        Task<bool> Delete(int salaryPeriodAdditionTypeId);
    }
}
