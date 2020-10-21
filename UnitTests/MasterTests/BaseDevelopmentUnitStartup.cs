using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.AccountancyDB;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Infrastructure.EF.StockDB;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.WebApis.VErpApi;

namespace MasterTests
{
    public abstract class BaseDevelopmentUnitStartup
    {
        protected readonly IWebHost webHost;

        //private IList<int> _stockIds;
        //private IList<int> _roleIds;
        private RoleInfo _roleInfo;

        protected MasterDBContext _masterDBContext;
        protected UnAuthorizeMasterDBContext _unAuthorizeMasterDBContext;
        protected StockDBContext _stockDBContext;
        protected AccountancyDBContext _accountancyDBContext;


        public int UserId { get; set; }
        public BaseDevelopmentUnitStartup()
        {
            Environment.SetEnvironmentVariable("DEBUG_ENVIRONMENT", "UNIT_TEST");
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");

            webHost = Program.CreateWebHostBuilder(null).Build();

            UserId = 2;

            _unAuthorizeMasterDBContext= (UnAuthorizeMasterDBContext)webHost.Services.GetService(typeof(UnAuthorizeMasterDBContext));

            var currentContextFactory = (ICurrentContextFactory)webHost.Services.GetService(typeof(ICurrentContextFactory));

            currentContextFactory.SetCurrentContext(new ScopeCurrentContextService(1, EnumAction.Add, RoleInfo, null, 0, null));

            _masterDBContext = (MasterDBContext)webHost.Services.GetService(typeof(MasterDBContext));

            _stockDBContext = (StockDBContext)webHost.Services.GetService(typeof(StockDBContext));

            _accountancyDBContext = (AccountancyDBContext)webHost.Services.GetService(typeof(AccountancyDBContext));


        }


        public RoleInfo RoleInfo
        {
            get
            {
                if (_roleInfo != null)
                {
                    return _roleInfo;
                }

                var userInfo = _unAuthorizeMasterDBContext.User.AsNoTracking().First(u => u.UserId == UserId);
                var roleInfo = (
                    from r in _unAuthorizeMasterDBContext.Role
                    where r.RoleId == userInfo.RoleId
                    select new
                    {
                        r.RoleId,
                        r.IsDataPermissionInheritOnStock,
                        r.IsModulePermissionInherit,
                        r.ChildrenRoleIds
                    }
                   )
                   .First();

                _roleInfo = new RoleInfo(
                    roleInfo.RoleId,
                    roleInfo.ChildrenRoleIds?.Split(',')?.Where(c => !string.IsNullOrWhiteSpace(c)).Select(c => int.Parse(c)).ToList(),
                    roleInfo.IsDataPermissionInheritOnStock,
                    roleInfo.IsModulePermissionInherit
                );


                return _roleInfo;
            }
        }
    }
}
