using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.AccountancyDB
{
    public partial class InputAreaField
    {
        public int InputAreaFieldId { get; set; }
        public int InputFieldId { get; set; }
        public int InputTypeId { get; set; }
        public int InputAreaId { get; set; }
        public string Title { get; set; }
        public string Placeholder { get; set; }
        public int SortOrder { get; set; }
        public bool IsAutoIncrement { get; set; }
        public bool IsRequire { get; set; }
        public bool IsUnique { get; set; }
        public bool IsHidden { get; set; }
        public bool IsCalcSum { get; set; }
        public string RegularExpression { get; set; }
        public string DefaultValue { get; set; }
        public string Filters { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public string TitleStyleJson { get; set; }
        public string InputStyleJson { get; set; }
        public string OnFocus { get; set; }
        public string OnKeydown { get; set; }
        public string OnKeypress { get; set; }
        public string OnBlur { get; set; }
        public string OnChange { get; set; }
        public bool? AutoFocus { get; set; }
        public int Column { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }
        public string RequireFilters { get; set; }
        public string ReferenceUrl { get; set; }
        public bool IsBatchSelect { get; set; }
        public string OnClick { get; set; }

        public virtual InputArea InputArea { get; set; }
        public virtual InputField InputField { get; set; }
        public virtual InputType InputType { get; set; }
    }
}
