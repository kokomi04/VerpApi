using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;

namespace VErp.Infrastructure.EF.EFExtensions
{    
    public interface ISubsidiayRequestDbContext
    {
        ICurrentContextService CurrentContextService { get; }
        int SubsidiaryId { get; }
    }

    public interface IStockRequestDbContext
    {
        List<int> StockIds { get; }
    }
}
