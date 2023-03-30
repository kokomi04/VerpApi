using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Services.Organization.Model.Salary;

namespace VErp.Services.Organization.Service.Salary
{
    public interface ISalaryPeriodAdditionTypeService
    {     

        Task<IList<SalaryPeriodAdditionTypeInfo>> List();
        Task<int> Create(SalaryPeriodAdditionTypeModel model);
        Task<bool> Update(int salaryPeriodAdditionTypeId, SalaryPeriodAdditionTypeModel model);
        Task<bool> Delete(int salaryPeriodAdditionTypeId);
    }
}
