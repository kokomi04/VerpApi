using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Services.Accountancy.Model.OutsideMapping
{
    public class OutsideImportMappingObjectModel
    {
        public int OutsideImportMappingFunctionId { get; set; }
        public int InputTypeId { get; set; }
        public string SourceId { get; set; }
        public long InputBillFId { get; set; }
    }
}
