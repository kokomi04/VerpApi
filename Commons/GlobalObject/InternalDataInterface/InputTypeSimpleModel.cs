using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using VErp.Commons.Enums.MasterEnum;

namespace VErp.Commons.GlobalObject.InternalDataInterface
{
    public class InputTypeSimpleModel
    {
        public int InputTypeId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập tên chứng từ")]
        [MaxLength(256, ErrorMessage = "Tên chứng từ quá dài")]
        public string Title { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập mã chứng từ")]
        [MaxLength(45, ErrorMessage = "Mã chứng từ quá dài")]
        [RegularExpression(@"(^[a-zA-Z0-9_]*$)", ErrorMessage = "Mã chứng từ chỉ gồm các ký tự chữ, số và ký tự _.")]
        public string InputTypeCode { get; set; }

        public int SortOrder { get; set; }
        public int? InputTypeGroupId { get; set; }

        public IList<InputActionSimpleModel> ActionObjects { get; set; }
        public IList<InputAreaFieldSimpleModel> AreaFields { get; set; }
    }

    public class InputActionSimpleModel
    {
        public int InputTypeId { get; set; }
        public int InputActionId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập tên chức năng")]
        [MaxLength(256, ErrorMessage = "Tên chức năng quá dài")]
        public string Title { get; set; }
        public int SortOrder { get; set; }
    }

    public class InputAreaFieldSimpleModel
    {
        public int InputAreaId { get; set; }
        public string InputAreaTitle { get; set; }
        public int InputAreaFieldId { get; set; }
        public string InputAreaFieldTitle { get; set; }
        public int InputFieldId { get; set; }
        public EnumFormType FormTypeId { get; set; }
    }
}
