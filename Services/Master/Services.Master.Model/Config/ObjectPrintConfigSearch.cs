using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.MasterEnum;

namespace VErp.Services.Master.Model.Config
{
    public class ObjectPrintConfigSearch: ObjectPrintConfig
    {
        public EnumModuleType ModuleTypeId { get; set; }
        public string ModuleTypeName { get; set; }
        public string ObjectTypeName { get; set; }
        public string ObjectTitle { get; set; }
    }
}
