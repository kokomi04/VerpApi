using System.Collections.Generic;

namespace VErp.Services.Stock.Model.Inventory
{
    public class ApprovedInputDataSubmitModel
    {
        public InventoryInModel Inventory { get; set; }
        public IList<CensoredInventoryInputProducts> AffectDetails { get; set; }


    }
}
