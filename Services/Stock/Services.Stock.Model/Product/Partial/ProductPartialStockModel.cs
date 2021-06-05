using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.MasterEnum;
using static VErp.Commons.GlobalObject.InternalDataInterface.ProductModel;

namespace VErp.Services.Stock.Model.Product.Partial
{
    public class ProductPartialStockModel
    {
        public IList<int> StockIds { get; set; }

        public long? AmountWarningMin { get; set; }
        public long? AmountWarningMax { get; set; }
        public string DescriptionToStock { get; set; }

        public EnumStockOutputRule? StockOutputRuleId { get; set; }

        public IList<ProductModelUnitConversion> UnitConversions { get; set; }
    }
}
