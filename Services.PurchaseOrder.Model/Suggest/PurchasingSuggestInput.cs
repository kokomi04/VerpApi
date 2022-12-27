﻿using System.Collections.Generic;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.PurchaseOrderDB;

namespace VErp.Services.PurchaseOrder.Model
{
    public class PurchasingSuggestInput
    {
        public string PurchasingSuggestCode { get; set; }
        public long Date { get; set; }
        public string Content { get; set; }
        public IList<long> FileIds { get; set; }
        public List<PurchasingSuggestDetailInputModel> Details { set; get; }
        public decimal? TaxInMoney { get; set; }
        public decimal? TaxInPercent { get; set; }
        public decimal? TotalMoney { get; set; }
        public long UpdatedDatetimeUtc { get; set; }
    }

    public class PurchasingSuggestDetailInputModel
    {
        public long? PurchasingSuggestDetailId { get; set; }
        public int CustomerId { get; set; }
        public long? PurchasingRequestDetailId { get; set; }


        public int ProductId { get; set; }
        public decimal PrimaryQuantity { get; set; }
        public int ProductUnitConversionId { get; set; }
        public decimal ProductUnitConversionQuantity { get; set; }
        public decimal PrimaryUnitPrice { get; set; }
        public decimal ProductUnitConversionPrice { get; set; }
        public string OrderCode { get; set; }
        public string ProductionOrderCode { get; set; }

        public string Description { get; set; }
        public decimal? IntoMoney { get; set; }
        public int? SortOrder { get; set; }
        public string PoProviderPricingCode { get; set; }

        public bool IsSubCalculation { get; set; }

        public IList<PurchasingSuggestDetailSubCalculationModel> SubCalculations { get; set; }
    }


    public class PurchasingSuggestDetailSubCalculationModel : IMapFrom<PurchasingSuggestDetailSubCalculation>
    {
        public int PurchasingSuggestDetailSubCalculationId { get; set; }
        public long PurchasingSuggestDetailId { get; set; }
        public long ProductBomId { get; set; }
        public decimal PrimaryUnitPrice { get; set; }
        public decimal PrimaryQuantity { get; set; }
        public int? UnitConversionId { get; set; }
    }

}
