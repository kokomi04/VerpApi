namespace VErp.Services.Stock.Model.Product
{
    public class ProductElementModel
    {
        public int ParentProductId { get; set; }
        public int ProductId { get; set; }
        public int ProductCateId { get; set; }
        public decimal? Quantity { get; set; }
        public decimal? Wastage { get; set; }
        public ProductElementModel()
        {
        }
    }
}
