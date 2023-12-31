﻿using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.AccountantEnum;

namespace VErp.Services.Accountant.Model.Input
{
    public class InputTypeBillsRequestModel
    {
        public string Keyword { get; set; }
        public IList<InputValueFilterModel> FieldFilters { get; set; }
        public string OrderBy { get; set; }
        public bool Asc { get; set; } = true;
        public int Page { get; set; }
        public int Size { get; set; }
    }

    public class InputValueFilterModel
    {
        public int InputAreaFieldId { get; set; }
        public EnumOperator Operator { get; set; }
        public string[] Values { get; set; }
    }
}
