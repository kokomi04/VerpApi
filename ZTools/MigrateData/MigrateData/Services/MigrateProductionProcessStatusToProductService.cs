using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VErp.Infrastructure.EF.AccountancyDB;
using VErp.Infrastructure.EF.ManufacturingDB;
using VErp.Infrastructure.EF.StockDB;
using VErp.Services.Accountancy.Service.Input;
using VErp.Infrastructure.EF.EFExtensions;
using static VErp.Commons.Enums.Manafacturing.EnumProductionProcess;
using VErp.Commons.Enums.Manafacturing;

namespace MigrateData.Services
{


    internal interface IMigrateProductionProcessStatusToProductService
    {
        Task Execute();
    }

    internal class MigrateProductionProcessStatusToProductService : IMigrateProductionProcessStatusToProductService
    {
        private readonly ManufacturingDBContext _manufacturingDBContext;
        private readonly StockDBContext _stockDBContext;

        public MigrateProductionProcessStatusToProductService(ManufacturingDBContext manufacturingDBContext, StockDBContext stockDBContext)
        {
            _manufacturingDBContext = manufacturingDBContext;
            _stockDBContext = stockDBContext;
        }

        public async Task Execute()
        {
            var createdProductionProcessProductIds = await _manufacturingDBContext.ProductionStep
                .IgnoreQueryFilters()
                .Where(s => s.ContainerTypeId == (int)EnumContainerType.Product && !s.IsDeleted)
                .Select(p => p.ContainerId)
                .ToListAsync();
            /*  await _masterDbContext.CustomGenCode.Where(c => c.CustomGenCodeId != customGenCodeId)
                    .UpdateByBatch(c => new CustomGenCode()
                    {
                        IsDefault = false
                    });*/

            var createdProductionProcessProducts = _stockDBContext.Product
                .IgnoreQueryFilters()
                .Where(p => !p.IsDeleted && createdProductionProcessProductIds.Contains(p.ProductId));
            await createdProductionProcessProducts.UpdateByBatch(p => new Product()
            {
                ProductionProcessStatusId = (int)EnumProductionProcessStatus.Created,
            });
        }
    }
}
