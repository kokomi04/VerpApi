namespace VErp.Services.PurchaseOrder.Model
{
    public class PurchaseOrderOutsourcePartAllocate
    {
        public long PurchaseOrderId { get; set; }
        public string PurchaseOrderCode { get; set; }
        public int ProductId { get; set; }
        public decimal PrimaryQuantity { get; set; }
        public long PurchaseOrderDetailId { get; set; }
        public decimal PrimaryQuantityAllocated { get; set; }
    }
}