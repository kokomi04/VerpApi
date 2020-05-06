using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.AccountingDB;

namespace VErp.Services.Accountant.Model.Category
{
    public class CategoryValueModel : IMapFrom<CategoryRowValue>
    {
        public int CategoryValueId { get; set; }
        public int CategoryFieldId { get; set; }
        public string Value { get; set; }
    }
}
