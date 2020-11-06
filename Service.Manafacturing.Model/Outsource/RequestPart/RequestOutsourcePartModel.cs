using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ManufacturingDB;

namespace VErp.Services.Manafacturing.Model.Outsource.RequestPart
{
    public class RequestOutsourcePartModel : IMapFrom<RequestOutsourcePart>
    {
        public int RequestOutsourcePartId { get; set; }
        public int ProductionOrderDetailId { get; set; }
        public int ProductInStepId { get; set; }
        public int Quantity { get; set; }
        public string RequestOrder { get; set; }
        public int UnitId { get; set; }
    }
}
