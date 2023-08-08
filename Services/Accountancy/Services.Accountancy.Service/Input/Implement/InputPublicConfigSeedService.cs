using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Verp.Cache.RedisCache;
using Verp.Resources.Accountancy.InputConfig;
using Verp.Resources.Master.Config.ActionButton;
using VErp.Commons.Constants;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface.DynamicBill;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.AccountancyDB;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Accountancy.Model.Input;
using static NPOI.HSSF.UserModel.HeaderFooter;

namespace VErp.Services.Accountancy.Service.Input.Implement
{
   public interface IInputPublicConfigSeedService
    {
        Task ReplacePublicRefTableCode();
    }
    public class InputPublicConfigSeedService : IInputPublicConfigSeedService
    {
        private readonly UnAuthorizeAccountancyDBPublicContext _unAuthorizeAccountancyDBPublicContext;

        public InputPublicConfigSeedService(UnAuthorizeAccountancyDBPublicContext unAuthorizeAccountancyDBPublicContext)
        {
            _unAuthorizeAccountancyDBPublicContext = unAuthorizeAccountancyDBPublicContext;
        }

        public async Task ReplacePublicRefTableCode()
        {
            var fields = await _unAuthorizeAccountancyDBPublicContext.InputField.Where(f => f.RefTableCode == "_Input_Row").ToListAsync();
            foreach (var f in fields)
            {
                f.RefTableCode = "_InputPublic_Row";
            }
            var types = await _unAuthorizeAccountancyDBPublicContext.InputType.ToListAsync();
            foreach (var t in types)
            {
                t.CalcResultAllowcationSqlQuery = t.CalcResultAllowcationSqlQuery?.Replace("_Input_Row", "_InputPublic_Row");
            }

            await _unAuthorizeAccountancyDBPublicContext.SaveChangesAsync();
        }
    }
}

