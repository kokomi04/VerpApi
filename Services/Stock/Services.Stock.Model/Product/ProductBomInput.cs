﻿using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.StockDB;

namespace VErp.Services.Stock.Model.Product
{
    public class ProductBomInput : IMapFrom<ProductBom>
    {
        public long? ProductBomId { get; set; }
        public int ProductId { get; set; }
        public int? ParentProductId { get; set; }
        public decimal? Quantity { get; set; }
        public decimal? Wastage { get; set; }
    }
}
