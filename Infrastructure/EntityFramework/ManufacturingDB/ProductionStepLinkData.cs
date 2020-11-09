using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.ManufacturingDB
{
    public partial class ProductionStepLinkData
    {
        public ProductionStepLinkData()
        {
            ProductionStepLinkDataRole = new HashSet<ProductionStepLinkDataRole>();
            RequestOutsourcePartDetail = new HashSet<RequestOutsourcePartDetail>();
        }

        public long ProductionStepLinkDataId { get; set; }
        public int ProductId { get; set; }
        public int ProductUnitConversionId { get; set; }
        public decimal Quantity { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int CreatedByUserId { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }
        public int SortOrder { get; set; }

        public virtual ICollection<ProductionStepLinkDataRole> ProductionStepLinkDataRole { get; set; }
        public virtual ICollection<RequestOutsourcePartDetail> RequestOutsourcePartDetail { get; set; }
    }
}
