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
using VErp.Services.Master.Model.Dictionary;
using VErp.Services.Master.Service.Activity.Implement;
using VErpApi.Controllers;
using VErp.Services.Master.Service.Activity;
using Microsoft.EntityFrameworkCore;
using VErp.Infrastructure.ServiceCore.Service;

namespace VErp.WebApis.VErpApi.Controllers
{
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class TestController : VErpBaseController
    {
        private readonly MasterDBContext _masterDBContext;
        private readonly AppSetting _appSetting;
        private readonly IActivityService _activityService;
        private readonly IAsyncRunnerService _asyncRunnerService;
        public TestController(
            MasterDBContext masterDBContext
            , IOptions<AppSetting> appSetting
            , IActivityService activityService
            , IAsyncRunnerService asyncRunnerService
            )
        {
            _masterDBContext = masterDBContext;
            _appSetting = appSetting.Value;
            _activityService = activityService;
            _asyncRunnerService = asyncRunnerService;
        }
        [HttpPost]
        [Route("CreateUser")]
        public async Task<bool> Post([FromQuery] string userName, [FromQuery] string password, [FromQuery] int roleId)
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
                PasswordHash = passwordHash,
                RoleId = roleId
            };
            _masterDBContext.User.Add(user);
            await _masterDBContext.SaveChangesAsync();
            return true;
        }

        [HttpPost]
        [Route("TestDiff")]
        public async Task<string> TestChange([FromQuery] UnitOutput oldUnit, [FromBody] UnitOutput newUnit)
        {
            await Task.CompletedTask;
            return Utils.GetJsonDiff(Newtonsoft.Json.JsonConvert.SerializeObject(oldUnit), newUnit);
        }

        public async Task<int> RunAbc(int a)
        {
            var u = await _masterDBContext.User.FirstOrDefaultAsync();
            return u.UserId;
        }
        [HttpGet]
        [Route("TestAsync")]
        public async Task<int> TestAsync()
        {
            await RunAbc(1);

            _asyncRunnerService.RunAsync<TestController>(c=>c.RunAbc(1));

            return 0;
        }

    }
}
