namespace VErp.Services.PurchaseOrder.Model.PurchaseOrder
{
    public class EnrichDataPurchaseOrderAllocate
    {
        public long PurchaseOrderId { get; set; }
        public long PurchaseOrderDetailId { get; set; }
        public long? OutsourceRequestId { get; set; }
        public long? ProductionOrderId { get; set; }
        public string ProductionOrderCode { get; set; }
        public string OutsourceRequestCode { get; set; }
        public long PurchaseOrderOutsourceMappingId { get; set; }
    }

}