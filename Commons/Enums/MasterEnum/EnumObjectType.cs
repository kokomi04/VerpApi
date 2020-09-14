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

        /// <summary>
        /// Yêu cầu VT HH
        /// </summary>
        PurchasingRequest = 19,

        PurchasingRequestDetail = 20,

        /// <summary>
        /// Đề nghị mua VT HH
        /// </summary>
        PurchasingSuggest = 21,

        PurchasingSuggestDetail = 22,

        PoAssignment = 23,
        PoAssignmentDetail = 24,

        /// <summary>
        /// PO - Đơn đặt hàng
        /// </summary>
        PurchaseOrder = 25,
        PurchaseOrderDetail = 26,

        /// <summary>
        /// Cấu hình mã tự sinh tùy chọn
        /// </summary>
        CustomGenCodeConfig = 27,
        /// <summary>
        /// 
        /// </summary>
        BusinessInfo = 28,
        /// <summary>
        /// 
        /// </summary>
        Department = 29,
        InventoryInput = 30,
        InventoryOutput = 31,
        Category = 32,
        AccountingAccount = 33,
        InputType = 34,

        InputTypeGroup = 35,
        InputTypeView = 36,

        InputTypeRow = 37,

        Subsidiary = 41,

        ObjectProcessStep = 42,

        ReportTypeGroup = 43,

        ReportTypeView = 44,
        ReportType = 45,
        Menu = 46,

        ObjectCustomGenCodeMapping = 47,
        SystemParameter = 48,
        AccountantConfig = 49,
        PrintConfig = 50
    }
}
