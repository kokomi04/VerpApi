using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Services.Stock.Model.Product.Bom
{
    public class ProductBomUpdateInfoModel
    {
        public ProductBomUpdateInfo BomInfo { get; set; }
        public ProductBomMaterialUpdateInfo MaterialsInfo { get; set; }
        public ProductBomPropertyUpdateInfo PropertiesInfo { get; set; }
    }
}
