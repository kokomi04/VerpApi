﻿using System.ComponentModel.DataAnnotations;

namespace VErp.Services.Stock.Model.Product
{
    public abstract class ProductBomInfoPathBaseModel
    {
        [Required(ErrorMessage = "Vui lòng nhập sản phẩm chính")]
        public int RootProductId { get; set; }
        public int[] PathProductIds { get; set; }

        public string[] PathProductCodes { get; set; }

        [Required(ErrorMessage = "Chi tiết không hợp lệ")]
        public virtual int ProductId { get; set; }
    }
}
