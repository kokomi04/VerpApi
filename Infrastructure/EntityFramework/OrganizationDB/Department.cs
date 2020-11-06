using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.OrganizationDB
{
    public partial class Department
    {
        public Department()
        {
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
        public int UpdatedUserId { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime UpdatedTime { get; set; }

        public virtual Department Parent { get; set; }
        public virtual ICollection<EmployeeDepartmentMapping> EmployeeDepartmentMapping { get; set; }
        public virtual ICollection<Department> InverseParent { get; set; }
    }
}
