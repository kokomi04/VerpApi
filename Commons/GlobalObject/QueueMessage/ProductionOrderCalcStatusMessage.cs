using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.GlobalObject.InternalDataInterface;

namespace VErp.Commons.GlobalObject.QueueMessage
{
    public class ProductionOrderCalcStatusMessage
    {

        public string ProductionOrderCode { get; set; }     
        public IList<InternalProductionInventoryRequirementModel> Inventories { get; set; }

        public string Description { get; set; }
        public ProductionOrderCalcStatusMessage()
        {
            Inventories = new List<InternalProductionInventoryRequirementModel>();
        }

    }
}
