
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VErp.Services.Accountant.Model.Input

{
    public class InputTypeModel
    {
        public InputTypeModel()
        {
        }

        public int InputTypeId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập tên chứng từ")]
        [MaxLength(256, ErrorMessage = "Tên chứng từ quá dài")]
        public string Title { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập mã chứng từ")]
        [MaxLength(45, ErrorMessage = "Mã chứng từ quá dài")]
        public string InputTypeCode { get; set; }
    }

    public class InputTypeFullModel : InputTypeModel
    {
        public InputTypeFullModel()
        {
            InputAreas = new List<InputAreaOutputModel>();
        }

        public ICollection<InputAreaOutputModel> InputAreas { get; set; }
    }
}
