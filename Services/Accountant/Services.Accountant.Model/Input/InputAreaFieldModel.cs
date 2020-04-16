using System.ComponentModel.DataAnnotations;
using VErp.Services.Accountant.Model.Category;

namespace VErp.Services.Accountant.Model.Input
{
    public abstract class InputAreaFieldModel
    {
        public int InputAreaId { get; set; }
        public int FieldIndex { get; set; }
        public int InputTypeId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập tên trường dữ liệu")]
        [MaxLength(45, ErrorMessage = "Tên trường dữ liệu quá dài")]
        public string FieldName { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập tiêu đề trường dữ liệu")]
        [MaxLength(256, ErrorMessage = "Tiêu đề trường dữ liệu quá dài")]
        public string Title { get; set; }
        public string Placeholder { get; set; }
        public int SortOrder { get; set; }
        public int DataTypeId { get; set; }
        public int DataSize { get; set; }
        public int FormTypeId { get; set; }
        public bool IsAutoIncrement { get; set; }
        public bool IsRequire { get; set; }
        public bool IsUnique { get; set; }
        public bool IsHidden { get; set; }
        public string RegularExpresion { get; set; }
        public string DefaultValue { get; set; }
        public int? ReferenceCategoryFieldId { get; set; }
        public int? ReferenceCategoryTitleFieldId { get; set; }
    }

    public class InputAreaFieldInputModel : InputAreaFieldModel
    {
    }

    public class InputAreaFieldOutputModel : InputAreaFieldModel
    {
        public int? ReferenceCategoryId { get; set; }
    }
    
    public class InputAreaFieldOutputFullModel : InputAreaFieldOutputModel
    {
        public DataTypeModel DataType { get; set; }
        public FormTypeModel FormType { get; set; }
        public CategoryFieldOutputFullModel SourceCategoryField { get; set; }
        public CategoryFieldOutputFullModel SourceCategoryTitleField { get; set; }
        public CategoryModel SourceCategory { get; set; }
    }
}
