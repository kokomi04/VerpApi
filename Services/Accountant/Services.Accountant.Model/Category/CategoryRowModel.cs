
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VErp.Services.Accountant.Model.Category

{
    public class CategoryRowInputModel
    {
        public CategoryRowInputModel()
        {
            CategoryRowValues = new HashSet<CategoryValueModel>();
        }
        public int? ParentCategoryRowId { get; set; }

        public ICollection<CategoryValueModel> CategoryRowValues { get; set; }
    }

    public class CategoryRowListOutputModel : CategoryRowInputModel
    {
        public int CategoryRowId { get; set; }
    }

    public class CategoryRowOutputModel : CategoryRowListOutputModel
    {
        public CategoryRowOutputModel ParentCategoryRow { get; set; }
    }
}
