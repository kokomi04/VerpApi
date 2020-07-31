using System;
using System.Collections.Generic;
using System.Text;

namespace Verp.Services.ReportConfig.Model
{
    public class ReportColumnModel
    {
        public int SortOrder { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public string Alias { get; set; }
        public string Where { get; set; }
        public string Width { get; set; }
        public int? DataTypeId { get; set; }
        public int? DecimalPlace { get; set; }
        public bool IsCalcSum { get; set; }
        public bool IsHidden { get; set; }
        public string RowSpan { get; set; }
        public string ColSpan { get; set; }

        public bool IsGroup { get; set; }

    }
}
