using System.ComponentModel.DataAnnotations;

namespace VErp.Commons.GlobalObject.InternalDataInterface
{
    public class DepartmentSimpleModel
    {
        public int DepartmentId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập mã bộ phận")]
        [MaxLength(32, ErrorMessage = "Mã bộ phận quá dài")]
        public string DepartmentCode { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập tên tên bộ phận")]
        [MaxLength(128, ErrorMessage = "Tên bộ phận quá dài")]
        public string DepartmentName { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập số người trong bộ phận")]
        public int NumberOfPerson { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập số máy trong bộ phận")]
        public int NumberOfMachine { get; set; }
    }
}
