using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VErp.Commons.GlobalObject.InternalDataInterface
{
    public class CategoryListModel
    {
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên danh mục")]
        [MaxLength(256, ErrorMessage = "Tên danh mục quá dài")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mã danh mục")]
        [MaxLength(45, ErrorMessage = "Mã danh mục quá dài")]
        [RegularExpression(@"(^_[a-zA-Z0-9_]*$)", ErrorMessage = "Mã danh mục bắt đầu bằng ký tự _ và chỉ gồm các ký tự chữ, số và ký tự _.")]
        public string CategoryCode { get; set; }

        public int? CategoryGroupId { get; set; }
    }

    public class CategoryFieldSimpleModel
    {
        public int CategoryFieldId { get; set; }
        public int CategoryId { get; set; }
        public string CategoryFieldName { get; set; }
        public string Title { get; set; }
        public int FormTypeId { get; set; }
    }

    public class CategoryFullSimpleModel : CategoryListModel
    {
        public ICollection<CategoryFieldSimpleModel> CategoryField { get; set; }

        public CategoryFullSimpleModel()
        {
            CategoryField = new List<CategoryFieldSimpleModel>();
        }

    }
}
