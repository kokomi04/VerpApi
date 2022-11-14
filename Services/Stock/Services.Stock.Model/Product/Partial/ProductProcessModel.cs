using VErp.Commons.Enums.Manafacturing;

namespace VErp.Services.Stock.Model.Product.Partial
{
    public class ProductProcessModel
    {
        public decimal Coefficient { get; set; }
        public EnumProductionProcessStatus ProductionProcessStatusId { get; set; }
    }
}
