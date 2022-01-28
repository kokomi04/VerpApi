using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.StockDB;

namespace VErp.Services.Stock.Model.Product
{
    public class ProductMaterialsConsumptionGroupModel: IMapFrom<ProductMaterialsConsumptionGroup>
    {
        public int ProductMaterialsConsumptionGroupId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mã nhóm tiêu hao")]
        [MinLength(1, ErrorMessage = "Mã nhóm tiêu hao quá ngắn")]
        [MaxLength(128, ErrorMessage = "Mã nhóm tiêu hao quá dài")]
        public string ProductMaterialsConsumptionGroupCode { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên nhóm tiêu hao")]
        [MinLength(1, ErrorMessage = "Tên nhóm tiêu hao quá ngắn")]
        [MaxLength(128, ErrorMessage = "Tên nhóm tiêu hao quá dài")]
        public string Title { get; set; }
    }
}
