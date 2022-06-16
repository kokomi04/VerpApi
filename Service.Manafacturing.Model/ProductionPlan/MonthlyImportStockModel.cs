namespace VErp.Services.Manafacturing.Model.WorkloadPlanModel
{
    public class ImportProductModel
    {
        public long ProductId { get; set; }
        public decimal PlanQuantity { get; set; }
        public decimal ImportQuantity { get; set; }
        public string OrderCode { get; set; }
        public string PartnerId { get; set; }

        public decimal LastestDateImportQuantity { get; set; }
        public decimal LastestWeekImportQuantity { get; set; }
    }
}
