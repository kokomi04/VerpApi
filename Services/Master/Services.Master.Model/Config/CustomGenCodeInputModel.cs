using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace VErp.Services.Master.Model.Config
{
    public class CustomGenCodeInputModel
    {
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

        public int ObjectTypeId { get; set; }

        public int? ObjectId { get; set; }

        [StringLength(128)]
        public string ObjectName { get; set; }

        [StringLength(128)]
        public string ObjectTypeName { get; set; }

    }
}
