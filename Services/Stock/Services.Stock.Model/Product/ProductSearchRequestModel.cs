using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;

namespace VErp.Services.Stock.Model.Product
{
    public class ProductFilterRequestModel
    {
        public string Keyword { get; set; }
        public IList<int> ProductIds { get; set; }
        public string ProductName { get; set; }
        public int[] ProductTypeIds { get; set; }
        public int[] ProductCateIds { get; set; }
        public bool? IsProductSemi { get; set; }
        public bool? IsProduct { get; set; }
        public bool? IsMaterials { get; set; }
        public Clause Filters { get; set; }
        public IList<int> StockIds { get; set; }
    }

    public class ProductSearchRequestModel : ProductFilterRequestModel
    {
        public int Page { get; set; }
        public int Size { get; set; }
    }

    public class ProductExportRequestModel : ProductSearchRequestModel
    {
        public IList<string> FieldNames { get; set; }
    }
}
