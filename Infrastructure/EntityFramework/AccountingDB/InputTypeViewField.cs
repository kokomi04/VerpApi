using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.AccountingDB
{
    public partial class InputTypeViewField
    {
        public int InputTypeViewId { get; set; }
        public int InputAreaFieldId { get; set; }
        public int Column { get; set; }
        public int SortOrder { get; set; }
        public string DefaultValue { get; set; }
        public int Operator { get; set; }

        public virtual InputAreaField InputAreaField { get; set; }
        public virtual InputTypeView InputTypeView { get; set; }
    }
}
