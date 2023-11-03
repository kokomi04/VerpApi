using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.MasterEnum.PO;
using VErp.Commons.GlobalObject;

namespace VErp.Services.PurchaseOrder.Model.PurchaseOrder
{
    public class PurchaseOrderFilterRequestModel
    {
        public string Keyword { get; set; }
        public IList<string> PoCodes { get; set; }
        public IList<int> PurchaseOrderTypes { get; set; }
        public IList<int> ProductIds { get; set; }
        public IList<long> IgnoreDetailIds { get; set; }
        public EnumPurchaseOrderStatus? PurchaseOrderStatusId { get; set; }
        public EnumPoProcessStatus? PoProcessStatusId { get; set; }
        public int? CreateByUserId { get; set; }
        public int? CheckByUserId { get; set; }
        public int? CensorByUserId { get; set; }
        public bool? IsChecked { get; set; }
        public bool? IsApproved { get; set; }
        public long? FromDate { get; set; }
        public long? ToDate { get; set; }
        public string SortBy { get; set; }
        public bool Asc { get; set; }
        public int Page { get; set; }
        public int Size { get; set; }

        public Clause Filters { get; set; }

        public void Deconstruct(out string keyword, out IList<string> poCodes, out IList<int> purchaseOrderTypes, out IList<int> productIds,
            out EnumPurchaseOrderStatus? purchaseOrderStatusId, out EnumPoProcessStatus? poProcessStatusId, out int? createByUserId, out int? checkByUserId, out int? censorByUserId,
            out bool? isChecked, out bool? isApproved,
            out long? fromDate, out long? toDate,
            out string sortBy, out bool asc, out int page, out int size, out Clause filters)
        {
            keyword = Keyword;
            poCodes = PoCodes;
            purchaseOrderTypes = PurchaseOrderTypes;
            productIds = ProductIds;
            purchaseOrderStatusId = PurchaseOrderStatusId;
            poProcessStatusId = PoProcessStatusId;
            createByUserId = CreateByUserId;
            checkByUserId = CheckByUserId;
            censorByUserId = CensorByUserId;
            isChecked = IsChecked;
            isApproved = IsApproved;
            fromDate = FromDate;
            toDate = ToDate;
            sortBy = SortBy;
            asc = Asc;
            page = Page;
            size = Size;
            filters = Filters;
        }
    }
}
