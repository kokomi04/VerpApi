using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Organization.Model.Salary;

namespace VErp.Services.Organization.Service.Salary
{
    public interface ISalaryPeriodService
    {
        Task<PageData<SalaryPeriodModel>> GetList(int page, int size);

        Task<int> Create(SalaryPeriodModel model);

        Task<bool> Update(int salaryPeriodId, SalaryPeriodModel model);

        Task<bool> Delete(int salaryPeriodId);

        Task<bool> Check(int salaryPeriodId, bool isSuccess);
        Task<bool> Censor(int salaryPeriodId, bool isSuccess);
    }
}
