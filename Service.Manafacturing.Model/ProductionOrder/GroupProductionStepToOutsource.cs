using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Services.Manafacturing.Model.ProductionOrder
{
    public class GroupProductionStepToOutsource
    {
        public string Title { get; set; }
        public long[] ProdictionStepId { get; set; }
        public long[] ProductionStepLinkDataOutput { get; set; }
        public long[] ProductionStepLinkDataOutputInterpolation { get; set; }
    }
}
