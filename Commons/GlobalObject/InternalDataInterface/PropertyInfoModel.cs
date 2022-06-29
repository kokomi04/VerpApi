using System.ComponentModel.DataAnnotations;

namespace VErp.Commons.GlobalObject.InternalDataInterface
{
    public class PropertyInfoModel
    {
        public int PropertyId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập mã thuộc tính")]
        [MinLength(1, ErrorMessage = "Mã thuộc tính quá ngắn")]
        [MaxLength(128, ErrorMessage = "Mã thuộc tính quá dài")]
        public string PropertyCode { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên thuộc tính")]
        [MinLength(1, ErrorMessage = "Tên thuộc tính quá ngắn")]
        [MaxLength(128, ErrorMessage = "Tên thuộc tính quá dài")]
        public string PropertyName { get; set; }

    }
}
