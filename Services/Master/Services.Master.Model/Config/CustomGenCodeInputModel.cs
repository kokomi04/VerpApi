using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Infrastructure.EF.MasterDB;

namespace VErp.Services.Master.Model.Config
{
    public class CustomGenCodeInputModel : IMapFrom<CustomGenCode>
    {
        public int? ParentId { get; set; }
        public int CodeLength { get; set; }

        [StringLength(128)]
        public string CustomGenCodeName { get; set; }

        [StringLength(128)]
        public string BaseFormat { get; set; }

        [StringLength(128)]
        public string CodeFormat { get; set; }

        [StringLength(32)]
        public string Prefix { get; set; }

        [StringLength(32)]
        public string Suffix { get; set; }

        [StringLength(128)]
        public string Description { get; set; }

        public int SortOrder { get; set; }

        public bool IsDefault { get; set; }

        public IList<CustomGenCodeBaseValueModel> LastValues { get; set; }
      
    }

}
