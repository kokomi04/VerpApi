using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ManufacturingDB;

namespace VErp.Services.Manafacturing.Model.Outsource.RequestStep
{
    public class OutsourceStepRequestDataModel : IMapFrom<OutsourceStepRequestData>
    {
        public long OutsourceStepRequestId { get; set; }
        public long ProductionStepLinkDataId { get; set; }
        public decimal? Quantity { get; set; }
        public int ProductionStepLinkDataRoleTypeId { get; set; }
    }

    public class OutsourceStepRequestDataInfo : OutsourceStepRequestDataModel
    {
    }
}
