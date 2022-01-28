using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Verp.Resources.Enums.PO;
using VErp.Commons.ObjectExtensions.CustomAttributes;

namespace VErp.Commons.Enums.MasterEnum.PO
{
    [LocalizedDescription(ResourceType = typeof(EnumPurchasingRequestTypeDescription))]
    public enum EnumPurchasingRequestType
    {
        Normal = 0,
        OrderMaterial = 1,
        MaterialCalc = 2,
        ProductionOrderMaterialCalc = 3,
    }
}
