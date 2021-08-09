

using System.Collections.Generic;
using VErp.Commons.GlobalObject.InternalDataInterface;

namespace VErp.Services.Manafacturing.Model.Outsource.RequestPart
{
    public class OutsourcePartRequestSearchModel
    {
        public long OutsourcePartRequestId { get; set; }
        public string OutsourcePartRequestCode { get; set; }
        public long CreatedDatetimeUtc { get; set; }
        public bool MarkInvalid { get; set; }
        public int OutsourcePartRequestStatusId { get; set; }
        public long ProductionOrderId { get; set; }
        public string ProductionOrderCode { get; set; }
        public string OrderCode { get; set; }
        public int RootProductId { get; set; }
        public string RootProductCode { get; set; }
        public string RootProductName { get; set; }
        public long ProductId { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public long? OutsourcePartRequestDetailFinishDate { get; set; }

        public IList<PurchaseOrderSimple> PurchaseOrder { get; set; }
    }
}