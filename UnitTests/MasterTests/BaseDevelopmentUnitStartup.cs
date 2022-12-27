﻿using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.AccountancyDB;
using VErp.Infrastructure.EF.ManufacturingDB;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Infrastructure.EF.StockDB;
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
        protected AccountancyDBPrivateContext _accountancyDBContext;
        protected ManufacturingDBContext _manufacturingDBContext;


        public int UserId { get; set; }
        public BaseDevelopmentUnitStartup(int userId, int subsidiaryId)
        {
            Environment.SetEnvironmentVariable("DEBUG_ENVIRONMENT", "UNIT_TEST");
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");

            webHost = Program.CreateWebHostBuilder(null).Build();

            UserId = userId;

            _unAuthorizeMasterDBContext = (UnAuthorizeMasterDBContext)webHost.Services.GetService(typeof(UnAuthorizeMasterDBContext));

            var currentContextFactory = (ICurrentContextFactory)webHost.Services.GetService(typeof(ICurrentContextFactory));

            currentContextFactory.SetCurrentContext(new ScopeCurrentContextService(null, userId, EnumActionType.Add, RoleInfo, null, subsidiaryId, null, "", null, null));

            _masterDBContext = (MasterDBContext)webHost.Services.GetService(typeof(MasterDBContext));

            _stockDBContext = (StockDBContext)webHost.Services.GetService(typeof(StockDBContext));

            _accountancyDBContext = (AccountancyDBPrivateContext)webHost.Services.GetService(typeof(AccountancyDBPrivateContext));

            _manufacturingDBContext = (ManufacturingDBContext)webHost.Services.GetService(typeof(ManufacturingDBContext));

        }

        public BaseDevelopmentUnitStartup()
        {
            Environment.SetEnvironmentVariable("DEBUG_ENVIRONMENT", "UNIT_TEST");
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");

            webHost = Program.CreateWebHostBuilder(null).Build();

            UserId = 2;

            _unAuthorizeMasterDBContext = (UnAuthorizeMasterDBContext)webHost.Services.GetService(typeof(UnAuthorizeMasterDBContext));

            var currentContextFactory = (ICurrentContextFactory)webHost.Services.GetService(typeof(ICurrentContextFactory));

            currentContextFactory.SetCurrentContext(new ScopeCurrentContextService(null, UserId, EnumActionType.Add, RoleInfo, null, 2, null, "", null, null));

            _masterDBContext = (MasterDBContext)webHost.Services.GetService(typeof(MasterDBContext));

            _stockDBContext = (StockDBContext)webHost.Services.GetService(typeof(StockDBContext));

            _accountancyDBContext = (AccountancyDBPrivateContext)webHost.Services.GetService(typeof(AccountancyDBPrivateContext));


        }


        public RoleInfo RoleInfo
        {
            get
            {
                if (_roleInfo != null)
                {
                    return _roleInfo;
                }

                var userInfo = _unAuthorizeMasterDBContext.User.AsNoTracking().FirstOrDefault(u => u.UserId == UserId);

                if (userInfo == null)
                    throw new BadRequestException(GeneralCode.ItemNotFound);

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
                    roleInfo.IsModulePermissionInherit,
                    string.Empty
                );


                return _roleInfo;
            }
        }
    }
}
