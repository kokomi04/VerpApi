using System.Collections.Generic;

namespace VErp.Infrastructure.EF.EFExtensions
{
    public interface IStockRequestDbContext
    {
        List<int> StockIds { get; }
    }

}
