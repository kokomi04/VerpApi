using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.MasterEnum;

namespace VErp.Commons.GlobalObject.Org
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
        public bool? IsDeveloper { get; set; }
    }
}
