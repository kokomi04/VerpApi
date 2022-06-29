using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.StockDB;

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

            foreach (var p in products)
            {
                p.ProductInternalName = p.ProductName.NormalizeAsInternalName();
            }

            await _stockDBContext.SaveChangesAsync();
        }
    }
}
