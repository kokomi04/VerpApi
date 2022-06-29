using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VErp.Commons.Library.Model;

namespace VErp.Services.Stock.Model.Package
{


    public class PackageInputModel
    {

        [Display(Name = "(Kiện) - Mã kiện", GroupName = "Thông tin kiện")]
        public string PackageCode { get; set; }
        [Display(Name = "(Kiện) - Vị trí", GroupName = "Thông tin kiện")]
        public int? LocationId { get; set; }
        [Display(Name = "(Kiện) - Hạn sử dụng", GroupName = "Thông tin kiện")]
        public long ExpiryTime { get; set; }
        [Display(Name = "(Kiện) - Mô tả", GroupName = "Thông tin kiện")]
        public string Description { get; set; }
        [Display(Name = "(Kiện) - Mã đơn hàng", GroupName = "Thông tin kiện")]
        public string OrderCode { get; set; }
        [Display(Name = "(Kiện) - Mã đơn mua", GroupName = "Thông tin kiện")]
        public string POCode { get; set; }
        [Display(Name = "(Kiện) - Mã lệnh sản xuất", GroupName = "Thông tin kiện")]
        public string ProductionOrderCode { get; set; }

        [FieldDataIgnore]
        [Display(Name = "(Kiện) - Thuộc tính khác", GroupName = "Thông tin kiện")]
        public IDictionary<int, object> CustomPropertyValue { get; set; }
    }
}
