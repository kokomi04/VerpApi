using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Commons.GlobalObject.InternalDataInterface
{
    public class CustomGenCodeOutputModel
    {
        public int CustomGenCodeId { get; set; }
        public int? ParentId { get; set; }
        public string CustomGenCodeName { get; set; }
        public int CodeLength { get; set; }
        //public string Prefix { get; set; }
        //public string Suffix { get; set; }
        //public string Seperator { get; set; }
        public string LastCode { get; set; }
        public bool IsActived { get; set; }
        public int? UpdatedUserId { get; set; }
        public long CreatedTime { get; set; }
        public long UpdatedTime { get; set; }
        public string Description { get; set; }
        public int SortOrder { get; set; }

        public bool IsDefault { get; set; }
        public string BaseFormat { get; set; }
        public string CodeFormat { get; set; }

        public IList<CustomGenCodeBaseValueModel> LastValues { get; set; }
        public CustomGenCodeBaseValueModel CurrentLastValue { get; set; }
    }

    public class CustomGenCodeBaseValueModel
    {
        public int CustomGenCodeId { get; set; }
        public string BaseValue { get; set; }
        public int LastValue { get; set; }
        public string LastCode { get; set; }
    }
}
