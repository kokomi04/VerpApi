using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.GlobalObject.InternalDataInterface.Manufacturing;

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

    public class ProductionOrderCalcStatusV2Message
    {

        public string ProductionOrderCode { get; set; }     
        public IList<InventoryRequireDetailByProductionOrderModel> InvRequireDetails { get; set; }
        public IList<InventoryDetailByProductionOrderModel> InvDetails { get; set; }
        public string Description { get; set; }
        public ProductionOrderCalcStatusV2Message()
        {
            InvRequireDetails = new List<InventoryRequireDetailByProductionOrderModel>();
            InvDetails = new List<InventoryDetailByProductionOrderModel>();
        }

    }
}
