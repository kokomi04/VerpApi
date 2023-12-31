﻿using System.ComponentModel;

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

        [Description("Tính khối lượng tinh")]
        ProductPurityCalc = 611,

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



        [Description("Xuất/nhập kho chi tiết")]
        InventoryDetail = 13,

        [Description("Kiện")]
        [GenCodeObject]
        Package = 14,

        [Description("Thuộc tính kiện")]
        [GenCodeObject]
        PackageCustomProperty = 14001,

        [Description("Sản phẩm kho")]
        StockProduct = 15,

        //[Description("Cấu hình sinh mã")]
        //GenCodeConfig = 16,

        [Description("Đối tác")]
        [GenCodeObject]
        Customer = 17,

        [Description("Danh mục đối tác")]
        CustomerCate = 170,

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

        [Description("Báo giá nhà cung cấp")]
        [GenCodeObject]
        PoProviderPricing = 25001,

        [Description("Đơn đặt hàng chi tiết")]
        PurchaseOrderDetail = 26,

        [Description("Cấu hình sinh mã")]
        CustomGenCodeConfig = 27,


        [Description("Thông tin doanh nghiệp")]
        BusinessInfo = 28,

        [Description("Bộ phận")]
        [GenCodeObject]
        Department = 29,

        //[Description("Phiếu xuất/nhập kho")]
        //Inventory = 12,
        [Description("Phiếu YC nhập kho")]
        [GenCodeObject]
        RequestInventoryInput = 300,

        [Description("Phiếu YC xuất kho")]
        [GenCodeObject]
        RequestInventoryOutput = 301,

        [Description("Phiếu nhập kho")]
        InventoryInput = 30,

        [Description("Phiếu xuất kho")]
        InventoryOutput = 31,

        [Description("Nhóm danh mục")]
        CategoryGroup = 320,

        [Description("Loại danh mục")]
        Category = 32,

        [Description("Dữ liệu danh mục")]
        CategoryData = 32001,

        [Description("Trường danh mục")]
        CategoryField = 33,

        [Description("Bộ lọc danh mục")]
        CategoryView = 1034,
        //AccountingAccount = 33,



        [Description("Loại CTGS")]
        InputType = 34,

        [Description("Loại CTGS biên thuế")]
        InputTypePublic = 34001,

        [Description("Nhóm CTGS")]
        InputTypeGroup = 35,

        [Description("Bộ lọc CTGS")]
        InputTypeView = 36,

        [Description("Dòng chứng từ ghi sổ")]
        InputTypeRow = 37,

        [Description("Dòng chứng từ ghi sổ biên thuế")]
        InputTypeRowPublic = 37001,

        [Description("Trường dữ liệu vùng CTGS")]
        InputAreaField = 38,

        [Description("Trường dữ liệu vùng CTGS biên thuế")]
        InputAreaFieldPublic = 38001,

        [Description("Chứng từ ghi sổ")]
        InputBill = 39,

        [Description("Chứng từ ghi sổ biên thuế")]
        InputBillPublic = 39001,


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

        [Description("Hướng dẫn")]
        Guide = 46001,
        [Description("Danh mục hướng dẫn")]
        GuideCate = 46002,

        [Description("Thiết lập sinh mã đối tượng")]
        ObjectCustomGenCodeMapping = 47,

        [Description("Thiết lập ánh xạ các chứng từ")]
        OutsideImportMappingFunction = 47001,

        [Description("Thông số hệ thống")]
        SystemParameter = 48,


        [Description("Chứng từ bán hàng")]
        VoucherBill = 49,

        [Description("Cấu hình phiếu in mặc định")]
        PrintConfigStandard = 50001,

        [Description("Cấu hình phiếu in tùy chỉnh")]
        PrintConfigCustom = 50002,

        [Description("Cấu hình header phiếu in mặc định")]
        PrintConfigHeaderStandard = 50003,

        [Description("Cấu hình header phiếu in tùy chỉnh")]
        PrintConfigHeaderCustom = 50004,

        [Description("CSDL")]
        StorageDabase = 51,

        [Description("Thiết lập dữ liệu (Chốt sổ)")]
        DataConfig = 52,


        [Description("Cấu hình nhà cung cấp hóa đơn điện tử")]
        ElectronicInvoiceProvider = 53000,
        [Description("Cấu hình mapping các trường dữ liệu hóa đơn điện tử")]
        ElectronicInvoiceMapping = 53001,
        [Description("Nhà cung cấp dịch vụ HDDT Easy invoice")]
        EasyInvoiceProvider = 53003,

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
        [Description("Yêu cầu gia công chi tiết")]
        [GenCodeObject]
        OutsourceRequestPart = 63,

        [Description("Yêu cầu gia công công đoạn")]
        [GenCodeObject]
        OutsourceRequestStep = 64,

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

        [Description("Phiếu Bàn giao công đoạn / Yêu cầu xuất kho")]
        ProductionHandoverReceipt = 7501,

        [Description("Lịch sử sản xuất")]
        ProductionHistory = 76,
        [Description("Nhân công sản xuất")]
        ProductionHumanResource = 79,

        //[Description("Khai báo nhân công và chi phí")]
        //ProductionScheduleTurnShift = 77,
        [Description("Yêu cầu nhập kho")]

        OutsourceTrack = 78,
        [Description("Khai báo vật tư tiêu hao")]
        ProductionConsumMaterial = 82,

        [Description("Tính toán nhu cầu VT của đơn hàng")]
        MaterialCalc = 85,

        [Description("Action")]
        ActionType = 100,

        [Description("ActionButton")]
        ActionButton = 101,
        [Description("Yêu cầu vật tư thêm")]
        [GenCodeObject]
        ProductionMaterialsRequirement = 102,
        [Description("Vật tư tiêu hao của mặt hàng")]
        ProductMaterialsConsumption = 321,
       
        [Description("Bán thành phẩm")]
        ProductSemi = 322,
        [Description("Bán thành phẩm chuyển đổi")]
        ProductSemiConversion = 323,
        [Description("Quy trình sản xuất mẫu")]
        ProductionProcessMold = 324,
        [Description("Nhóm vật tư tiêu hao")]
        ConsumptionGroup = 325,
        [Description("Thiết lập ngày làm việc")]
        WorkingDateConfig = 229,

        [Description("Kế hoạch sản xuất")]
        ProductionPlan = 400,
        [Description("Thuộc tính sản phầm")]
        Property = 401,
        [Description("Tính toán nhu cầu VT có thuộc tính đặc biệt của đơn hàng")]
        PropertyCalc = 402,


        [Description("Nội dung thường xuyên sử dụng")]
        ReuseContent = 501,
        [Description("Cập nhật tiến độ đơn hàng gia công")]
        PurchaseOrderTracked = 1035,

        [Description("Công thức tính giá mặt hàng")]
        ProductPriceConfig = 550,

        [Description("Phiên bản tính giá mặt hàng")]
        ProductPriceConfigVersion = 551,

        [Description("Tính giá mặt hàng")]
        ProductPriceInfo = 552,

        [Description("Kỳ kiểm kê")]
        [GenCodeObject]
        StockTakePeriod = 553,

        [Description("Phiếu kiểm kê")]
        [GenCodeObject]
        StockTake = 554,

        [Description("Phiếu xử lý kiểm kê")]
        [GenCodeObject]
        StockTakeAcceptanceCertificate = 555,

        [Description("Lịch làm việc")]
        Calendar = 600,

        [Description("Lịch nghỉ")]
        DayOffCalendar = 601,

        [Description("Lịch làm việc")]
        DepartmentCalendar = 602,

        [Description("Lịch nghỉ của bộ phận")]
        DepartmentDayOffCalendar = 603,

        [Description("Lịch tăng ca của bộ phận")]
        DepartmentOverHour = 604,


        [Description("Cấu hình nghỉ phép")]
        LeaveConfig = 60501,

        [Description("Đơn nghỉ phép")]
        LeaveBill = 60502,

        [Description("Nhóm chứng từ hành chính nhân sự")]
        HrTypeGroup = 1036,

        [Description("Loại chứng từ hành chính nhân sự")]
        HrType = 1037,

        [Description("Cấu hình chứng từ hành chính nhân sự")]
        HrTypeGlobalSetting = 1038,
        [Description("Trường dữ liệu vùng chứng từ hành chính nhân sự")]
        HrAreaField = 1039,
        
        [Description("Dòng chứng từ hành chính nhân sự")]
        HrTypeRow = 1040,

        [Description("Bộ lọc chứng từ hành chính nhân sự")]
        HrTypeView = 1041,

        [Description("Chứng từ hành chính nhân sự")]
        HrBill = 1042,

        [Description("Vùng thông tin chứng từ hành chính nhân sự")]
        HrArea = 1043,

        [Description("Cấu hình liên kết bảng lương")]
        SalaryRefTable = 10004200,

        [Description("Trường dữ liệu bảng lương")]
        SalaryField = 10004201,
        [Description("Loại bảng lương")]
        SalaryGroup = 10004202,
        [Description("Kỳ tính lương")]
        SalaryPeriod = 10004203,
        [Description("Bảng lương theo kỳ")]
        SalaryPeriodGroup = 10004204,
        [Description("Bảng lương nhân sự")]
        SalaryEmployee = 10004205,


        [Description("Trường Thưởng/phụ cấp và khấu trừ")]
        SalaryPeriodAdditionField = 10004206,

        [Description("Loại Thưởng/phụ cấp và khấu trừ")]
        SalaryPeriodAdditionType = 10004207,

        [Description("Chứng từ Thưởng/phụ cấp và khấu trừ")]
        SalaryPeriodAdditionBill = 10004208,

        [Description("Dữ liệu nháp")]
        DraftData = 1043,

        [Description("Thông tin thêm kế hoạch")]
        ProductionPlanExtraInfo = 1044,

        [Description("Phân bổ vật tư sản xuất")]
        MaterialAllocation = 1045,
        [Description("Cấu hình mail")]
        EmailConfiguration = 53004,
        [Description("Mẫu gửi mail")]
        MailTemplate = 53005,
        [Description("Cấu hình upload file")]
        FileConfiguration = 53006,
        Notification = 53007,
        [Description("Thiết lập thông số sắp xếp giờ")]
        TimeSortConfiguration = 53008,
        [Description("Thiết lập lịch trình làm việc")]
        WorkSchedule = 53009,
        [Description("Thiết lập ca làm việc")]
        ShiftConfiguration = 53010,
        [Description("Nhóm biểu đồ báo cáo")]
        DashboardTypeGroup = 60503,
        [Description("Biểu đồ báo cáo")]
        DashboardType = 60504,
        [Description("Bộ lọc biểu đồ báo cáo")]
        DashboardTypeView = 60505
    }
}
