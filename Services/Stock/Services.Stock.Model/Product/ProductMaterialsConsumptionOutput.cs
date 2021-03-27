using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VErp.Services.Stock.Model.Product
{
    public class ProductMaterialsConsumptionOutput: ProductMaterialsConsumptionBaseModel
    {
        //public decimal TotalQuantityInheritance { get; set; }
        public decimal BomQuantity { get; set; }

        public IList<ProductMaterialsConsumptionOutput> MaterialsConsumptionInheri { get; set; }
    }
}
