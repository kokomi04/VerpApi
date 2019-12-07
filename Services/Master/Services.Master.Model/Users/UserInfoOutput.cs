using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.MasterEnum;

namespace VErp.Services.Master.Model.Users
{  
    public class UserInfoOutput : EmployeeBase
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public EnumUserStatus UserStatusId { get; set; }
        public int? RoleId { get; set; }
    }
    
}
