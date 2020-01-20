using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Services.Stock.Model.Product
{
    public class BillOfMaterialOutput
    {
        public long BillOfMaterialId { get; set; }
        public int? Level { get; set; }

        //public int RootProductId { get; set; }

        public int? ProductId { get; set; }
        public int? ParentProductId { get; set; }

        public string ProductCode { get; set; }

        public string ProductName { get; set; }        

        public string ProductCateName { get; set; }

        public string ProductSpecification { get; set; }

        public decimal? Quantity { get; set; }
        public decimal? Wastage { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        
    }
}
