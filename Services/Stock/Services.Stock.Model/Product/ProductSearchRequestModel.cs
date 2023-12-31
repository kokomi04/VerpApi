﻿using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.Manafacturing;
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
        public EnumProductionProcessStatus? ProductionProcessStatusId { get; set; }
        public bool? IsMaterials { get; set; }
        public Clause Filters { get; set; }
        public IList<int> StockIds { get; set; }
        public IList<int> QuantityStockIds { get; set; }

        public ProductFilterRequestModel(string keyword, IList<int> productIds, string productName, int[] productTypeIds, int[] productCateIds, bool? isProductSemi, bool? isProduct, bool? isMaterials, EnumProductionProcessStatus? productionProcessStatusId, Clause filters = null, IList<int> stockIds = null, IList<int> quantityStockIds = null)
        {
            this.Keyword = keyword;
            this.ProductIds = productIds;
            this.ProductName = productName;
            this.ProductTypeIds = productTypeIds;
            this.ProductCateIds = productCateIds;
            this.IsProductSemi = isProductSemi;
            this.IsProduct = isProduct;
            this.IsMaterials = isMaterials;
            this.ProductionProcessStatusId = productionProcessStatusId;
            this.Filters = filters;
            this.StockIds = stockIds;
            this.QuantityStockIds = quantityStockIds;
        }
    }

    public class ProductSearchRequestModel : ProductFilterRequestModel
    {
        public ProductSearchRequestModel(string keyword, IList<int> productIds, string productName, int[] productTypeIds, int[] productCateIds, bool? isProductSemi, bool? isProduct, bool? isMaterials, EnumProductionProcessStatus? productionProcessStatusId, Clause filters = null, IList<int> stockIds = null, IList<int> quantityStockIds = null)
            : base(keyword, productIds, productName, productTypeIds, productCateIds, isProductSemi, isProduct, isMaterials, productionProcessStatusId, filters, stockIds, quantityStockIds)
        {
        }

        public int Page { get; set; }
        public int Size { get; set; }

    }

    public class ProductExportRequestModel : ProductFilterRequestModel
    {
        public ProductExportRequestModel(string keyword, IList<int> productIds, string productName, int[] productTypeIds, int[] productCateIds, bool? isProductSemi, bool? isProduct, bool? isMaterials, EnumProductionProcessStatus? productionProcessStatusId, Clause filters = null, IList<int> stockIds = null, IList<int> quantityStockIds = null)
            : base(keyword, productIds, productName, productTypeIds, productCateIds, isProductSemi, isProduct, isMaterials, productionProcessStatusId, filters, stockIds, quantityStockIds)
        {
        }

        public IList<string> FieldNames { get; set; }
    }
}
