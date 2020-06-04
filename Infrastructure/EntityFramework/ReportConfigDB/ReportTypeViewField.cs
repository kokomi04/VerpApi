﻿using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.ReportConfigDB
{
    public partial class ReportTypeViewField
    {
        public int ReportTypeViewFieldId { get; set; }
        public int ReportTypeViewId { get; set; }
        public string ParamerterName { get; set; }
        public int Column { get; set; }
        public int SortOrder { get; set; }
        public string Title { get; set; }
        public string Placeholder { get; set; }
        public int DataTypeId { get; set; }
        public int DataSize { get; set; }
        public int FormTypeId { get; set; }
        public string DefaultValue { get; set; }
        public int? ReferenceCategoryId { get; set; }
        public int? ReferenceCategoryFieldId { get; set; }
        public int? ReferenceCategoryTitleFieldId { get; set; }
        public bool IsRequire { get; set; }
        public string RegularExpression { get; set; }

        public virtual ReportTypeView ReportTypeView { get; set; }
    }
}