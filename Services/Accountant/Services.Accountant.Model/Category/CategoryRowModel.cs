
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VErp.Services.Accountant.Model.Category

{
    public class CategoryValueModel
    {
        public int CategoryValueId { get; set; }
        public int CategoryFieldId { get; set; }
        public string Value { get; set; }
    }

    public class CategoryRowInputModel
    {
        public CategoryRowInputModel()
        {
            Values = new HashSet<CategoryValueModel>();
        }
        public ICollection<CategoryValueModel> Values { get; set; }
    }

    public class CategoryRowOutputModel
    {
        public CategoryRowOutputModel()
        {
            Values = new List<CategoryValueModel>();
        }
        public int CategoryRowId { get; set; }
        public ICollection<CategoryValueModel> Values { get; set; }
    }
}
