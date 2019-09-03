using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.MasterDB
{
    public partial class Employee
    {
        public int UserId { get; set; }
        public string EmployeeCode { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public int Gender { get; set; }
    }
}
