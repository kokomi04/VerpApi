using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Stock.Model.Inventory;
using VErp.Services.Stock.Model.StockTake;

namespace VErp.Services.Stock.Service.StockTake

{
    public interface IStockTakeService
    {
        Task<PageData<StockTakePeriotListModel>> GetStockTakePeriods(string keyword, int page, int size);


    }
}
