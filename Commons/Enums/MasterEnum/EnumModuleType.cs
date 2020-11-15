using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace VErp.Commons.Enums.MasterEnum
{
    public enum EnumModuleType
    {
        [Description("Phân hệ chung")]
        Master = 1,
        [Description("Kho")]
        Stock = 2,
        [Description("Cơ cấu, tổ chức")]
        Organization = 3,
        [Description("Mua, bán hàng")]
        PurchaseOrder = 4,
        [Description("Kế toán")]
        Accountant = 5,
        [Description("Sản xuất")]
        Manufacturing = 6,
    }

}
