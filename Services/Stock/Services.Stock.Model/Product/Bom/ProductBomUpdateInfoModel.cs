namespace VErp.Services.Stock.Model.Product.Bom
{
    public class ProductBomUpdateInfoModel
    {
        public ProductBomUpdateInfo BomInfo { get; set; }
        public ProductBomMaterialUpdateInfo MaterialsInfo { get; set; }
        public ProductBomIgnoreStepUpdateInfo IgnoreStepInfo { get; set; }
        public ProductBomPropertyUpdateInfo PropertiesInfo { get; set; }
    }
}
