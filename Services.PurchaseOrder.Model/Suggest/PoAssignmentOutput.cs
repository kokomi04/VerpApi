﻿using System.Collections.Generic;
using VErp.Commons.Enums.MasterEnum.PO;

namespace VErp.Services.PurchaseOrder.Model
{
    public class PoAssignmentOutputList
    {
        public long PoAssignmentId { get; set; }
        public long PurchasingSuggestId { get; set; }
        public string PurchasingSuggestCode { get; set; }
        public long PurchasingSuggestDate { get; set; }

        public string PoAssignmentCode { get; set; }
        public int AssigneeUserId { get; set; }

        public EnumPoAssignmentStatus PoAssignmentStatusId { get; set; }
        public bool? IsConfirmed { get; set; }
        public int CreatedByUserId { get; set; }
        public long CreatedDatetimeUtc { get; set; }
        public string Content { get; set; }
    }

    public class PoAssignmentOutput : PoAssignmentOutputList
    {

        public IList<PoAssimentDetailModel> Details { get; set; }
    }

    public class PoAssignmentOutputListByProduct : PoAssignmentOutputList, IPoAssimentDetailModel
    {
        public long? PoAssignmentDetailId { get; set; }
        public long PurchasingSuggestDetailId { get; set; }
        public decimal PrimaryQuantity { get; set; }
        public decimal PrimaryUnitPrice { get; set; }

        public int ProductUnitConversionId { get; set; }
        public decimal ProductUnitConversionQuantity { get; set; }
        public decimal ProductUnitConversionPrice { get; set; }

        public decimal? TaxInPercent { get; set; }
        public decimal? TaxInMoney { get; set; }

        //output
        public int? ProductId { get; set; }
        public string ProviderProductName { get; set; }
        public int? CustomerId { get; set; }
    }
}
