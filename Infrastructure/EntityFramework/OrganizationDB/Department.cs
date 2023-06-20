using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.OrganizationDB;

public partial class Department
{
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

    public bool IsFactory { get; set; }

    public virtual ICollection<DepartmentCapacityBalance> DepartmentCapacityBalance { get; set; } = new List<DepartmentCapacityBalance>();

    public virtual ICollection<EmployeeDepartmentMapping> EmployeeDepartmentMapping { get; set; } = new List<EmployeeDepartmentMapping>();

    public virtual ICollection<Department> InverseParent { get; set; } = new List<Department>();

    public virtual Department Parent { get; set; }
}
