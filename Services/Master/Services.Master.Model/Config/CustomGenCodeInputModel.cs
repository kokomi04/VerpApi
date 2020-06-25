using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace VErp.Services.Master.Model.Config
{
    public class CustomGenCodeInputModel
    {
        public int? ParentId { get; set; }
        public int CodeLength { get; set; }

        [StringLength(128)]
        public string CustomGenCodeName { get; set; }

        [StringLength(32)]
        public string Prefix { get; set; }

        [StringLength(32)]
        public string Suffix { get; set; }

        [StringLength(1)]
        public string Seperator { get; set; }

        [StringLength(128)]
        public string Description { get; set; }

        public int LastValue { get; set; }
        public int SortOrder { get; set; }
    }
}
