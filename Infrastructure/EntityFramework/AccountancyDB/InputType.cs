using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.AccountancyDB;

public partial class InputType
{
    public int InputTypeId { get; set; }

    public int? InputTypeGroupId { get; set; }

    public string Title { get; set; }

    public string InputTypeCode { get; set; }

    public int SortOrder { get; set; }

    public int CreatedByUserId { get; set; }

    public DateTime CreatedDatetimeUtc { get; set; }

    public int UpdatedByUserId { get; set; }

    public DateTime UpdatedDatetimeUtc { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedDatetimeUtc { get; set; }

    public string PreLoadAction { get; set; }

    public string PostLoadAction { get; set; }

    public string AfterLoadAction { get; set; }

    public string BeforeSubmitAction { get; set; }

    public string BeforeSaveAction { get; set; }

    public string AfterSaveAction { get; set; }

    public string AfterUpdateRowsJsAction { get; set; }

    public bool IsOpenning { get; set; }

    public bool IsHide { get; set; }

    public bool IsParentAllowcation { get; set; }

    public string DataAllowcationInputTypeIds { get; set; }

    public int? ResultAllowcationInputTypeId { get; set; }

    public string CalcResultAllowcationSqlQuery { get; set; }

    public virtual ICollection<InputArea> InputArea { get; set; } = new List<InputArea>();

    public virtual ICollection<InputAreaField> InputAreaField { get; set; } = new List<InputAreaField>();

    public virtual ICollection<InputBill> InputBill { get; set; } = new List<InputBill>();

    public virtual InputTypeGroup InputTypeGroup { get; set; }

    public virtual ICollection<InputTypeView> InputTypeView { get; set; } = new List<InputTypeView>();

    public virtual ICollection<InputType> InverseResultAllowcationInputType { get; set; } = new List<InputType>();

    public virtual InputType ResultAllowcationInputType { get; set; }
}
