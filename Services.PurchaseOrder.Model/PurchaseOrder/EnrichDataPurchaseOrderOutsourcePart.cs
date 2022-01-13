namespace VErp.Services.PurchaseOrder.Model.PurchaseOrder
{
    public class EnrichDataPurchaseOrderOutsourcePart
    {
        public long PurchaseOrderId { get; set; }
        public long PurchaseOrderDetailId { get; set; }
        public long[] OutsourceRequestId { get; set; }
        public string ProductionOrderCode { get; set; }
        public string OutsourceRequestCode { get; set; }
    }

}