using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Services.PurchaseOrder.Model.Request
{
    public class SingleInvoicePurchasingRequestExcelMappingModel
    {
        public string SheetName { get; set; }
        public int FromRow { get; set; }
        public int ToRow { get; set; }

        public SingleInvoiceStaticContent StaticValue { get; set; }

        public PurchasingRequestDetailMappingColumn ColumnMapping{ get; set; }

        public class SingleInvoiceStaticContent
        {
            public string OrderCode { get; set; }
            public string ProductionOrderCode { get; set; }

            public string ProductUnitConversionName { get; set; }
        }

        public class PurchasingRequestDetailMappingColumn
        {
            public string ProductCodeColumn { get; set; }
            public string ProductNameColumn { get; set; }
            public string PrimaryQuantityColumn { get; set; }            
            public string ProductUnitConversionNameColumn { get; set; }
            public string ProductUnitConversionQuantityColumn { get; set; }
            public string OrderCodeColumn { get; set; }
            public string ProductionOrderCodeColumn { get; set; }
            public string DescriptionColumn { get; set; }
        }
    }

    public class PurchasingRequestDetailRowValue
    {
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string ProductInternalName { get; set; }
        public decimal PrimaryQuantity { get; set; }
        public string ProductUnitConversionName { get; set; }
        public decimal ProductUnitConversionQuantity { get; set; }
        public string OrderCode { get; set; }
        public string ProductionOrderCode { get; set; }
        public string Description { get; set; }
    }
}
