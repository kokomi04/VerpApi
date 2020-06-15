using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Services.PurchaseOrder.Model
{
    public class PurchasingSuggestBasicInfo
    {
        public long PurchasingSuggestId { get; set; }
        public string PurchasingSuggestCode { get; set; }

    }

    public class PurchasingSuggestDetailInfo: PurchasingSuggestBasicInfo
    {     
        public long PurchasingSuggestDetailId { get; set; }
        public int ProductId { get; set; }
        public decimal PrimaryQuantity { get; set; }

        public int ProductUnitConversionId { get; set; }
        public decimal ProductUnitConversionQuantity { get; set; }
        
    }

    public class PoAssignmentBasicInfo
    {
        public long PoAssignmentId { get; set; }
        public string PoAssignmentCode { get; set; }
    }

    public class PoAssignmentDetailInfo: PoAssignmentBasicInfo
    {
        public long PoAssignmentDetailId { get; set; }
        public int ProductId { get; set; }
        public decimal PrimaryQuantity { get; set; }

        public int ProductUnitConversionId { get; set; }
        public decimal ProductUnitConversionQuantity { get; set; }

    }
}
