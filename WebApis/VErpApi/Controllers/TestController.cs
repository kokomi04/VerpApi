using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Library;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.MasterDB;
using VErpApi.Controllers;

namespace VErp.WebApis.VErpApi.Controllers
{
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class TestController : VErpBaseController
    {
        private readonly MasterDBContext _masterDBContext;
        private readonly AppSetting _appSetting;
        public TestController(MasterDBContext masterDBContext, IOptions<AppSetting> appSetting)
        {
            _masterDBContext = masterDBContext;
            _appSetting = appSetting.Value;
        }
        [HttpPost]
        [Route("CreateUser")]
        public async Task<bool> Post([FromQuery] string userName, [FromQuery] string password)
        {
            var (salt, passwordHash) = Sercurity.GenerateHashPasswordHash(_appSetting.PasswordPepper, password);
            var user = new User()
            {
                UserName = userName,
                UserNameHash = userName.ToGuid(),
                IsDeleted = false,
                CreatedDatetimeUtc = DateTime.UtcNow,
                UserStatusId = (int)EnumUserStatus.Actived,
                PasswordSalt = salt,
                PasswordHash = passwordHash
            };
            _masterDBContext.User.Add(user);
            await _masterDBContext.SaveChangesAsync();
            return true;
        }
    }
}
