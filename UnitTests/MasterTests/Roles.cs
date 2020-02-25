using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
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

        // Return a DbSet of the specified generic type with support for async operations
        public static Mock<DbSet<T>> GetDbSet<T>(IQueryable<T> TestData) where T : class
        {
            var MockSet = new Mock<DbSet<T>>();
            MockSet.As<IAsyncEnumerable<T>>().Setup(x => x.GetEnumerator()).Returns(new TestAsyncEnumerator<T>(TestData.GetEnumerator()));
            MockSet.As<IQueryable<T>>().Setup(x => x.Provider).Returns(new TestAsyncQueryProvider<T>(TestData.Provider));
            MockSet.As<IQueryable<T>>().Setup(x => x.Expression).Returns(TestData.Expression);
            MockSet.As<IQueryable<T>>().Setup(x => x.ElementType).Returns(TestData.ElementType);
            MockSet.As<IQueryable<T>>().Setup(x => x.GetEnumerator()).Returns(TestData.GetEnumerator());
            return MockSet;
        }

        // Test data for the DbSet<User> getter
        public static IQueryable<User> Users
        {
            get
            {
                return new List<User>
                {
                    new User { UserId = 1, UserName = "admin"},
                }
                .AsQueryable();
            }
        }

        public static IQueryable<Employee> Employees
        {
            get
            {
                return new List<Employee>
                {
                    new Employee { UserId = 1, FullName="admin name"},
                }
                .AsQueryable();
            }
        }

        [Fact]
        public void GetInfo()
        {
            // Create a mock version of the DbContext
            var DbContext = new Mock<MasterDBContext>();

            // Users getter will return our mock DbSet with test data
            // (Here is where we call our helper function)
            DbContext.SetupGet(x => x.User).Returns(GetDbSet<User>(Users).Object);
            DbContext.SetupGet(x => x.Employee).Returns(GetDbSet<Employee>(Employees).Object);

            var setting = new Mock<IOptions<AppSetting>>();
            setting.Setup(s => s.Value).Returns(AppConfigSetting.Config().AppSetting);

            var logger = new Mock<ILogger<UserService>>();
            var roleService = new Mock<IRoleService>();
            var activityLogService = new Mock<IActivityLogService>();
            //var setting = new Mock<IOptions<AppSetting>>();//AppConfigSetting

            IUserService user = new UserService(DbContext.Object, setting.Object, logger.Object, roleService.Object, activityLogService.Object, null);
            // var paymentServiceMock = new Mock<IUserService>();

            //paymentServiceMock.Verify(p => p.GetInfo(1),);
            var a = user.GetInfo(1).GetAwaiter().GetResult();

            // Call the function to test
            //var UserHandler = new MockProject.Common.UserHandler(DbContext.Object);
            //var Result = await UserHandler.GetUserIDByEmail("admin@host.com");

            // Verify the results
            Assert.Equal(GeneralCode.Success, a.Code);
            Assert.Equal(1, a.Data.UserId);
        }

        [Fact]
        public void Create()
        {
            var DbContext = new Mock<MasterDBContext>();

            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Local");

            var setting = new Mock<IOptions<AppSetting>>();
            var basePath = Assembly.GetExecutingAssembly().CodeBase;
            basePath = basePath.Substring(0, basePath.LastIndexOf('/'));

            setting.Setup(s => s.Value).Returns(AppConfigSetting.Config(basePath: basePath).AppSetting);

            var logger = new Mock<ILogger<UserService>>();
            var roleService = new Mock<IRoleService>();
            var activityLogService = new Mock<IActivityLogService>();

            IUserService user = new UserService(DbContext.Object, setting.Object, logger.Object, roleService.Object, activityLogService.Object, null);

            var b = user.CreateUser(new UserInfoInput()
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
            }).GetAwaiter().GetResult();

            Assert.Equal(1, 1);
        }
    }
}
