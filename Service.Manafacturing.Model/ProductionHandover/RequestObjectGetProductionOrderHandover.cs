namespace VErp.Services.Manafacturing.Model.ProductionHandover
{
    public class RequestObjectGetProductionOrderHandover
    {
        public long ProductionOrderId { get; set; }
        public long? ProductionStepId { get; set; }
    }
}
