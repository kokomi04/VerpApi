using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.OrganizationDB
{
    public partial class Department
    {
        public Department()
        {
            Childs = new HashSet<Department>();
            UserDepartmentMapping = new HashSet<EmployeeDepartmentMapping>();
        }

        public int DepartmentId { get; set; }
        public string DepartmentCode { get; set; }
        public string DepartmentName { get; set; }
        public string Description { get; set; }
        public int? ParentId { get; set; }
        public bool IsActived { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime UpdatedTime { get; set; }
        public int UpdatedUserId { get; set; }

        public virtual Department Parent { get; set; }
        public virtual ICollection<Department> Childs { get; set; }

        public virtual ICollection<EmployeeDepartmentMapping> UserDepartmentMapping { get; set; }
    }
}
