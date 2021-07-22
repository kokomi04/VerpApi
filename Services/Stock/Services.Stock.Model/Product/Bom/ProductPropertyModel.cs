using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.StockDB;
using System.ComponentModel.DataAnnotations;
using AutoMapper;

namespace VErp.Services.Stock.Model.Product
{
    public class ProductPropertyModel: ProductBomInfoPathBaseModel
    {
        public int ProductPropertyId { get; set; }
       
        [Required(ErrorMessage = "Chi tiết của thuộc tính không hợp lệ")]
        public override int ProductId { get; set; }

        public int PropertyId { get; set; }
    }
}
