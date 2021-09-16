using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.OrganizationDB
{
    public partial class HrType
    {
        public HrType()
        {
            HrAction = new HashSet<HrAction>();
            HrArea = new HashSet<HrArea>();
            HrAreaField = new HashSet<HrAreaField>();
        }

        public int HrTypeId { get; set; }
        public int? HrTypeGroupId { get; set; }
        public string Title { get; set; }
        public string HrTypeCode { get; set; }
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

        public virtual HrTypeGroup HrTypeGroup { get; set; }
        public virtual ICollection<HrAction> HrAction { get; set; }
        public virtual ICollection<HrArea> HrArea { get; set; }
        public virtual ICollection<HrAreaField> HrAreaField { get; set; }
    }
}
