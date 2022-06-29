using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.PurchaseOrderDB
{
    public partial class VoucherTypeGlobalSetting
    {
        public int VoucherTypeGlobalSettingId { get; set; }
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
    }
}
