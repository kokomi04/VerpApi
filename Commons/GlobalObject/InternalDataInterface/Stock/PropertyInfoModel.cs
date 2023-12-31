﻿using System.ComponentModel.DataAnnotations;

namespace VErp.Commons.GlobalObject.InternalDataInterface.Stock
{
    public class PropertyInfoModel
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

        [MaxLength(512, ErrorMessage = "Nhóm thuộc tính quá dài")]
        public string PropertyGroup { get; set; }

    }
}
