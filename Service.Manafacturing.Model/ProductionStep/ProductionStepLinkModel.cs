using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ManufacturingDB;

namespace VErp.Services.Manafacturing.Model.ProductionStep
{
    public class ProductionStepLinkModel: IMapFrom<ProductionStepLink>
    {
        public int ProductId { get; set; }
        public int FromStepId { get; set; }
        public int ToStepId { get; set; }
    }
}
