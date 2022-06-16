namespace VErp.Commons.GlobalObject.InternalDataInterface
{
    public class InternalProductElementModel
    {
        public int ParentProductId { get; set; }
        public int ProductId { get; set; }
        public int ProductCateId { get; set; }
        public decimal? Quantity { get; set; }
        public decimal? Wastage { get; set; }
    }
}
