using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.MasterEnum;

namespace VErp.Infrastructure.ServiceCore.Model
{
    public class FileAssignToObjectInput
    {
        public EnumObjectType ObjectTypeId { get; set; }
        public long ObjectId { get; set; }
    }
}
