using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.MasterEnum;

namespace VErp.Services.Master.Model.Config
{
    public class ObjectGenCodeMapping
    {
        public int ObjectCustomGenCodeMappingId { get; set; }
        public EnumObjectType ObjectTypeId { get; set; }
        public EnumObjectType TargetObjectTypeId { get; set; }
        public EnumObjectType ConfigObjectTypeId { get; set; }
        public long ConfigObjectId { get; set; }

        public int CustomGenCodeId { get; set; }
    }

    public class ObjectGenCodeMappingTypeModel
    {
        public int? ObjectCustomGenCodeMappingId { get; set; }
        public EnumModuleType ModuleTypeId { get; set; }
        public string ModuleTypeName { get; set; }

        public EnumObjectType TargetObjectTypeId { get; set; }
        public string TargetObjectTypeName { get; set; }

        public EnumObjectType ObjectTypeId { get; set; }
        public string ObjectTypeName { get; set; }
     
        public EnumObjectType ConfigObjectTypeId { get; set; }
        public long ConfigObjectId { get; set; }
        public string TargetObjectName { get; set; }

        public string FieldName { get; set; }
        public int? CustomGenCodeId { get; set; }
        public string CustomGenCodeName { get; set; }
    }
}
