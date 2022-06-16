using System.Collections.Generic;

namespace VErp.Services.Manafacturing.Model.ProductionStep
{
    public class ProductionProcessInfo
    {
        public List<ProductionStepInfo> ProductionSteps { get; set; }
        public List<ProductionStepLinkModel> ProductionStepLinks { get; set; }
    }
}
