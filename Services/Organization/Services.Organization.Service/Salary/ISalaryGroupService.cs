using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Services.Organization.Model.Salary;

namespace VErp.Services.Organization.Service.Salary
{
    public interface ISalaryGroupService
    {
        Task<IList<SalaryGroupInfo>> GetList();

        Task<SalaryGroupInfo> GetInfo(int salaryGroupId);

        Task<int> Create(SalaryGroupModel model);

        Task<bool> Update(int salaryGroupId, SalaryGroupModel model); 

        Task<bool> Delete(int salaryGroupId);
    }
}
