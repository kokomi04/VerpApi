using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.OrganizationDB
{
    public partial class Employee
    {
        public Employee()
        {
            EmployeeDepartmentMapping = new HashSet<EmployeeDepartmentMapping>();
            EmployeeSubsidiary = new HashSet<EmployeeSubsidiary>();
        }

        public int UserId { get; set; }
        public string EmployeeCode { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public int? GenderId { get; set; }
        public bool IsDeleted { get; set; }
        public long? AvatarFileId { get; set; }

        public virtual ICollection<EmployeeDepartmentMapping> EmployeeDepartmentMapping { get; set; }
        public virtual ICollection<EmployeeSubsidiary> EmployeeSubsidiary { get; set; }
    }
}
