using System.ComponentModel;

namespace VErp.Commons.GlobalObject.InternalDataInterface
{
    public class ObjectDataTemplateMail
    {
        [Description("Domain")]
        public string Domain { get; set; }

        [Description("Mã số định danh")]
        public long F_Id { get; set; }


        [Description("Người tạo")]
        public string CreatedByUser { get; set; }
        [Description("Người chỉnh sửa")]
        public string UpdatedByUser { get; set; }

        [Description("Người kiểm tra")]
        public string CheckedByUser { get; set; }

        [Description("Người duyệt")]
        public string CensoredByUser { get; set; }

        [Description("Tên công ty")]
        public string CompanyName { get; set; }
        [Description("Giá bán")]
        public string Price { get; set; }
        [Description("Số lượng")]
        public string Quantity { get; set; }
        [Description("Tỉ lệ")]
        public string Rate { get; set; }
        [Description("Mã code")]
        public string Code { get; set; }
        [Description("Tên mặt hàng")]
        public string ProductName { get; set; }
        [Description("Mã mặt hàng")]
        public string ProductCode { get; set; }
        [Description("Số tiền bằng chữ")]
        public string MoneyInWords { get; set; }
        [Description("Tiền tệ")]
        public string Currency { get; set; }
        [Description("Tổng tiền")]
        public string TotalMoney { get; set; }
    }

    public class TemplateMailField
    {
        public string Title { get; set; }
        public string FieldName { get; set; }
    }
}