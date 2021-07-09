using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.StockDB;

namespace VErp.Services.Stock.Model.Product
{
    public class ProductBomModel
    {
        public IList<ProductBomInput> ProductBoms { get; set; }
        public IList<ProductMaterialModel> ProductMaterials { get; set; }
        public IList<ProductPropertyModel> ProductProperties { get; set; }
        public bool IsCleanOldMaterial { get; set; }

        public ProductBomModel()
        {
            ProductBoms = new List<ProductBomInput>();
            ProductMaterials = new List<ProductMaterialModel>();
            ProductProperties = new List<ProductPropertyModel>();
        }
    }
}
