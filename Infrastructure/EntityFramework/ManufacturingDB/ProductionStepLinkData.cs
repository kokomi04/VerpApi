﻿using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.ManufacturingDB
{
    public partial class ProductionStepLinkData
    {
        public ProductionStepLinkData()
        {
            OutsourceStepRequestData = new HashSet<OutsourceStepRequestData>();
            ProductionStepLinkDataRole = new HashSet<ProductionStepLinkDataRole>();
        }

        public long ProductionStepLinkDataId { get; set; }
        public string ProductionStepLinkDataCode { get; set; }
        public long ObjectId { get; set; }
        public int ObjectTypeId { get; set; }
        public int UnitId { get; set; }
        public decimal Quantity { get; set; }
        public int SortOrder { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int CreatedByUserId { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }
        public int SubsidiaryId { get; set; }
        public int ProductId { get; set; }

        public virtual ICollection<OutsourceStepRequestData> OutsourceStepRequestData { get; set; }
        public virtual ICollection<ProductionStepLinkDataRole> ProductionStepLinkDataRole { get; set; }
    }
}
