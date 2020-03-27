using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VErp.Commons.Enums.MasterEnum;

namespace VErp.Services.Organization.Model.BusinessInfo

{
    public class BusinessInfoModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên doanh nghiệp")]
        [MaxLength(128, ErrorMessage = "Tên doanh nghiệp quá dài")]
        public string CompanyName { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập tên người đại diện")]
        [MaxLength(128, ErrorMessage = "Tên người đại diện quá dài")]
        public string LegalRepresentative { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập địa chỉ doanh nghiệp")]
        [MaxLength(128, ErrorMessage = "Địa chỉ doanh nghiệp quá dài")]
        public string Address { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập mã số thuế")]
        [MaxLength(64, ErrorMessage = "Mã số thuế quá dài")]
        public string TaxIdNo { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [MaxLength(32, ErrorMessage = "Số điện thoại quá dài")]
        public string PhoneNumber { get; set; }
        [MaxLength(128, ErrorMessage = "Website quá dài")]
        public string Website { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập email")]
        [MaxLength(128, ErrorMessage = "Email quá dài")]
        public string Email { get; set; }
        public int? LogoFileId { get; set; }
    }
}
