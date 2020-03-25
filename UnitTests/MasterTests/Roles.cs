using EntityFrameworkCore3Mock;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Reflection;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.AppSettings;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Master.Model.Users;
using VErp.Services.Master.Service.RolePermission;
using VErp.Services.Master.Service.Users;
using VErp.Services.Master.Service.Users.Implement;
using Xunit;

namespace MasterTests
{
    public class Roles
    {

        [Fact]
        public void CreateUser()
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Local");

            var option = new DbContextOptionsBuilder<MasterDBContext>()
                                  .UseInMemoryDatabase(Guid.NewGuid().ToString())
                                  .Options;


            var f = new LoggerFactory();

            var inMem = SetupInMemoryDbContext<MasterDBContext>(f);

            var setting = new Mock<IOptions<AppSetting>>();
            var basePath = Assembly.GetExecutingAssembly().CodeBase;
            basePath = basePath.Substring(0, basePath.LastIndexOf('/'));

            setting.Setup(s => s.Value).Returns(AppConfigSetting.Config(basePath: basePath).AppSetting);

            var logger = new Mock<ILogger<UserService>>();
            var roleService = new Mock<IRoleService>();
            var activityLogService = new Mock<IActivityLogService>();
            var asyncRunnerService = new Mock<IAsyncRunnerService>();

            var currentContext = new ScopeCurrentContextService(1, EnumAction.Add, new RoleInfo(1, null, true, true), new List<int>());

            IUserService user = new UserService(inMem, setting.Object, logger.Object, roleService.Object, activityLogService.Object, currentContext, asyncRunnerService.Object);

            var result = user.CreateUser(new UserInfoInput()
            {
                UserName = "test01",
                Password = "123456",
                Address = "address",
                Email = "abc@gmail.com",
                EmployeeCode = "CODE1",
                FullName = "Fullname",
                GenderId = EnumGender.Male,
                Phone = "0000",
                RoleId = 1,
                UserStatusId = EnumUserStatus.Actived
            },0).GetAwaiter().GetResult();

            Assert.Equal(1, result.Data);
        }

        public static TContext SetupInMemoryDbContext<TContext>(ILoggerFactory loggerFactory)
            where TContext : DbContext
        {
            var customerContextOptions = new DbContextOptionsBuilder<TContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .UseLoggerFactory(loggerFactory)
                .ConfigureWarnings(a => a.Log(InMemoryEventId.TransactionIgnoredWarning))
                .Options;
            return (TContext)Activator.CreateInstance(typeof(TContext), customerContextOptions);
        }
    }
}
