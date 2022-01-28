using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.MasterDB
{
    public partial class OutsideDataFieldConfig
    {
        public int OutsideDataFieldConfigId { get; set; }
        public int OutsideDataConfigId { get; set; }
        public string Value { get; set; }
        public string Alias { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }

        public virtual OutSideDataConfig OutsideDataConfig { get; set; }
    }
}
