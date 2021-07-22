using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.StockDB;
using System.ComponentModel.DataAnnotations;
using AutoMapper;

namespace VErp.Services.Stock.Model.Product
{
    public class ProductBomDescriptionModel : ProductBomInfoPathBaseModel
    {
        public long ProductBomDescriptionId { get; set; }
     
        [Required(ErrorMessage = "Chi tiết mô tả không hợp lệ")]
        public override int ProductId { get; set; }

        public string Description { get; set; }
    }
}
