using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Commons.GlobalObject.Org;

namespace VErp.Services.Master.Model.Users
{
   
    public class UserInfoInput : EmployeeBase, IMapFrom<UserImportModel>
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public EnumUserStatus UserStatusId { get; set; }
        public int? RoleId { get; set; }

        public IList<UserDepartmentMappingModel> Departments { get; set; }
    }



    public class UserChangepasswordInput
    {
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }
}
