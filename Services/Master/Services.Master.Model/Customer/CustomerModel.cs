using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VErp.Commons.Enums.MasterEnum;

namespace VErp.Services.Master.Model.Customer
{
    public class CustomerModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên đối tác")]
        [MaxLength(128, ErrorMessage = "Tên đối tác quá dài")]
        public string CustomerName { get; set; }
        public EnumCustomerType CustomerTypeId { get; set; }
        [MaxLength(128, ErrorMessage = "Địa chỉ đối tác quá dài")]
        public string Address { get; set; }
        [MaxLength(64, ErrorMessage = "Mã số thuế quá dài")]
        public string TaxIdNo { get; set; }
        [MaxLength(64, ErrorMessage = "Số điện thoại quá dài")]
        public string PhoneNumber { get; set; }
        [MaxLength(128, ErrorMessage = "Website quá dài")]
        public string Website { get; set; }
        [MaxLength(128, ErrorMessage = "Email quá dài")]
        public string Email { get; set; }

        [MaxLength(128, ErrorMessage = "Mô tả quá dài")]
        public string Description { get; set; }
        public bool IsActived { get; set; }

        public IList<CustomerContactModel> Contacts { get; set; }
    }
}
