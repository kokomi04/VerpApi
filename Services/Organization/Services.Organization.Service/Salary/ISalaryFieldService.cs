using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Services.Organization.Model.Salary;

namespace VErp.Services.Organization.Service.Salary
{
    public interface ISalaryFieldService
    {
        Task<IList<SalaryFieldModel>> GetList();

        Task<int> Create(SalaryFieldModel model);

        Task<bool> Update(int salaryFieldId, SalaryFieldModel model);

        Task<bool> Delete(int salaryFieldId);
    }
}
