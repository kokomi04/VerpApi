using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.AccountancyDB
{
    public partial class OutsideImportMappingFunction
    {
        public OutsideImportMappingFunction()
        {
            OutsideImportMapping = new HashSet<OutsideImportMapping>();
            OutsideImportMappingObject = new HashSet<OutsideImportMappingObject>();
        }

        public int OutsideImportMappingFunctionId { get; set; }
        public string MappingFunctionKey { get; set; }
        public string FunctionName { get; set; }
        public string Description { get; set; }
        public bool IsWarningOnDuplicated { get; set; }
        public string SourceDetailsPropertyName { get; set; }
        public string DestinationDetailsPropertyName { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }

        public virtual ICollection<OutsideImportMapping> OutsideImportMapping { get; set; }
        public virtual ICollection<OutsideImportMappingObject> OutsideImportMappingObject { get; set; }
    }
}
