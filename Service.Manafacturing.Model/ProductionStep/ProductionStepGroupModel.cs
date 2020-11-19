using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Services.Manafacturing.Model.ProductionStep
{
    public class ProductionStepGroupModel: ProductionStepInfo
    {
        public IList<long>  ListProductionStepId { get; set; }
    }
}
