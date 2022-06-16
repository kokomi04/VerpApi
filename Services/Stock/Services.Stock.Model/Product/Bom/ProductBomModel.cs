using System.Collections.Generic;

namespace VErp.Services.Stock.Model.Product
{
    public class ProductBomModel
    {
        public IList<ProductBomInput> ProductBoms { get; set; }
        public IList<ProductMaterialModel> ProductMaterials { get; set; }
        public IList<ProductIgnoreStepModel> ProductIgnoreSteps { get; set; }
        public IList<ProductPropertyModel> ProductProperties { get; set; }

        public bool IsCleanOldMaterial { get; set; }
        public bool IsCleanOldProperties { get; set; }
        public bool IsCleanOldIgnoreStep { get; set; }
        public ProductBomModel()
        {
            ProductBoms = new List<ProductBomInput>();
            ProductMaterials = new List<ProductMaterialModel>();
            ProductProperties = new List<ProductPropertyModel>();
            ProductIgnoreSteps = new List<ProductIgnoreStepModel>();
        }
    }
}
