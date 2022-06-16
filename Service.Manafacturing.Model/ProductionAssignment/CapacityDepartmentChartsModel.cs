namespace VErp.Services.Manafacturing.Model.ProductionAssignment
{
    public class CapacityDepartmentChartsModel
    {
        public int DepartmentId { get; set; }
        public long ProductionOrderId { get; set; }
        public decimal Capacity { get; set; }
    }
}
