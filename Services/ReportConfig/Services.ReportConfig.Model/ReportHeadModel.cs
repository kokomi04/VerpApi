using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.MasterEnum;

namespace Verp.Services.ReportConfig.Model
{
    public class ReportHeadModel
    {
        public string Value { get; set; }
        public int FontSize { get; set; }
        public string TextAlign { get; set; }
        public EnumDataType DataTypeId { get; set; }
        public bool IsBold { get; set; }
        public bool IsItalic { get; set; }
        public bool IsMergedCell {get;set;}
    }
}
