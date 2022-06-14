﻿using System;
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

        public ProductSearchRequestModel(string keyword, IList<int> productIds, string productName, int[] productTypeIds, int[] productCateIds, int page, int size, bool? isProductSemi, bool? isProduct, bool? isMaterials, Clause filters = null, IList<int> stockIds = null)
        {
            this.Keyword = keyword;
            this.ProductIds = productIds;
            this.ProductName = productName;
            this.ProductTypeIds = productTypeIds;
            this.ProductCateIds = productCateIds;
            this.IsProductSemi = isProductSemi;
            this.IsProduct = isProduct;
            this.IsMaterials = isMaterials;
            this.Page = page;
            this.Size = size;
            this.Filters = filters;
            this.StockIds = stockIds;
        }
    }

    public class ProductExportRequestModel : ProductFilterRequestModel
    {
        public IList<string> FieldNames { get; set; }
    }
}
