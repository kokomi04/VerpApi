using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Services.Manafacturing.Model.ProductionStep
{
    public class ProductionProcessInfo
    {
        public List<ProductionStepInfo> ProductionSteps { get; set; }
        public List<ProductionStepLinkModel> ProductionStepLinks { get; set; }
    }
}
