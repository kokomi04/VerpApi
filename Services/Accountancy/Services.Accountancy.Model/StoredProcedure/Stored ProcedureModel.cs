using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using VErp.Commons.Enums.AccountantEnum;

namespace VErp.Services.Accountancy.Model.StoredProcedure
{
    public class StoredProcedureModel
    {
        [RegularExpression(@"^(usp|uv|ufn)[a-zA-Z0-9_]*", ErrorMessage = @"Tên được bắt đầu với 'usp', 'uv' hoặc 'ufn'")]
        public string Name { get; set; }
        public EnumStoreProcedureType Type { get; set; }
        public string Definition { get; set; }
    }
}
