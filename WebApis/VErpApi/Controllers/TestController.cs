using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject.QueueMessage;
using VErp.Commons.Library;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper.General;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Master.Model.Dictionary;
using VErp.Services.Master.Service.Activity;
using static VErp.Commons.GlobalObject.QueueName.ManufacturingQueueNameConstants;

namespace VErpApi.Controllers.System
{
    [Route("api/[controller]")]
    [GlobalApi]
    public class TestController : VErpBaseController
    {
        private readonly MasterDBContext _masterDBContext;
        private readonly AppSetting _appSetting;
        private readonly IUserLogActionService _userLogActionService;
        private readonly IAsyncRunnerService _asyncRunnerService;
        private readonly IQueueProcessHelperService _queueProcessHelperService;

        public TestController(
            MasterDBContext masterDBContext
            , IOptions<AppSetting> appSetting
            , IUserLogActionService userLogActionService
            , IAsyncRunnerService asyncRunnerService
            , IQueueProcessHelperService queueProcessHelperService)
        {
            _masterDBContext = masterDBContext;
            _appSetting = appSetting?.Value;
            _userLogActionService = userLogActionService;
            _asyncRunnerService = asyncRunnerService;
            _queueProcessHelperService = queueProcessHelperService;
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
            return JsonUtils.GetJsonDiff(Newtonsoft.Json.JsonConvert.SerializeObject(oldUnit), newUnit);
        }

        [HttpPost]
        public async Task<int> RunAbc(int a)
        {
            Console.WriteLine(a);
            var u = await _masterDBContext.User.FirstOrDefaultAsync();
            return u.UserId;
        }
        [HttpGet]
        [Route("TestAsync")]
        public async Task<int> TestAsync()
        {
            await RunAbc(1);

            _asyncRunnerService.RunAsync<TestController>(c => c.RunAbc(1));

            return 0;
        }


        [HttpPost]
        [Route("EnqueueProductionOrderStatus")]
        public async Task<int> EnqueueProductionOrderStatus([FromBody] string productionOrderCode, string inventoryCode, EnumInventoryType inventoryTypeId)
        {

            await _queueProcessHelperService.EnqueueAsync(PRODUCTION_INVENTORY_STATITICS, new ProductionOrderStatusInventorySumaryMessage()
            {
                Description = "test",
                ProductionOrderCode = productionOrderCode,
            });
            return 0;
        }

    }
}
