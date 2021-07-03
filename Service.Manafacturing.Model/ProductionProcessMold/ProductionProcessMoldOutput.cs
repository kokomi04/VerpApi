using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;
using ProductionProcessMoldEntity = VErp.Infrastructure.EF.ManufacturingDB.ProductionProcessMold;

namespace VErp.Services.Manafacturing.Model.ProductionProcessMold
{
    public class ProductionProcessMoldOutput: ProductionProcessMoldSimple, IMapFrom<ProductionProcessMoldEntity>
    {
    }

    public class ProductionProcessMoldSimple : IMapFrom<ProductionProcessMoldEntity>
    {
        public long ProductionProcessMoldId { get; set; }
        public string Title { get; set; }
    }
}
