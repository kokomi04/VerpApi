﻿using System;
using System.Collections.Generic;

#nullable disable

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
        public int SubsidiaryId { get; set; }
        public string EmployeeCode { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public int? GenderId { get; set; }
        public bool IsDeleted { get; set; }
        public long? AvatarFileId { get; set; }
        public int EmployeeTypeId { get; set; }
        public int UserStatusId { get; set; }
        public string PartnerId { get; set; }

        public virtual ICollection<EmployeeDepartmentMapping> EmployeeDepartmentMapping { get; set; }
        public virtual ICollection<EmployeeSubsidiary> EmployeeSubsidiary { get; set; }
    }
}
