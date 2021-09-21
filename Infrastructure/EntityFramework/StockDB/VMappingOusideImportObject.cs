using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.StockDB
{
    public partial class VMappingOusideImportObject
    {
        public int OutsideImportMappingFunctionId { get; set; }
        public string MappingFunctionKey { get; set; }
        public int InputTypeId { get; set; }
        public string SourceId { get; set; }
        public long InputBillFId { get; set; }
        public int? BillObjectTypeId { get; set; }
    }
}
