using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Organization.Model.Salary;

namespace VErp.Services.Organization.Service.Salary
{
    public interface ISalaryPeriodAdditionBillService
    {
        Task<PageData<SalaryPeriodAdditionBillList>> GetList(int salaryPeriodAdditionTypeId, int? year, int? month, int page, int size);
        Task<SalaryPeriodAdditionBillInfo> GetInfo(int salaryPeriodAdditionTypeId, long salaryPeriodAdditionBillId);
        Task<long> Create(int salaryPeriodAdditionTypeId, SalaryPeriodAdditionBillModel model);
        Task<bool> Update(int salaryPeriodAdditionTypeId, long salaryPeriodAdditionBillId, SalaryPeriodAdditionBillModel model);
        Task<bool> Delete(int salaryPeriodAdditionTypeId, long salaryPeriodAdditionBillId);
    }
}
