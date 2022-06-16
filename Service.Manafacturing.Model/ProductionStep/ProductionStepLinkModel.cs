namespace VErp.Services.Manafacturing.Model.ProductionStep
{
    public class ProductionStepLinkModel
    {
        public long FromStepId { get; set; }
        public string FromStepCode { get; set; }
        public long ToStepId { get; set; }
        public string ToStepCode { get; set; }
        public int? ProductionStepLinkTypeId { get; set; }
    }
}
