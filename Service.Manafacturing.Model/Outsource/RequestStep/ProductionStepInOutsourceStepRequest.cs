using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Services.Manafacturing.Model.Outsource.RequestStep
{
    public class ProductionStepInOutsourceStepRequest
    {
        public long OutsourceStepRequestId { get; set; }
        public string OutsourceStepRequestCode { get; set; }
        public long ProductionProcessId { get; set; }
        public long ProductionStepId { get; set; }
        public string ProductionStepCode { get; set; }
    }
}
