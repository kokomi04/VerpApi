using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.MasterEnum;

namespace VErp.Services.Master.Model.Activity
{
    public class ActivityInput
    {
        public EnumObjectType ObjectTypeId { get; set; }
        public long ObjectId { get; set; }
        public string Message { get; set; }
        public string Data { get; set; }
    }
}
