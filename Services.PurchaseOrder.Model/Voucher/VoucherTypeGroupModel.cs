using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.PurchaseOrderDB;

namespace VErp.Services.PurchaseOrder.Model.Voucher
{
    public class VoucherTypeGroupModel : IMapFrom<VoucherTypeGroup>
    {
        public string VoucherTypeGroupName { get; set; }
        public int SortOrder { get; set; }
    }

    public class VoucherTypeGroupList : VoucherTypeGroupModel
    {
        public int VoucherTypeGroupId { get; set; }
    }
}
