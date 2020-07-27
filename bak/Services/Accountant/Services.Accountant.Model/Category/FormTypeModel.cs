using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.AccountingDB;

namespace VErp.Services.Accountant.Model.Category

{
    public class FormTypeModel : IMapFrom<FormType>
    {
        public int FormTypeId { get; set; }
        public string Title { get; set; }
        public string Name { get; set; }
    }
}
