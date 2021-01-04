using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using VErp.Commons.Enums.MasterEnum;

namespace VErp.Commons.GlobalObject.InternalDataInterface
{
    public class VoucherTypeSimpleModel
    {
        public int VoucherTypeId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập tên chứng từ")]
        [MaxLength(256, ErrorMessage = "Tên chứng từ quá dài")]
        public string Title { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập mã chứng từ")]
        [MaxLength(45, ErrorMessage = "Mã chứng từ quá dài")]
        [RegularExpression(@"(^[a-zA-Z0-9_]*$)", ErrorMessage = "Mã chứng từ chỉ gồm các ký tự chữ, số và ký tự _.")]
        public string VoucherTypeCode { get; set; }

        public int SortOrder { get; set; }
        public int? VoucherTypeGroupId { get; set; }

        public IList<VoucherActionSimpleModel> ActionObjects { get; set; }

        public IList<VoucherAreaFieldSimpleModel> AreaFields { get; set; }
    }

    public class VoucherActionSimpleModel
    {
        public int VoucherTypeId { get; set; }
        public int VoucherActionId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập tên chức năng")]
        [MaxLength(256, ErrorMessage = "Tên chức năng quá dài")]
        public string Title { get; set; }
        public int SortOrder { get; set; }
        public EnumActionType? ActionTypeId { get; set; }
    }

    public class VoucherAreaFieldSimpleModel
    {
        public int VoucherAreaId { get; set; }
        public string VoucherAreaTitle { get; set; }
        public int VoucherAreaFieldId { get; set; }
        public string VoucherAreaFieldTitle { get; set; }        
        public int VoucherFieldId { get; set; }
        public EnumFormType FormTypeId { get; set; }
    }
}
