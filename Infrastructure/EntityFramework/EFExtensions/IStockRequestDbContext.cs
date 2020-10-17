using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Infrastructure.EF.EFExtensions
{
    public interface IStockRequestDbContext
    {
        List<int> StockIds { get; }
    }

}
