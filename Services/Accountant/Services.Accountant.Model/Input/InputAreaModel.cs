using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VErp.Services.Accountant.Model.Input
{
    public abstract class InputAreaModel
    {
        public InputAreaModel()
        {
        }
        public int InputAreaId { get; set; }
        public int InputTypeId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập tên vùng dữ liệu")]
        [MaxLength(256, ErrorMessage = "Tên vùng dữ liệu quá dài")]
        public string Title { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập mã vùng dữ liệu")]
        [MaxLength(45, ErrorMessage = "Vùng dữ liệu quá dài")]
        public string InputAreaCode { get; set; }
        public bool IsMultiRow { get; set; }
    }

    public class InputAreaInputModel : InputAreaModel
    {
    }

    public class InputAreaOutputModel : InputAreaModel
    {
        public InputAreaOutputModel()
        {
            InputAreaFields = new List<InputAreaFieldOutputFullModel>();
        }

        public ICollection<InputAreaFieldOutputFullModel> InputAreaFields { get; set; }
    }
    
}
