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

        public IList<UserDepartmentInfoModel> Departments { get; set; }
    }


    public class SubsidiaryBasicInfo
    {
        public int SubsidiaryId { get; set; }
        public string SubsidiaryCode { get; set; }
        public string SubsidiaryName { get; set; }
    }

    public class UserDepartmentInfoModel : UserDepartmentMappingModel
    {      
        public string DepartmentCode { get; set; }
        public string DepartmentName { get; set; }
    }

    public class UserDepartmentMappingModel
    {
        public int DepartmentId { get; set; }
        public int? UserDepartmentMappingId { get; set; }
        public long? EffectiveDate { get; set; }
        public long? ExpirationDate { get; set; }
    }
}
