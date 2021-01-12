using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.MasterDB
{
    public partial class OutsideImportMappingObject
    {
        public int OutsideImportMappingFunctionId { get; set; }
        public string SourceId { get; set; }
        public long InputBillFId { get; set; }
        public int BillObjectTypeId { get; set; }

        public virtual OutsideImportMappingFunction OutsideImportMappingFunction { get; set; }
    }
}
