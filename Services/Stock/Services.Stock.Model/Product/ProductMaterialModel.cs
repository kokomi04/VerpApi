using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.StockDB;
using System.ComponentModel.DataAnnotations;
using AutoMapper;

namespace VErp.Services.Stock.Model.Product
{
    public class ProductMaterialModel
    {
        public int ProductMaterialId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập sản phẩm chính")]
        public int RootProductId { get; set; }
        public int[] PathProductIds { get; set; }

        public string[] PathProductCodes { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn sản phẩm là nguyên vật liệu")]
        public int ProductId { get; set; }
    }
}
