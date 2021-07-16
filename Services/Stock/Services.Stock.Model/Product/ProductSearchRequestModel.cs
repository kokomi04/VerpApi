using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Services.Stock.Model.Product
{
    public class ProductSearchRequestModel
    {
     
        public string Keyword { get; set; }
        public IList<int> ProductIds { get; set; }
        public string ProductName { get; set; }
        public int Page { get; set; }
        public int Size { get; set; }
        public int[] ProductTypeIds { get; set; }
        public int[] ProductCateIds { get; set; }
        public bool? IsProductSemi { get; set; }
        public bool? IsProduct { get; set; }
        public bool? IsMaterials { get; set; }
    }

    public class ProductExportRequestModel: ProductSearchRequestModel
    {
        public IList<string> FieldNames { get; set; }
    }
}
