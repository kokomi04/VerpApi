using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Services.Master.Model.Config
{
    public class CustomGenCodeOutputModel
    {
        public int CustomGenCodeId { get; set; }
        public string CustomGenCodeName { get; set; }
        public int CodeLength { get; set; }
        public string Prefix { get; set; }
        public string Suffix { get; set; }
        public string Seperator { get; set; }
        public string LastCode { get; set; }
        public bool IsActived { get; set; }
        public int? UpdatedUserId { get; set; }
        public long CreatedTime { get; set; }
        public long UpdatedTime { get; set; }
        public string Description { get; set; }
        public int ObjectTypeId { get; set; }
        public int? ObjectId { get; set; }
        public string ObjectTypeName { get; set; }
        public string ObjectName { get; set; }
    }
}
