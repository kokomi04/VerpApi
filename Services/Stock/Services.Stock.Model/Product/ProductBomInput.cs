using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Services.Stock.Model.Product
{
    public class ProductBomInput
    {
        //public long ProductBomId { get; set; }
        //public int? Level { get; set; }
        public int RootProductId { get; set; }
        public int ProductId { get; set; }
        public int? ParentProductId { get; set; }
        public decimal? Quantity { get; set; }
        public decimal? Wastage { get; set; }
        public string Description { get; set; }
        //public DateTime CreatedDatetimeUtc { get; set; }
        //public DateTime UpdatedDatetimeUtc { get; set; }
    }
}
