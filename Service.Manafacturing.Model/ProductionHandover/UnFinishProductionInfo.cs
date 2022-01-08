using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ManufacturingDB;
using VErp.Commons.Library;

namespace VErp.Services.Manafacturing.Model.ProductionHandover
{
    public class UnFinishProductionInfo
    {
        public long ProductionOrderId { get; set; }
        public string ProductionOrderCode { get; set; }
        public IList<UnFinishProductionStepInfo> ProductionStep { get; set; }
    }

    public class UnFinishProductionStepInfo
    {
        public long ProductionStepId { get; set; }
        public string ProductionStepTitle { get; set; }
    }
}
