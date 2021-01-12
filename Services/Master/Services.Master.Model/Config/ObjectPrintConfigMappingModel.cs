using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.MasterDB;

namespace VErp.Services.Master.Model.Config
{
    public class ObjectPrintConfigMappingModel : IMapFrom<ObjectPrintConfigMapping>
    {
        public int PrintConfigId { get; set; }
        public EnumObjectType ObjectTypeId { get; set; }
        public int ObjectId { get; set; }
    }
    public class ObjectPrintConfig
    {
        public EnumObjectType ObjectTypeId { get; set; }
        public int ObjectId { get; set; }
        public int[] PrintConfigIds { get; set; }
    }
}
