using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Services.Manafacturing.Model.Outsource.RequestStep
{
    public class OutsourceStepRequestProductionStepInfo
    {
        public long ProductionStepId { get; set; }
        public long ProductionOrderDetailId { get; set; }
        public string ProductionOrderCode { get; set; }
        public string OrderCode { get; set; }
        public bool MarkInvalid { get; set; }
    }
}
