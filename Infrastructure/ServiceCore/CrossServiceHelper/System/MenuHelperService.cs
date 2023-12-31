﻿using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper.Product;
using VErp.Infrastructure.ServiceCore.Service;

namespace VErp.Infrastructure.ServiceCore.CrossServiceHelper.System
{
    public interface IMenuHelperService
    {
        Task<bool> CreateMenu(int? parentId, bool isGroup, int moduleId, string moduleName, string url, string param, string icon, int sortOrder, bool isDisabled);
    }

    public class MenuHelperService : IMenuHelperService
    {
        private readonly IHttpCrossService _httpCrossService;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        public MenuHelperService(IHttpCrossService httpCrossService, IOptions<AppSetting> appSetting, ILogger<ProductHelperService> logger)
        {
            _httpCrossService = httpCrossService;
            _appSetting = appSetting.Value;
            _logger = logger;
        }

        public async Task<bool> CreateMenu(int? parentId, bool isGroup, int moduleId, string menuName, string url, string param, string icon, int sortOrder, bool isDisabled)
        {
            return await _httpCrossService.Post<bool>("api/internal/InternalMenu", new
            {
                parentId,
                isGroup,
                moduleId,
                menuName,
                url,
                param,
                icon,
                sortOrder,
                isDisabled
            });
        }
    }
}
