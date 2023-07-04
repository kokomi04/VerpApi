﻿using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.PurchaseOrderDB;

public partial class VoucherType
{
    public int VoucherTypeId { get; set; }

    public int? VoucherTypeGroupId { get; set; }

    public string Title { get; set; }

    public string VoucherTypeCode { get; set; }

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

    public bool IsHide { get; set; }

    public virtual ICollection<VoucherArea> VoucherArea { get; set; } = new List<VoucherArea>();

    public virtual ICollection<VoucherAreaField> VoucherAreaField { get; set; } = new List<VoucherAreaField>();

    public virtual ICollection<VoucherBill> VoucherBill { get; set; } = new List<VoucherBill>();

    public virtual VoucherTypeGroup VoucherTypeGroup { get; set; }

    public virtual ICollection<VoucherTypeView> VoucherTypeView { get; set; } = new List<VoucherTypeView>();
}
