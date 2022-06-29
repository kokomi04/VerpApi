namespace VErp.Services.Manafacturing.Model.ProductionOrder
{
    public class StepLinkDataInfo
    {
        public long ProductionStepLinkDataId { get; set; }
        public long ProductionStepId { get; set; }
        public string StepName { get; set; }
        public long? OutsourceStepRequestId { get; set; }
        public string OutsourceStepRequestCode { get; set; }
    }


}
