using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Services.Stock.Model.Inventory
{
    public class ApprovedInputDataSubmitModel
    {
        public InventoryInModel Inventory { get; set; }
        public IList<CensoredInventoryInputProducts> AffectDetails { get; set; }


    }
}
