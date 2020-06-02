using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.MasterDB
{
    public partial class ObjectProcessType
    {
        public int ObjectProcessTypeId { get; set; }
        public string ObjectProcessTypeName { get; set; }
        public string ObjectProcessTypeDescription { get; set; }
    }
}
