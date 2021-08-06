using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.StockDB;
using System.ComponentModel.DataAnnotations;
using AutoMapper;

namespace VErp.Services.Stock.Model.Product
{
    public class PropertyModel : IMapFrom<Property>
    {
        public int PropertyId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập mã thuộc tính")]
        [MinLength(1, ErrorMessage = "Mã thuộc tính quá ngắn")]
        [MaxLength(128, ErrorMessage = "Mã thuộc tính quá dài")]
        public string PropertyCode { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên thuộc tính")]
        [MinLength(1, ErrorMessage = "Tên thuộc tính quá ngắn")]
        [MaxLength(128, ErrorMessage = "Tên thuộc tính quá dài")]
        public string PropertyName { get; set; }
    }
}
