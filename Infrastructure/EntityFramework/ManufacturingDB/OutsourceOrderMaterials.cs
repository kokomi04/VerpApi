using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.ManufacturingDB
{
    public partial class OutsourceOrderMaterials
    {
        public long OutsourceOrderMaterialsId { get; set; }
        public long OutsourceOrderId { get; set; }
        public long ProductId { get; set; }
        public decimal Quantity { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
        public long? OutsourceRequestId { get; set; }
        public long? ProductionStepLinkDataId { get; set; }

        public virtual OutsourceOrder OutsourceOrder { get; set; }
    }
}
