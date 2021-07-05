using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.MasterEnum;

namespace VErp.Commons.GlobalObject.InternalDataInterface
{
    public class OutsideImportMappingObjectModel
    {
        public int OutsideImportMappingFunctionId { get; set; }
        public EnumObjectType ObjectTypeId { get; set; }
        public int InputTypeId { get; set; }
        public string SourceId { get; set; }
        public long InputBillFId { get; set; }
    }

    public class MappingObjectCreateRequest
    {
        public string MappingFunctionKey { get; set; }
        public string ObjectId { get; set; }
        public EnumObjectType BillObjectTypeId { get; set; }
        public long BillId { get; set; }
    }
}
