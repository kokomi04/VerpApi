using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ManufacturingDB;
using VErp.Commons.Library;
using VErp.Services.Manafacturing.Model.ProductionOrder.Materials;
using VErp.Services.Manafacturing.Model.ProductionAssignment;

namespace VErp.Services.Manafacturing.Model.ProductionHandover
{
    public class RequestObjectGetProductionOrderHandover
    {
        public long ProductionOrderId { get; set; }
        public long? ProductionStepId { get; set; }
    }
}
