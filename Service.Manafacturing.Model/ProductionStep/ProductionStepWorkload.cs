using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;
using ProductionStepEntity = VErp.Infrastructure.EF.ManufacturingDB.ProductionStep;

namespace VErp.Services.Manafacturing.Model.ProductionStep
{
    public class ProductionStepWorkload: IMapFrom<ProductionStepEntity>
    {
        public long ProductionStepId { get; set; }
        public decimal? Workload { get; set; }
    }
}
