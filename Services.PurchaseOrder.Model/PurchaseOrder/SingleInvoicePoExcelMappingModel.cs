using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Services.PurchaseOrder.Model.PurchaseOrder
{
    public class SingleInvoicePoExcelMappingModel
    {
        public string SheetName { get; set; }
        public int FromRow { get; set; }
        public int ToRow { get; set; }

        public SinglePoStaticContent StaticValue { get; set; }

        public PoDetailMappingColumn ColumnMapping { get; set; }

        public class SinglePoStaticContent
        {
            public string OrderCode { get; set; }
            public string ProductionOrderCode { get; set; }

            public string ProductUnitConversionName { get; set; }
        }

        public class PoDetailMappingColumn
        {
            public string ProductCodeColumn { get; set; }
            public string ProductNameColumn { get; set; }
            public string ProductProviderNameColumn { get; set; }
            
            public string PrimaryQuantityColumn { get; set; }
            public string PrimaryPriceColumn { get; set; }

            public string ProductUnitConversionNameColumn { get; set; }
            public string ProductUnitConversionQuantityColumn { get; set; }
            public string ProductUnitConversionPriceColumn { get; set; }

            public string MoneyColumn { get; set; }

            public string TaxInPercentColumn { get; set; }
            public string TaxInMoneyColumn { get; set; }

            public string OrderCodeColumn { get; set; }
            public string ProductionOrderCodeColumn { get; set; }
            public string DescriptionColumn { get; set; }
        }
    }

    public class PoDetailRowValue
    {
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string ProductInternalName { get; set; }
        public string ProductProviderName { get; set; }


        public decimal? PrimaryQuantity { get; set; }
        public decimal? PrimaryPrice { get; set; }

        public string ProductUnitConversionName { get; set; }
        public decimal? ProductUnitConversionQuantity { get; set; }
        public decimal? ProductUnitConversionPrice { get; set; }
        public decimal? Money { get; set; }

        public decimal TaxInPercent { get; set; }
        public decimal TaxInMoney { get; set; }


        public string OrderCode { get; set; }
        public string ProductionOrderCode { get; set; }
        public string Description { get; set; }
    }
}
