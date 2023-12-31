﻿using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.MasterDB;

public partial class Role
{
    public int RoleId { get; set; }

    public int SubsidiaryId { get; set; }

    public string RoleName { get; set; }

    public string Description { get; set; }

    public int RoleStatusId { get; set; }

    public int RoleTypeId { get; set; }

    public bool IsDeleted { get; set; }

    public bool IsEditable { get; set; }

    public DateTime CreatedDatetimeUtc { get; set; }

    public DateTime UpdatedDatetimeUtc { get; set; }

    public int? ParentRoleId { get; set; }

    public string RootPath { get; set; }

    public bool IsModulePermissionInherit { get; set; }

    public bool IsDataPermissionInheritOnStock { get; set; }

    public string ChildrenRoleIds { get; set; }

    public virtual ICollection<Role> InverseParentRole { get; set; } = new List<Role>();

    public virtual Role ParentRole { get; set; }

    public virtual ICollection<RoleDataPermission> RoleDataPermission { get; set; } = new List<RoleDataPermission>();

    public virtual ICollection<RolePermission> RolePermission { get; set; } = new List<RolePermission>();
}
