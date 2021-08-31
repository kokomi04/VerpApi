using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.AccountancyDB
{
    public partial class OutsideImportMappingObject
    {
        public int OutsideImportMappingFunctionId { get; set; }
        public string SourceId { get; set; }
        public long InputBillFId { get; set; }

        public virtual OutsideImportMappingFunction OutsideImportMappingFunction { get; set; }
    }
}
