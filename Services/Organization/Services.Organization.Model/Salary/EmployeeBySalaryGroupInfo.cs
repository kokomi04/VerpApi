using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;

namespace VErp.Services.Organization.Model.Salary
{
    public class EmployeeSalaryGroupInfo
    {
        public NonCamelCaseDictionary EmployeeInfo { get; set; }
        public IList<int> SalaryGroupIds { get; set; }
    }

    public class GroupSalaryEmployeeWarningInfo
    {
        public IList<NonCamelCaseDictionary> NoSalaryGroupEmployees { get; set; }
        public IList<EmployeeSalaryGroupInfo> DuplicatedSalayGroupEmployees { get; set; }
    }
}
