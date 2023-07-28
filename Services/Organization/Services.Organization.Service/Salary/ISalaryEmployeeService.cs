using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Constants;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.EF.OrganizationDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Organization.Model.Employee;
using VErp.Services.Organization.Model.Salary;
using VErp.Services.Organization.Service.Salary.Implement;

namespace VErp.Services.Organization.Service.Salary
{
    public interface ISalaryEmployeeService
    {
        Task<GroupSalaryEmployeeWarningInfo> GetSalaryGroupEmployeesWarning();
        Task<IList<NonCamelCaseDictionary<SalaryEmployeeValueModel>>> EvalSalaryEmployeeByGroup(int salaryPeriodId, int salaryGroupId, GroupSalaryEmployeeModel req);

        Task<IList<NonCamelCaseDictionary<SalaryEmployeeValueModel>>> GetSalaryEmployeeByGroup(int salaryPeriodId, int salaryGroupId);

        Task<IList<NonCamelCaseDictionary<SalaryEmployeeValueModel>>> GetInfoEmployeeByGroupSalary(int salaryGroupId);

        Task<IList<GroupSalaryEmployeeEvalData>> GetSalaryEmployeeAll(int salaryPeriodId);

        Task<bool> Update(int salaryPeriodId, int salaryGroupId, GroupSalaryEmployeeModel model);

        Task<PageData<NonCamelCaseDictionary>> GetEmployeeGroupInfo(Clause filter, int page, int size);
        Task<(Stream stream, string fileName, string contentType)> Export(IList<string> fieldNames, IList<string> groupField,int salaryPeriodId, int salaryGroupId, IList<NonCamelCaseDictionary<SalaryEmployeeValueModel>> data);

        Task<CategoryNameModel> GetFieldDataForMapping();
    }
}
