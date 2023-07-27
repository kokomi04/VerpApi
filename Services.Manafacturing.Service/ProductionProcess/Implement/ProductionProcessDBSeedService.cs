using AutoMapper;
using AutoMapper.QueryableExtensions;
using DocumentFormat.OpenXml.EMMA;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Bcpg.OpenPgp;
using Org.BouncyCastle.Ocsp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verp.Resources.Manafacturing.Production.Process;
using Verp.Resources.Master.Config.ActionButton;
using VErp.Commons.Enums.ErrorCodes;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface.Manufacturing;
using VErp.Commons.GlobalObject.InternalDataInterface.Stock;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.ManufacturingDB;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper.QueueHelper;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Manafacturing.Model.ProductionOrder;
using VErp.Services.Manafacturing.Model.ProductionProcess;
using VErp.Services.Manafacturing.Model.ProductionStep;
using VErp.Services.Manafacturing.Service.ProductionAssignment;
using VErp.Services.Manafacturing.Service.ProductionAssignment.Implement;
using static VErp.Commons.Enums.Manafacturing.EnumProductionProcess;
using ProductSemiEnity = VErp.Infrastructure.EF.ManufacturingDB.ProductSemi;

namespace VErp.Services.Manafacturing.Service.ProductionProcess.Implement
{
    public class ProductionProcessDBSeedService : IProductionProcessDBSeedService
    {
        private readonly UnAuthorizeManufacturingDBContext _unAuthorizeManufacturingDBContext;

        public ProductionProcessDBSeedService(UnAuthorizeManufacturingDBContext unAuthorizeManufacturingDBContext)
        {
            _unAuthorizeManufacturingDBContext = unAuthorizeManufacturingDBContext;
        }


        public async Task CreateStockProductionStep()
        {
            await _unAuthorizeManufacturingDBContext.ExecuteNoneQueryProcedure("asp_ProductionStep_CreateStockStep", Array.Empty<SqlParameter>());
        }

    }
}
