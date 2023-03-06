using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Services.Organization.Model.Salary;

namespace VErp.Services.Organization.Service.Salary
{
    public interface ISalaryPeriodGroupService
    {
        Task<bool> Censor(long salaryPeriodGroupId, bool isSuccess);

        Task<bool> Check(long salaryPeriodGroupId, bool isSuccess);

        Task<IList<SalaryPeriodGroupModel>> GetList(int salaryPeriodId);

        Task<SalaryPeriodGroupModel> GetInfo(long salaryPeriodGroupId);

        Task<SalaryPeriodGroupModel> GetInfo(int salaryPeriodId, int salaryGroupId);

        Task<int> Create(SalaryPeriodGroupModel model);

        Task<bool> Delete(long salaryPeriodGroupId);

        Task<bool> Update(long salaryPeriodGroupId, SalaryPeriodGroupModel model);
    }
}
