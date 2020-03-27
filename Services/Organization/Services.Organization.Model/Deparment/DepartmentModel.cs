using System.ComponentModel.DataAnnotations;

namespace VErp.Services.Organization.Model.Department
{
    public class DepartmentModel
    {
        public int DepartmentId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập mã bộ phận")]
        [MaxLength(32, ErrorMessage = "Mã bộ phận quá dài")]
        public string DepartmentCode { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập tên tên bộ phận")]
        [MaxLength(128, ErrorMessage = "Tên bộ phận quá dài")]
        public string DepartmentName { get; set; }
        public string Description { get; set; }
        public int? ParentId { get; set; }
        public string ParentName { get; set; }
        public bool IsActived { get; set; }
    }
}
