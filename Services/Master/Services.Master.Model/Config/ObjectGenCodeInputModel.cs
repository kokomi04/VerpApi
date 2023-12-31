﻿using System.ComponentModel.DataAnnotations;

namespace VErp.Services.Master.Model.Config
{
    public class ObjectGenCodeInputModel
    {
        //public int ObjectGenCodeId { get; set; }
        //public int ObjectTypeId { get; set; }
        //public string ObjectName { get; set; }
        public int CodeLength { get; set; }

        [StringLength(32)]
        public string Prefix { get; set; }

        [StringLength(32)]
        public string Suffix { get; set; }

        [StringLength(1)]
        public string Seperator { get; set; }
        //public string DateFormat { get; set; }
        //public int LastValue { get; set; }
        //public string LastCode { get; set; }
        //public bool IsActived { get; set; }
        //public bool IsDeleted { get; set; }
        //public int? UpdatedUserId { get; set; }
        //public DateTime? ResetDate { get; set; }
        //public DateTime? CreatedTime { get; set; }
        //public DateTime? UpdatedTime { get; set; }

    }
}
