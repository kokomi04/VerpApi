using System;
using System.Collections.Generic;

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
    }
}
