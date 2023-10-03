using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Commons.GlobalObject.Org;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper.Product;
using VErp.Infrastructure.ServiceCore.Service;

namespace VErp.Infrastructure.ServiceCore.CrossServiceHelper.Hr
{
    public interface IUserHelperService
    {
        Task<IList<EmployeeBasicNameModel>> GetAll();
        Task<IList<UserInfoOutput>> GetByIds(IList<int> userIds);
        Task<IList<UserInfoOutput>> GetListByRoles(IList<int> roles);

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

        public async Task<IList<EmployeeBasicNameModel>> GetAll()
        {
            return await _httpCrossService.Get<List<EmployeeBasicNameModel>>($"api/internal/InternalUser/GetAll");
        }

        public async Task<IList<UserInfoOutput>> GetByIds(IList<int> userIds)
        {
            return await _httpCrossService.Post<List<UserInfoOutput>>($"api/internal/InternalUser/GetByIds", userIds);
        }

        public async Task<IList<UserInfoOutput>> GetListByRoles(IList<int> roles)
        {
            return await _httpCrossService.Post<List<UserInfoOutput>>($"api/internal/InternalUser/GetListByRoles", roles);
        }


    }
}
