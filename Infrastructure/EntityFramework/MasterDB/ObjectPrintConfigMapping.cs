﻿using System;

#nullable disable

namespace VErp.Infrastructure.EF.MasterDB
{
    public partial class ObjectPrintConfigMapping
    {
        public int PrintConfigCustomId { get; set; }
        public int ObjectTypeId { get; set; }
        public int ObjectId { get; set; }
        public int UpdateByUserId { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
    }
}
