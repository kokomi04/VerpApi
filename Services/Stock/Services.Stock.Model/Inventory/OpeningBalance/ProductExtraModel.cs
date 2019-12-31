using System;
using System.Collections.Generic;
using System.Text;
using VErp.Infrastructure.EF.StockDB;

namespace VErp.Services.Stock.Model.Inventory.OpeningBalance
{
    public class ProductExtraModel:ProductExtraInfo
    {
        public string ProductCode { set; get; }
    }
}
