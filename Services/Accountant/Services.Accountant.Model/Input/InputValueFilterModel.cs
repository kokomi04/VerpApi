using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.AccountantEnum;

namespace VErp.Services.Accountant.Model.Input
{
    public class InputValueFilterModel
    {
        public int InputAreaFieldId { get; set; }
        public EnumOperator Operator { get; set; }
        public string[] Values { get; set; }
    }
}
