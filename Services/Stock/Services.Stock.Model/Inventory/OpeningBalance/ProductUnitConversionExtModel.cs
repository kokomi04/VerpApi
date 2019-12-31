using System;
using System.Collections.Generic;
using System.Text;
using VErp.Infrastructure.EF.StockDB;

namespace VErp.Services.Stock.Model.Inventory.OpeningBalance
{
    public class ProductUnitConversionExtModel: ProductUnitConversion
    {
        public string ProductCode { set; get; }
    }
}
