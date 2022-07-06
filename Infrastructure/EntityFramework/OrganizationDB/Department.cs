﻿using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.OrganizationDB
{
    public partial class Department
    {
        public Department()
        {
            DepartmentCapacityBalance = new HashSet<DepartmentCapacityBalance>();
            EmployeeDepartmentMapping = new HashSet<EmployeeDepartmentMapping>();
            InverseParent = new HashSet<Department>();
        }

        public int DepartmentId { get; set; }
        public int SubsidiaryId { get; set; }
        public string DepartmentCode { get; set; }
        public string DepartmentName { get; set; }
        public string Description { get; set; }
        public int? ParentId { get; set; }
        public bool IsActived { get; set; }
        public bool IsDeleted { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public bool IsProduction { get; set; }
        public long? ImageFileId { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }
        public int NumberOfPerson { get; set; }
        public int? WorkingHoursPerDay { get; set; }
        public int NumberOfMachine { get; set; }

        public virtual Department Parent { get; set; }
        public virtual ICollection<DepartmentCapacityBalance> DepartmentCapacityBalance { get; set; }
        public virtual ICollection<EmployeeDepartmentMapping> EmployeeDepartmentMapping { get; set; }
        public virtual ICollection<Department> InverseParent { get; set; }
    }
}
