﻿using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.AccountancyDB;

public partial class InputField
{
    public int InputFieldId { get; set; }

    public string FieldName { get; set; }

    public string Title { get; set; }

    public string Placeholder { get; set; }

    public int SortOrder { get; set; }

    public int DataTypeId { get; set; }

    public int DataSize { get; set; }

    public int DecimalPlace { get; set; }

    public int FormTypeId { get; set; }

    public string DefaultValue { get; set; }

    public string RefTableCode { get; set; }

    public string RefTableField { get; set; }

    public string RefTableTitle { get; set; }

    public int CreatedByUserId { get; set; }

    public DateTime CreatedDatetimeUtc { get; set; }

    public int UpdatedByUserId { get; set; }

    public DateTime UpdatedDatetimeUtc { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedDatetimeUtc { get; set; }

    public string Structure { get; set; }

    public bool IsReadOnly { get; set; }

    public string OnFocus { get; set; }

    public string OnKeydown { get; set; }

    public string OnKeypress { get; set; }

    public string OnBlur { get; set; }

    public string OnChange { get; set; }

    public string OnClick { get; set; }

    public string ReferenceUrl { get; set; }

    public bool? IsImage { get; set; }

    public string MouseEnter { get; set; }

    public string MouseLeave { get; set; }

    public string CustomButtonHtml { get; set; }

    public string CustomButtonOnClick { get; set; }

    public int? ObjectApprovalStepTypeId { get; set; }

    public string SqlValue { get; set; }

    public virtual ICollection<InputAreaField> InputAreaField { get; set; } = new List<InputAreaField>();
}
