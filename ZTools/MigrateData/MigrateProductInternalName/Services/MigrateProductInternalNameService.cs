using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VErp.Infrastructure.EF.StockDB;
using Z.BulkOperations;
using VErp.Commons.Library;

namespace MigrateProductInternalName.Services
{
    public interface IMigrateProductInternalNameService
    {
        Task Execute();
    }

    public class MigrateProductInternalNameService : IMigrateProductInternalNameService
    {
        private readonly StockDBContext _stockDBContext;
        public MigrateProductInternalNameService(StockDBContext stockDBContext)
        {
            _stockDBContext = stockDBContext;
        }

        public async Task Execute()
        {
            var products = await _stockDBContext
                .Product
                .IgnoreQueryFilters()
                .ToListAsync();

            foreach(var p in products)
            {
                p.ProductInternalName = p.ProductName.NormalizeAsInternalName();
            }

            await _stockDBContext.SaveChangesAsync();
        }
    }
}
