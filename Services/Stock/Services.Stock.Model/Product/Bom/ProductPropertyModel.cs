using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VErp.Services.Stock.Model.Product
{
    public class ProductPropertyModel : ProductBomInfoPathBaseModel
    {
        public long ProductPropertyId { get; set; }

        [Required(ErrorMessage = "Chi tiết của thuộc tính không hợp lệ")]
        public override int ProductId { get; set; }

        public int PropertyId { get; set; }
    }

    public class ProductBomPropertyUpdateInfo
    {
        public ProductBomPropertyUpdateInfo()
        {

        }
        public ProductBomPropertyUpdateInfo(IList<ProductPropertyModel> bomProperties, bool cleanOldData)
        {
            BomProperties = bomProperties;
            CleanOldData = cleanOldData;
        }
        public IList<ProductPropertyModel> BomProperties { get; set; }
        public bool CleanOldData { get; set; }
    }
}
