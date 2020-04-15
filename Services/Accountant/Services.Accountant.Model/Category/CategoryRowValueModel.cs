
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VErp.Services.Accountant.Model.Category

{
    public class CategoryRowValueModel
    {
        public CategoryRowValueModel()
        {
        }
        public int CategoryRowId { get; set; }
        public int CategoryFieldId { get; set; }
        public int CategoryValueId { get; set; }
        public string Value { get; set; }
    }
}
