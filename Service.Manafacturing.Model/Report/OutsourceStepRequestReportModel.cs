namespace VErp.Services.Manafacturing.Model.Report
{
    public class OutsourceStepRequestReportModel
    {
        public long OutsourceStepRequestId { get; set; }
        public string OutsourceStepRequestCode { get; set; }
        public string ProductionOrderCode { get; set; }
        public long ProductionOrderId { get; set; }
        public string ProductionStepArrayString { get; set; }
        public long ProductionStepLinkDataId { get; set; }
        public string ProductionStepLinkDataTitle { get; set; }
        public int UnitId { get; set; }
        public decimal Quantity { get; set; }
        public decimal QuantityComplete { get; set; }
        public long OutsourcePartRequestFinishDate { get; set; }
    }
}
