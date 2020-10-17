using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.MasterEnum;

namespace VErp.Services.PurchaseOrder.Model.Voucher
{
    public class VoucherTypeListInfo
    {
        public string Title { get; set; }
        public string InputTypeCode { get; set; }
        public IList<VoucherTypeViewModelList> Views { get; set; }
        public IList<VoucherTypeListColumn> ColumnsInList { get; set; }
    }

    public class VoucherTypeListColumn
    {
        public int VoucherAreaFieldId { get; set; }
        public int VoucherAreaId { get; set; }
        public int FieldIndex { get; set; }
        public string FieldName { get; set; }
        public string FieldTitle { get; set; }
        public bool IsMultiRow { get; set; }
        public int? ReferenceCategoryFieldId { get; set; }
        public int? ReferenceCategoryTitleFieldId { get; set; }
        public string ReferenceCategoryTitleFieldName { get; set; }
        public EnumDataType DataTypeId { get; set; }

    }

    public class VoucherValueBillListOutput
    {
        public long VoucherValueBillId { get; set; }
        public IDictionary<int, string> FieldValues { get; set; }
    }
}
