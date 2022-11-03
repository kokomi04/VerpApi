﻿using System;
using VErp.Commons.Enums.Report;

namespace Verp.Services.ReportConfig.Model
{
    [Serializable]
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
        public bool? IsArray { get; set; }
        public bool? IsRepeat { get; set; }
        public bool IsCalcSum { get; set; }
        public string CalcSumConditionCol { get; set; }
        public bool IsHidden { get; set; }
        public string RowSpan { get; set; }
        public string ColSpan { get; set; }
        public bool IsDockLeft { get; set; }
        public bool IsGroup { get; set; }
        public bool IsGroupRow { get; set; }
        public bool isGroupRowLevel2 { get; set; }
        public string VAlign { get; set; }
        public string HAlign { get; set; }
        public bool IsColGroup { get; set; }
        public int ColGroupId { get; set; }
        public string ColGroupName { get; set; }

        public string SuffixKey { get; set; }
        public string OriginValue { get; set; }

        public EnumReportDetailOpenType? DetailOpenTypeId { get; set; }
        public EnumReportDetailTarget? DetailTargetId { get; set; }
        public int? DetailReportId { get; set; }
        public string DetailReportParams { get; set; }

        public EnumReportDetailOpenType? HeaderOpenTypeId { get; set; }
        public EnumReportDetailTarget? HeaderTargetId { get; set; }
        public int? HeaderReportId { get; set; }
        public string HeaderReportParams { get; set; }

    }

    [Serializable]
    public class ReportDisplayConfigModel
    {
        public bool IsVirtualScrolling { get; set; }
    }
}
