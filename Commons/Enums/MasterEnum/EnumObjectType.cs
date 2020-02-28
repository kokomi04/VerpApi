using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Commons.Enums.MasterEnum
{
    public enum EnumObjectType
    {
        UserAndEmployee = 1,
        Role = 2,
        RolePermission = 3,
        ProductCate = 4,
        ProductType = 5,
        Product = 6,
        Unit = 7,
        BarcodeConfig = 8,
        /// <summary>
        /// Kho         
        /// </summary>
        Stock = 9,
        File = 10,
        /// <summary>
        /// Vị trí trong Kho         
        /// </summary>
        Location = 11,

        /// <summary>
        /// Phiếu nhập/xuất kho
        /// </summary>
        Inventory = 12,

        /// <summary>
        /// Phiếu nhập/xuất kho chi tiết
        /// </summary>
        InventoryDetail = 13,

        /// <summary>
        /// Gói / kiện
        /// </summary>
        Package = 14,

        /// <summary>
        /// Gói / kiện
        /// </summary>
        StockProduct = 15,

        GenCodeConfig = 16,

        Customer = 17,

        /// <summary>
        /// BOM - Bill of Material - Thông tin vật tư cấu thành nên sản phẩm
        /// </summary>
        ProductBom = 18,

        PurchasingRequest = 19,

        PurchasingRequestDetail = 20

    }
}
