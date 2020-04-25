
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VErp.Services.Accountant.Model.Category

{
    public class CategoryRowInputModel
    {
        public CategoryRowInputModel()
        {
            Values = new HashSet<CategoryValueModel>();
        }
        public int? ParentCategoryRowId { get; set; }

        public ICollection<CategoryValueModel> Values { get; set; }
    }

    public class CategoryRowOutputModel : CategoryRowInputModel
    {
        public int CategoryRowId { get; set; }

        public CategoryRowOutputModel ParentCategoryRow { get; set; }
    }
}
