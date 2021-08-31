using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.OrganizationDB
{
    public partial class ObjectProcessObject
    {
        public long ObjectProcessObjectId { get; set; }
        public long ObjectId { get; set; }
        public int ObjectProcessTypeId { get; set; }
        public int ObjectProcessStepId { get; set; }
        public int UserId { get; set; }
        public DateTime CensoredDatetimeUtc { get; set; }
        public bool IsApproved { get; set; }
        public string Note { get; set; }
    }
}
