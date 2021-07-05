using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.MasterDB
{
    public partial class CustomGenCodeValue
    {
        public int CustomGenCodeId { get; set; }
        public string BaseValue { get; set; }
        public int LastValue { get; set; }
        public string LastCode { get; set; }
        public int? TempValue { get; set; }
        public string TempCode { get; set; }
    }
}
