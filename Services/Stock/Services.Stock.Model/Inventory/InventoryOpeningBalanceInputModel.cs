using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Services.Stock.Model.Inventory
{
    public class InventoryOpeningBalanceInputModel
    {
        public int StockId { get; set; }

        public string IssuedDate { get; set; }

        public string Description { get; set; }

        public IList<long> FileIdList { set; get; }
    }
}
