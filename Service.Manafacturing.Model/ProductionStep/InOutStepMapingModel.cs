using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ManufacturingDB;

namespace VErp.Services.Manafacturing.Model.ProductionStep
{
    public class InOutStepMapingModel : IMapFrom<InOutStepLink>
    {
        public int ProductionStepId { get; set; }
        public int ProductInStepId { get; set; }
        public EnumInOutStepType InOutStepType { get; set; }
    }
}
