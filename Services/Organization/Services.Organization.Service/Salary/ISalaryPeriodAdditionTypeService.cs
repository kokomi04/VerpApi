using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Services.Organization.Model.Salary;

namespace VErp.Services.Organization.Service.Salary
{
    public interface ISalaryPeriodAdditionFieldService
    {
        Task<IList<SalaryPeriodAdditionFieldInfo>> List();
        Task<int> Create(SalaryPeriodAdditionFieldModel model);
        Task<bool> Update(int salaryPeriodAdditionFieldId, SalaryPeriodAdditionFieldModel model);
        Task<bool> Delete(int salaryPeriodAdditionFieldId);      
    }
}
