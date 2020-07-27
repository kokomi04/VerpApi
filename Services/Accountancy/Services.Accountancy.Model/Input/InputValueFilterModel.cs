﻿using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.AccountantEnum;

namespace VErp.Services.Accountancy.Model.Input
{
    public class InputTypeBillsRequestModel
    {
        public string Keyword { get; set; }
        public Dictionary<int, object> Filters { get; set; }
        public string OrderBy { get; set; }
        public bool Asc { get; set; } = true;
        public int Page { get; set; }
        public int Size { get; set; }
    }
}