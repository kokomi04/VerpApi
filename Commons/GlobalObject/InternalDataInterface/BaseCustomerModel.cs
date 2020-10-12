using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using VErp.Commons.Enums.MasterEnum;

namespace VErp.Commons.GlobalObject.InternalDataInterface
{
    public class BaseCustomerImportModel
    {
        [Display(Name = "Mã KH")]
        [Required(ErrorMessage = "Vui lòng nhập mã đối tác")]
        [MaxLength(128, ErrorMessage = "Tên đối tác quá dài")]
        public string CustomerCode { get; set; }

        [Display(Name = "Tên KH")]
        [Required(ErrorMessage = "Vui lòng nhập tên đối tác")]
        [MaxLength(128, ErrorMessage = "Tên đối tác quá dài")]
        public string CustomerName { get; set; }

        [Display(Name = "Loại KH (Cá nhân, tổ chức)")]
        public EnumCustomerType CustomerTypeId { get; set; }
        [MaxLength(128, ErrorMessage = "Địa chỉ đối tác quá dài")]

        [Display(Name = "Địa chỉ")]
        public string Address { get; set; }

        [Display(Name = "Mã số thuế")]
        [MaxLength(64, ErrorMessage = "Mã số thuế quá dài")]
        public string TaxIdNo { get; set; }

        [Display(Name = "Số điện thoại")]
        [MaxLength(64, ErrorMessage = "Số điện thoại quá dài")]
        public string PhoneNumber { get; set; }

        [Display(Name = "Website")]
        [MaxLength(128, ErrorMessage = "Website quá dài")]
        public string Website { get; set; }

        [Display(Name = "Email")]
        [MaxLength(128, ErrorMessage = "Email quá dài")]
        public string Email { get; set; }

        [Display(Name = "Mô tả thêm")]
        [MaxLength(128, ErrorMessage = "Mô tả quá dài")]
        public string Description { get; set; }

        [Display(Name = "Tên người đại diện")]
        [MaxLength(128, ErrorMessage = "Tên người đại diện quá dài")]
        public string LegalRepresentative { get; set; }


        [Display(Name = "Số CMND, thẻ căn cước")]
        [MaxLength(64, ErrorMessage = "Số CMND quá dài")]
        public string Identify { get; set; }

        [Display(Name = "Số ngày nợ")]
        public int? DebtDays { get; set; }
        
    }

    public class BaseCustomerModel: BaseCustomerImportModel
    {
        public bool IsActived { get; set; }

        public EnumCustomerStatus CustomerStatusId { get; set; }
    }
}
