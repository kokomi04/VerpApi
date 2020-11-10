using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Services.Stock.Model.Product
{
    public class ProductBomOutput
    {
        public long? ProductBomId { get; set; }
        public int Level { get; set; }
        public int ProductId { get; set; }
        public int? ChildProductId { get; set; }

        public string ProductCode { get; set; }

        public string ProductName { get; set; }
        public string ProductSpecification { get; set; }

        public decimal Quantity { get; set; }
        public decimal Wastage { get; set; }
        public string Description { get; set; }
        public string UnitName { get; set; }
        public bool IsMaterial { get; set; }
        public string PathProductIds { get; set; }
        public string NumberOrder { get; set; }
    }
}
