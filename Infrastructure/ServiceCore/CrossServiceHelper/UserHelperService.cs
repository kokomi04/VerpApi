using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Commons.GlobalObject.Org;
using VErp.Grpc.Protos;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;

namespace VErp.Infrastructure.ServiceCore.CrossServiceHelper
{
    public interface IUserHelperService
    {
        Task<IList<UserInfoOutput>> GetByIds(IList<int> userIds);

    }


    public class UserHelperService : IUserHelperService
    {
        private readonly IHttpCrossService _httpCrossService;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;

        public UserHelperService(IHttpCrossService httpCrossService, IOptions<AppSetting> appSetting, ILogger<ProductHelperService> logger)
        {
            _httpCrossService = httpCrossService;
            _appSetting = appSetting.Value;
            _logger = logger;
        }

        public async Task<IList<UserInfoOutput>> GetByIds(IList<int> userIds)
        {
            return await _httpCrossService.Post<List<UserInfoOutput>>($"api/internal/InternalUser/GetByIds", userIds);
        }


    }
}
