using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.StockDB;
using System.ComponentModel.DataAnnotations;
using AutoMapper;

namespace VErp.Services.Stock.Model.Product
{
    public class ProductMaterialModel: ProductBomInfoPathBaseModel
    {
        public long ProductMaterialId { get; set; }
     
        [Required(ErrorMessage = "Chi tiết nguyên vật liệu không hợp lệ")]
        public override int ProductId { get; set; }
    }

    public class ProductBomMaterialUpdateInfo
    {
        public ProductBomMaterialUpdateInfo()
        {

        }
        public ProductBomMaterialUpdateInfo(IList<ProductMaterialModel> bomMaterials, bool cleanOldData)
        {
            BomMaterials = bomMaterials;
            CleanOldData = cleanOldData;
        }
        public IList<ProductMaterialModel> BomMaterials { get; set; }
        public bool CleanOldData { get; set; }
    }
   
}
