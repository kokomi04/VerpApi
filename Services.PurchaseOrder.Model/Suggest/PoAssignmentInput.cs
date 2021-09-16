using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Services.PurchaseOrder.Model
{
    public class PoAssignmentInput
    {
        public int AssigneeUserId { get; set; }
        public string Content { get; set; }
        public IList<PoAssimentDetailModel> Details { get; set; }
    }

    public interface IPoAssimentDetailModel
    {
        long? PoAssignmentDetailId { get; set; }
        long PurchasingSuggestDetailId { get; set; }
        decimal PrimaryQuantity { get; set; }
        decimal PrimaryUnitPrice { get; set; }

        public int ProductUnitConversionId { get; set; }
        public decimal ProductUnitConversionQuantity { get; set; }
        public decimal ProductUnitConversionPrice { get; set; }

        decimal? TaxInPercent { get; set; }
        decimal? TaxInMoney { get; set; }

        //output
        int? ProductId { get; set; }
        string ProviderProductName { get; set; }
        int? CustomerId { get; set; }
    }

    public class PoAssimentDetailModel: IPoAssimentDetailModel
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
        public int? SortOrder { get; set; }

        //output
        public int? ProductId { get; set; }
        public string ProviderProductName { get; set; }
        public int? CustomerId { get; set; }
    }
}
