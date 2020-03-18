﻿using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Services.Stock.Model.Product
{
    public class ProductBomOutput
    {
        public long ProductBomId { get; set; }
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
        public long CreatedDatetimeUtc { get; set; }
        public long UpdatedDatetimeUtc { get; set; }
        
    }
}