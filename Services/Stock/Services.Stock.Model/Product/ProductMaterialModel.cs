using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.StockDB;
using System.ComponentModel.DataAnnotations;

namespace VErp.Services.Stock.Model.Product
{
    public class ProductMaterialModel : IMapFrom<ProductMaterial>
    {
        public int ProductMaterialId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập sản phẩm chính")]
        public int RootProductId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập đường dẫn tới nguyên vật liệu")]
        public string PathProductIds { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn sản phẩm là nguyên vật liệu")]
        public int ProductId { get; set; }
    }
}
