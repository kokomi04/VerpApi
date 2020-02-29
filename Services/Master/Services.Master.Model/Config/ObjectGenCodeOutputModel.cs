using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Services.Master.Model.Config
{
    public class ObjectGenCodeOutputModel
    {
        public int ObjectGenCodeId { get; set; }
        public int ObjectTypeId { get; set; }
        public string ObjectTypeName { get; set; }
        public int CodeLength { get; set; }
        public string Prefix { get; set; }
        public string Suffix { get; set; }
        public string Seperator { get; set; }
        //public string DateFormat { get; set; }
        //public int LastValue { get; set; }
        public string LastCode { get; set; }
        public bool IsActived { get; set; }
        //public bool IsDeleted { get; set; }
        public int? UpdatedUserId { get; set; }
        //public DateTime? ResetDate { get; set; }
        public long CreatedTime { get; set; }
        public long UpdatedTime { get; set; }
    }
}
