using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.ManufacturingDB
{
    public partial class OutsourceOrder
    {
        public OutsourceOrder()
        {
            OutsourceOrderDetail = new HashSet<OutsourceOrderDetail>();
        }

        public int OutsoureOrderId { get; set; }
        public int RequestoObjectId { get; set; }
        public int RequestObjectTypeId { get; set; }
        public DateTime CreateDateOrder { get; set; }
        public string OutsoureOrderCode { get; set; }
        public string ProviderName { get; set; }
        public string ProviderReceiver { get; set; }
        public string ProviderAddress { get; set; }
        public string ProviderPhone { get; set; }
        public string TransportToReceiver { get; set; }
        public string TransportToCompany { get; set; }
        public string TransportToAdress { get; set; }
        public string TransportToPhone { get; set; }
        public string OutsoureRequired { get; set; }
        public string Note { get; set; }
        public decimal FreigthCost { get; set; }
        public decimal OtherCost { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }

        public virtual ICollection<OutsourceOrderDetail> OutsourceOrderDetail { get; set; }
    }
}
