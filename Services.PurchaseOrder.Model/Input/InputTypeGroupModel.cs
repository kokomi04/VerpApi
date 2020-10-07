using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.PurchaseOrderDB;

namespace VErp.Services.PurchaseOrder.Model.Input
{
    public class InputTypeGroupModel : IMapFrom<InputTypeGroup>
    {
        public string InputTypeGroupName { get; set; }
        public int SortOrder { get; set; }
    }

    public class InputTypeGroupList : InputTypeGroupModel
    {
        public int InputTypeGroupId { get; set; }
    }
}
