using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.MasterEnum;

namespace VErp.Commons.GlobalObject.QueueMessage
{
    public class ProductionOrderStatusInventorySumaryMessage
    {
        public string ProductionOrderCode { get; set; }
        public string Description { get; set; }
    }
}
