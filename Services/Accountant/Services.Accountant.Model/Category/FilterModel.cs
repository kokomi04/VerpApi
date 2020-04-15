﻿
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VErp.Commons.Enums.AccountantEnum;

namespace VErp.Services.Accountant.Model.Category

{
    public class FilterModel
    {
        public int CategoryFieldId { get; set; }
        public EnumOperator Operator { get; set; }
        public string Value { get; set; }

    }
}
