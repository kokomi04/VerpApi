using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Services.Organization.Model.Salary;

namespace VErp.Services.Organization.Service.Salary
{
    public interface ISalaryRefTableService
    {
        Task<IList<SalaryRefTableModel>> GetList();

        Task<int> Create(SalaryRefTableModel model);

        Task<bool> Update(int salaryRefTableId, SalaryRefTableModel model);

        Task<bool> Delete(int salaryRefTableId);

    }
}
