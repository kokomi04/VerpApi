using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VErp.Commons.Enums.MasterEnum;

namespace VErp.Services.Master.Model.BusinessInfo

{
    public class BusinessInfoModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên doanh nghiệp")]
        [MaxLength(128, ErrorMessage = "Tên doanh nghiệp quá dài")]
        public string CompanyName { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập tên người đại diện")]
        [MaxLength(128, ErrorMessage = "Tên người đại diện quá dài")]
        public string LegalRepresentative { get; set; }
        [MaxLength(128, ErrorMessage = "Địa chỉ đối tác quá dài")]
        public string Address { get; set; }
        [MaxLength(64, ErrorMessage = "Mã số thuế quá dài")]
        public string TaxIdNo { get; set; }
        [MaxLength(32, ErrorMessage = "Số điện thoại quá dài")]
        public string PhoneNumber { get; set; }
        [MaxLength(128, ErrorMessage = "Website quá dài")]
        public string Website { get; set; }
        [MaxLength(128, ErrorMessage = "Email quá dài")]
        public string Email { get; set; }
        public int? LogoFileId { get; set; }
    }
}
