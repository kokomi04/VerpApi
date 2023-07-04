namespace VErp.Commons.GlobalObject.InternalDataInterface.Stock
{
    public class ProductBomBaseSimple
    {
        public long? ProductBomId { get; set; }
        public int Level { get; set; }
        public int ProductId { get; set; }
        public int? ChildProductId { get; set; }

        public string ProductCode { get; set; }

        public string ProductName { get; set; }
        public string Specification { get; set; }

        public decimal Quantity { get; set; }
        public decimal Wastage { get; set; }
        public decimal TotalQuantity { get; set; }

        public string Description { get; set; }
        public string UnitName { get; set; }
        public int UnitId { get; set; }
        public bool IsMaterial { get; set; }
        public string NumberOrder { get; set; }
        public int ProductUnitConversionId { get; set; }
        public int DecimalPlace { get; set; }
        public int? InputStepId { get; set; }
        public int? OutputStepId { get; set; }
        public bool? IsIgnoreStep { get; set; }

    }
}