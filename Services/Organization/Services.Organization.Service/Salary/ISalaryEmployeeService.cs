using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Constants;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.OrganizationDB;
using VErp.Services.Organization.Model.Salary;
using VErp.Services.Organization.Service.Salary.Implement;

namespace VErp.Services.Organization.Service.Salary
{
    public interface ISalaryEmployeeService
    {
        Task<IList<NonCamelCaseDictionary>> EvalSalaryEmployeeByGroup(int salaryPeriodId, int salaryGroupId, GroupSalaryEmployeeRequestModel req);

        Task<IList<NonCamelCaseDictionary>> GetSalaryEmployeeByGroup(int salaryPeriodId, int salaryGroupId);

        Task<bool> Update(int salaryPeriodId, int salaryGroupId, GroupSalaryEmployeeModel model);    

    }
}
