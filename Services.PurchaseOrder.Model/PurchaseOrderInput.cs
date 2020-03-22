using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Services.PurchaseOrder.Model
{
    public class PurchaseOrderInput
    {
        public IList<long> PoAssignmentDetailIds { get; set; }
        public long Date { get; set; }
        public string PurchaseOrderCode { get; set; }
        public DeliveryDestinationModel DeliveryDestination { get; set; }
        public string Content { get; set; }
        public string AdditionNote { get; set; }
        public decimal DeliveryFee { get; set; }
        public decimal OtherFee { get; set; }
        public decimal TotalMoney { get; set; }
    }

    public class DeliveryDestinationModel
    {
        public string DeliverTo { get; set; }
        public string Company { get; set; }
        public string Address { get; set; }
        public string Telephone { get; set; }
        public string Fax { get; set; }
        public string AdditionNote { get; set; }
    }
}
