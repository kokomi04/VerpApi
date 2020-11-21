using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Services.Manafacturing.Model.ProductionStep
{
    public class ProductionStepGroupModel: ProductionStepModel
    {
        public IList<long>  ListProductionStepId { get; set; }
    }
}
