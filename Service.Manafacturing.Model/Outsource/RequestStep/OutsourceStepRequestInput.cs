using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Services.Manafacturing.Model.Outsource.RequestStep
{
    public class OutsourceStepRequestInput
    {
        public long ProductionOrderId { get; set; }
        public long OutsourceStepRequestFinishDate { get; set; }

        public ICollection<OutsourceStepRequestDetailInput> DetailInputs { get; set; }
        public ICollection<long> ProductionStepIds { get; set; }
    }

    public class OutsourceStepRequestDetailInput
    {
        public long ProductionStepLinkDataId { get; set; }
        public decimal Quantity { get; set; }
    }
}
