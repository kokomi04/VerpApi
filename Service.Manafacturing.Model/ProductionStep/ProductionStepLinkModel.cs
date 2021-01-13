using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ManufacturingDB;
using static VErp.Commons.Enums.Manafacturing.EnumProductionProcess;

namespace VErp.Services.Manafacturing.Model.ProductionStep
{
    public class ProductionStepLinkModel
    {
        public long FromStepId { get; set; }
        public string FromStepCode { get; set; }
        public long ToStepId { get; set; }
        public string ToStepCode { get; set; }
        public int? ProductionStepLinkTypeId { get; set; }
    }
}
