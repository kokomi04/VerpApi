using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using VErp.Commons.Enums.AccountantEnum;

namespace VErp.Services.Master.Model.StoredProcedure
{
    public class StoredProcedureModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên hàm")]
        [RegularExpression(@"^(usp|uv|ufn)[a-zA-Z0-9_]*", ErrorMessage = @"Tên được bắt đầu với 'usp', 'uv' hoặc 'ufn'")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập kiểu dữ hàm")]
        public EnumStoreProcedureType Type { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập định nghĩa của hàm")]
        public string Definition { get; set; }
    }
}
