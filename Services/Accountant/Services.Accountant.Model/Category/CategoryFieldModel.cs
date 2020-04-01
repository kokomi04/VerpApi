using System.ComponentModel.DataAnnotations;

namespace VErp.Services.Accountant.Model.Category

{
    public class CategoryFieldInputModel
    {
        public int CategoryFieldId { get; set; }
        public int? ReferenceCategoryFieldId { get; set; }
        public int CategoryId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập tiêu đề trường dữ liệu")]
        [MaxLength(256, ErrorMessage = "Tiêu đề trường dữ liệu quá dài")]
        public string Title { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập tên trường dữ liệu")]
        [MaxLength(45, ErrorMessage = "Tên trường dữ liệu quá dài")]
        public string Name { get; set; }
        public int Sequence { get; set; }
        public int DataTypeId { get; set; }
        public int DataSize { get; set; }
        public int FormTypeId { get; set; }
        public bool AutoIncrement { get; set; }
        public bool IsRequired { get; set; }
        public bool IsUnique { get; set; }
        public bool IsHidden { get; set; }
    }

    public class CategoryFieldOutputModel: CategoryFieldInputModel
    {
        public DataTypeModel DataType { get; set; }
        public FormTypeModel FormType { get; set; }
    }
}
