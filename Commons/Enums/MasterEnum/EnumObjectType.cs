using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace VErp.Commons.Enums.MasterEnum
{
    public enum EnumObjectType
    {

        [Description("Nhân viên")]
        [GenCodeObject]
        UserAndEmployee = 1,

        [Description("Nhóm quyền")]
        Role = 2,
        [Description("Phân quyền")]
        RolePermission = 3,
        [Description("Danh mục mặt hàng")]
        ProductCate = 4,
        [Description("Loại mặt hàng")]
        ProductType = 5,
        [Description("Mặt hàng")]
        Product = 6,
        [Description("Đơn vị tính")]
        Unit = 7,
        [Description("Cấu hình barcode")]
        BarcodeConfig = 8,
        [Description("Kho chứa")]
        Stock = 9,
        [Description("File")]
        File = 10,

        [Description("Vị trí trong kho")]
        [GenCodeObject]
        Location = 11,

        //[Description("Phiếu xuất/nhập kho")]
        //Inventory = 12,
        [Description("Phiếu YC nhập kho")]
        RequestInventoryInput = 300,

        [Description("Phiếu YC xuất kho")]
        RequestInventoryOutput = 301,

        [Description("Phiếu nhập kho")]
        InventoryInput = 30,

        [Description("Phiếu xuất kho")]
        InventoryOutput = 31,

        [Description("Xuất/nhập kho chi tiết")]
        InventoryDetail = 13,

        [Description("Kiện")]
        [GenCodeObject]
        Package = 14,

        [Description("Sản phẩm kho")]
        StockProduct = 15,

        //[Description("Cấu hình sinh mã")]
        //GenCodeConfig = 16,

        [Description("Đối tác")]
        [GenCodeObject]
        Customer = 17,

        [Description("BOM")]
        ProductBom = 18,

        [Description("Phiếu YC VTHH")]
        [GenCodeObject]
        PurchasingRequest = 19,

        [Description("YC VTHH chi tiết")]
        PurchasingRequestDetail = 20,

        [Description("Phiếu đề nghị VTHH")]
        [GenCodeObject]
        PurchasingSuggest = 21,

        [Description("Đề nghị VTHH chi tiết")]
        PurchasingSuggestDetail = 22,

        [Description("Phân công mua hàng")]
        [GenCodeObject]
        PoAssignment = 23,

        [Description("Phân công mua hàng chi tiết")]
        PoAssignmentDetail = 24,

        [Description("Đơn đặt hàng")]
        [GenCodeObject]
        PurchaseOrder = 25,

        [Description("Đơn đặt hàng chi tiết")]
        PurchaseOrderDetail = 26,

        [Description("Cấu hình sinh mã")]
        CustomGenCodeConfig = 27,


        [Description("Thông tin doanh nghiệp")]
        BusinessInfo = 28,

        [Description("Bộ phận")]
        [GenCodeObject]
        Department = 29,


        [Description("Nhóm danh mục")]
        CategoryGroup = 31,

        [Description("Loại danh mục")]
        Category = 32,

        [Description("Trường danh mục")]
        CategoryField = 33,


        //AccountingAccount = 33,



        [Description("Loại CTGS")]
        InputType = 34,

        [Description("Nhóm CTGS")]
        InputTypeGroup = 35,

        [Description("Bộ lọc CTGS")]
        InputTypeView = 36,

        [Description("Dòng chứng từ ghi sổ")]
        InputTypeRow = 37,

        [Description("Trường dữ liệu vùng CTGS")]
        InputAreaField = 38,

        [Description("Chứng từ ghi sổ")]
        InputBill = 39,

        [Description("Công ty")]
        Subsidiary = 41,

        [Description("Quy trình xử lý")]
        ObjectProcessStep = 42,

        [Description("Nhóm báo cáo")]
        ReportTypeGroup = 43,

        [Description("Loại báo cáo")]
        ReportType = 45,

        [Description("Bộ lọc báo cáo")]
        ReportTypeView = 44,

        [Description("Menu")]
        Menu = 46,

        [Description("Thiết lập sinh mã đối tượng")]
        ObjectCustomGenCodeMapping = 47,

        [Description("Thông số hệ thống")]
        SystemParameter = 48,


        [Description("Chứng từ bán hàng")]
        VoucherBill = 49,

        [Description("Cấu hình in")]
        PrintConfig = 50,

        [Description("CSDL")]
        StorageDabase = 51,

        [Description("Thiết lập dữ liệu (Chốt sổ)")]
        DataConfig = 52,

        [Description("Loại chứng từ bán hàng")]
        VoucherType = 53,

        [Description("Nhóm chứng từ bán hàng")]
        VoucherTypeGroup = 54,

        [Description("Bộ lọc chứng từ bán hàng")]
        VoucherTypeView = 55,

        [Description("Dòng chứng từ bán hàng")]
        VoucherTypeRow = 56,

        [Description("Nút chức năng chứng từ bán hàng")]
        VoucherAction = 57,

        [Description("Nút chức năng CTGS")]
        InputAction = 58,

        [Description("Trường dữ liệu vùng chứng từ bán hàng")]
        VoucherAreaField = 59,

        [Description("Công đoạn sản xuất")]
        ProductionStep = 60,
        [Description("Danh mục công đoạn")]
        Step = 61,
        [Description("Nhóm danh mục công đoạn")]
        StepGroup = 62,
        [Description("Yêu cầu gia công")]
        [GenCodeObject]
        OutsourceRequest = 63,
        [Description("Lệnh sản xuất")]
        [GenCodeObject]
        ProductionOrder = 70,
        [Description("Kế hoạch sản xuất")]
        [GenCodeObject]
        ProductionSchedule = 71,
        [Description("Đơn hàng gia công")]
        [GenCodeObject]
        OutsourceOrder = 72,
        [Description("Phân công sản xuất")]
        ProductionAssignment = 73,
        ProductionProcess = 74,
        [Description("Bàn giao công đoạn / Yêu cầu xuất kho")]
        ProductionHandover = 75,

        [Description("Khai báo nhân công và chi phí")]
        ProductionScheduleTurnShift = 77,
        [Description("Yêu cầu nhập kho")]
        [GenCodeObject]
        InventoryInputRequirement = 80,
        [Description("Yêu cầu xuất kho")]
        [GenCodeObject]
        InventoryOutputRequirement = 81,
        OutsourceTrack = 78,
        [Description("Khai báo vật tư tiêu hao")]
        ProductionConsumMaterial = 82,

        [Description("Action")]
        ActionType = 100,

        [Description("ActionButton")]
        ActionButton = 101,
        ProductionMaterialsRequirement = 102
    }
}
