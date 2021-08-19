namespace VErp.Services.Manafacturing.Model.Outsource.RequestPart
{
    public class MaterialsForProductOutsource
    {
        public long ProductId { get; set; }
        public long OutsourcePartRequestId { get; set; }
        public decimal Quantity { get; set; }
        public int? RootProductId { get; set; }
    }
}