using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.ManufacturingDB
{
    public partial class ProductionStepLinkData
    {
        public ProductionStepLinkData()
        {
            OutsourceStepRequestData = new HashSet<OutsourceStepRequestData>();
            ProductionAssignment = new HashSet<ProductionAssignment>();
            ProductionStepLinkDataRole = new HashSet<ProductionStepLinkDataRole>();
        }

        public long ProductionStepLinkDataId { get; set; }
        public string ProductionStepLinkDataCode { get; set; }
        public int ProductionStepLinkDataTypeId { get; set; }
        public long ObjectId { get; set; }
        public int ObjectTypeId { get; set; }
        public decimal Quantity { get; set; }
        public decimal QuantityOrigin { get; set; }
        public decimal? OutsourceQuantity { get; set; }
        public decimal? ExportOutsourceQuantity { get; set; }
        public decimal? OutsourcePartQuantity { get; set; }
        public int SortOrder { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int CreatedByUserId { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }
        public int SubsidiaryId { get; set; }
        public long? OutsourceRequestDetailId { get; set; }
        public int ProductionStepLinkTypeId { get; set; }
        public long? ConverterId { get; set; }
        public decimal WorkloadConvertRate { get; set; }

        public virtual ICollection<OutsourceStepRequestData> OutsourceStepRequestData { get; set; }
        public virtual ICollection<ProductionAssignment> ProductionAssignment { get; set; }
        public virtual ICollection<ProductionStepLinkDataRole> ProductionStepLinkDataRole { get; set; }
    }
}
