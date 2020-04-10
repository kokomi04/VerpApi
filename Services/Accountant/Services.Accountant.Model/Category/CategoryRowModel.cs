
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

    public class CategoryRowImportResultModel
    {
        public CategoryRowImportResultModel()
        {
            Success = new List<int>();
            Error = new Dictionary<int, string>();
        }
        public ICollection<int> Success { get; set; }
        public IDictionary<int, string> Error { get; set; }
    }

}
