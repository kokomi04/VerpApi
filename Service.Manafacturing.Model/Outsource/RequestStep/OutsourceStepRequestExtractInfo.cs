using System.Collections.Generic;
using VErp.Commons.GlobalObject.InternalDataInterface.PurchaseOrder;

namespace VErp.Services.Manafacturing.Model.Outsource.RequestStep
{
    public class OutsourceStepRequestSearch
    {
        public long OutsourceStepRequestId { get; set; }
        public string OutsourceStepRequestCode { get; set; }
        public long OutsourceStepRequestFinishDate { get; set; }
        public long OutsourceStepRequestDate { get; set; }
        public long ProductionOrderId { get; set; }
        public string ProductionOrderCode { get; set; }
        public string OrderCode { get; set; }
        public string ProductionStepCollectionTitle { get; set; }
        public int OutsourceStepRequestStatusId { get; set; }
        public bool IsInvalid { get; set; }

        public IEnumerable<PurchaseOrderSimple> PurchaseOrder { get; set; }

    }
}
