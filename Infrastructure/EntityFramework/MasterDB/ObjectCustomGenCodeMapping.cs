using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.MasterDB
{
    public partial class ObjectCustomGenCodeMapping
    {
        public int ObjectCustomGenCodeMappingId { get; set; }
        public int ObjectTypeId { get; set; }
        public int ObjectId { get; set; }
        public int CustomGenCodeId { get; set; }
        public int UpdatedByUserId { get; set; }
        public int SubsidiaryId { get; set; }
        public int TargetObjectTypeId { get; set; }
        public int ConfigObjectTypeId { get; set; }
        public long ConfigObjectId { get; set; }
    }
}
