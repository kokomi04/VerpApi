using System.ComponentModel.DataAnnotations;
using VErp.Commons.Enums.MasterEnum;

namespace VErp.Services.Master.Model.Customer
{
    public class CustomerContactModel
    {
        public int CustomerContactId { get; set; }
        [MaxLength(128, ErrorMessage = "Tên quá dài")]
        public string FullName { get; set; }
        public EnumGender? GenderId { get; set; }
        [MaxLength(128, ErrorMessage = "Chức vụ quá dài")]
        public string Position { get; set; }    
        [MaxLength(64, ErrorMessage = "Số điện thoại quá dài")]
        public string PhoneNumber { get; set; }
        [MaxLength(128, ErrorMessage = "Email quá dài")]
        public string Email { get; set; }

    }
}
