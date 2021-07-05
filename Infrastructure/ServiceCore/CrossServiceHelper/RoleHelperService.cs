using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Grpc.Protos;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;

namespace VErp.Infrastructure.ServiceCore.CrossServiceHelper
{
    public interface IRoleHelperService
    {
        Task<bool> GrantDataForAllRoles(EnumObjectType objectTypeId, long objectId);

        Task<bool> GrantPermissionForAllRoles(EnumModule moduleId, EnumObjectType objectTypeId, long objectId);

    }


    public class RoleHelperService : IRoleHelperService
    {
        private readonly IHttpCrossService _httpCrossService;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;

        public RoleHelperService(IHttpCrossService httpCrossService, IOptions<AppSetting> appSetting, ILogger<ProductHelperService> logger)
        {
            _httpCrossService = httpCrossService;
            _appSetting = appSetting.Value;
            _logger = logger;
        }
        public async Task<bool> GrantDataForAllRoles(EnumObjectType objectTypeId, long objectId)
        {
            return await _httpCrossService.Post<bool>($"api/internal/InternalRole/GrantDataForAllRoles", new { objectTypeId, objectId });
        }

        public async Task<bool> GrantPermissionForAllRoles(EnumModule moduleId, EnumObjectType objectTypeId, long objectId)
        {
            return await _httpCrossService.Post<bool>($"api/internal/InternalRole/GrantPermissionForAllRoles", new { moduleId, objectTypeId, objectId });
        }


    }
}
