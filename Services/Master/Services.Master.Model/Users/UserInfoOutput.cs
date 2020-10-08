using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject.Org;
using VErp.Services.Organization.Model.Department;

namespace VErp.Services.Master.Model.Users
{  
    public class UserInfoOutput : EmployeeBase
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public EnumUserStatus UserStatusId { get; set; }
        public int? RoleId { get; set; }

        public DepartmentModel Department { get; set; }
    }
    

    public class SubsidiaryBasicInfo
    {
        public int SubsidiaryId { get; set; }
        public string SubsidiaryCode { get; set; }
        public string SubsidiaryName { get; set; }
    }
}
