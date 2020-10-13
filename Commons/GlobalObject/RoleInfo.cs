using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Commons.GlobalObject
{
    public class RoleInfo
    {
        public RoleInfo(
            int roleId,
            IList<int> childrenRoleIds,
            bool isModulePermissionInherit,
            bool isDataPermissionInheritOnStock,
            string roleName
            )
        {
            RoleId = roleId;
            ChildrenRoleIds = childrenRoleIds;
            IsModulePermissionInherit = isModulePermissionInherit;
            IsDataPermissionInheritOnStock = isDataPermissionInheritOnStock;
            RoleName = roleName;
        }

        public int RoleId { get; set; }
        public bool IsModulePermissionInherit { get; }
        public bool IsDataPermissionInheritOnStock { get; }
        public IList<int> ChildrenRoleIds { get; }
        public IList<int> RoleIds {
            get
            {
                var roleIds = new List<int>();

                if (ChildrenRoleIds != null)
                {
                    roleIds.AddRange(ChildrenRoleIds);
                }

                roleIds.Add(RoleId);

                return roleIds;
            }
        }
        public string RoleName { get; set; }
    }
}
