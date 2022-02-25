using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ManufacturingDB;
using VErp.Commons.Library;

namespace VErp.Services.Manafacturing.Model.ProductionOrder
{
    public class StepLinkDataInfo
    {
        public long ProductionStepLinkDataId { get; set; }
        public long ProductionStepId { get; set; }
        public string StepName { get; set; }
        public long? OutsourceStepRequestId { get; set; }
        public string OutsourceStepRequestCode { get; set; }
    }


}
