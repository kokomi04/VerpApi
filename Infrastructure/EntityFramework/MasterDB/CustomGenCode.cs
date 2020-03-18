using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.MasterDB
{
    public partial class CustomGenCode
    {
        public int CustomGenCodeId { get; set; }
        public string CustomGenCodeName { get; set; }
        public int CodeLength { get; set; }
        public string Prefix { get; set; }
        public string Suffix { get; set; }
        public string Seperator { get; set; }
        public string DateFormat { get; set; }
        public int LastValue { get; set; }
        public string LastCode { get; set; }
        public bool IsActived { get; set; }
        public bool IsDeleted { get; set; }
        public int? UpdatedUserId { get; set; }
        public DateTime? ResetDate { get; set; }
        public DateTime? CreatedTime { get; set; }
        public DateTime? UpdatedTime { get; set; }
        public string Description { get; set; }

        public int ObjectTypeId { get; set; }
        public int? ObjectId { get; set; }

        public string ObjectTypeName { get; set; }
        public string ObjectName { get; set; }
    }
}
