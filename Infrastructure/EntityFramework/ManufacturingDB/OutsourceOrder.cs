using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.ManufacturingDB
{
    public partial class OutsourceOrder
    {
        public OutsourceOrder()
        {
            OutsourceOrderDetail = new HashSet<OutsourceOrderDetail>();
            OutsourceOrderExcess = new HashSet<OutsourceOrderExcess>();
            OutsourceOrderMaterials = new HashSet<OutsourceOrderMaterials>();
            OutsourceTrack = new HashSet<OutsourceTrack>();
        }

        public long OutsourceOrderId { get; set; }
        public int OutsourceTypeId { get; set; }
        public string OutsourceOrderCode { get; set; }
        public DateTime OutsourceOrderDate { get; set; }
        public DateTime OutsourceOrderFinishDate { get; set; }
        public string OutsourceRequired { get; set; }
        public string Note { get; set; }
        public decimal FreightCost { get; set; }
        public decimal OtherCost { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }
        public int SubsidiaryId { get; set; }
        public int? CustomerId { get; set; }
        public string DeliveryDestination { get; set; }
        public string Suppliers { get; set; }
        public long? AttachmentFileId { get; set; }
        public string ExcessMaterialNotes { get; set; }

        public virtual ICollection<OutsourceOrderDetail> OutsourceOrderDetail { get; set; }
        public virtual ICollection<OutsourceOrderExcess> OutsourceOrderExcess { get; set; }
        public virtual ICollection<OutsourceOrderMaterials> OutsourceOrderMaterials { get; set; }
        public virtual ICollection<OutsourceTrack> OutsourceTrack { get; set; }
    }
}
