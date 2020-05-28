using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.MasterEnum;

namespace VErp.Services.Master.Model.Users
{
    public class EmployeeBase
    {
        public string EmployeeCode { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public EnumGender? GenderId { get; set; }
        public long? AvatarFileId { get; set; }
    }
    public class UserInfoInput : EmployeeBase
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public EnumUserStatus UserStatusId { get; set; }
        public int? RoleId { get; set; }

        public int? DepartmentId { get; set; }
    }

    public class UserChangepasswordInput
    {
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }
}
