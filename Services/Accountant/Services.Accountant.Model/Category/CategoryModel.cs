
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VErp.Services.Accountant.Model.Category

{
    public abstract class CategoryBase<T> where T : CategoryBase<T>
    {
        public CategoryBase()
        {
            SubCategories = new List<T>();
        }
        public int? ParentId { get; set; }
        public int CategoryId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập tên danh mục")]
        [MaxLength(256, ErrorMessage = "Tên danh mục quá dài")]
        public string Title { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập mã danh mục")]
        [MaxLength(45, ErrorMessage = "Mã danh mục quá dài")]
        public string CategoryCode { get; set; }
        public bool IsModule { get; set; }
        public bool IsReadonly { get; set; }
        public ICollection<T> SubCategories { get; set; }
    }

    public class CategoryModel : CategoryBase<CategoryModel>
    {
    }

    public class CategoryFullModel : CategoryBase<CategoryFullModel>
    {
        public CategoryFullModel()
        {
            CategoryFields = new List<CategoryFieldOutputModel>();
        }

        public ICollection<CategoryFieldOutputModel> CategoryFields { get; set; }
    }
}
