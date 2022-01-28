using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.MasterEnum;

namespace VErp.Commons.Constants.Caching
{
    public static class AuthorizeCacheKeys
    {
        public static string AUTH_TAG = "AUTH_TAG";
        public static string UserInfoCacheKey(int userId)
        {
            return $"AUTH_USERINFO_{userId}";
        }

        public static string RoleInfoCacheKey(int roleId)
        {
            return $"AUTH_ROLEINFO_{roleId}";
        }

        public static string RoleStockPermissionCacheKey(string userName, int subsidiaryId)
        {
            return $"AUTH_USER_DEVELOPER_{userName}_{subsidiaryId}";
        }

        public static string ApiEndpointsCacheKey()
        {
            return $"AUTH_API_ENDPOINTS";
        }

        public static string ModuleApiEndpointMappingsCacheKey(int moduleId)
        {
            return $"AUTH_MODULE_APIS_{moduleId}";
        }

        public static string ActionButtonsCacheKey()
        {
            return $"AUTH_ACTION_BUTTONS";
        }

        public static string RoleModulePermissionCacheKey(int roleId, int moduleId)
        {
            return $"AUTH_ROLE_MODULE_PERMISSION_{roleId}_{moduleId}";
        }

        public static string RoleObjectPermissionCacheKey(int roleId, EnumObjectType objectTypeId, long objectId)
        {
            return $"AUTH_ROLE_OBJECT_PERMISSION_{roleId}_{objectTypeId}_{objectId}";
        }

        public static string RoleStockPermissionCacheKey(int roleId)
        {
            return $"AUTH_ROLE_STOCK_PERMISSION_{roleId}";
        }

    }
}
