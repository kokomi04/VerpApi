
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.AccountancyDB;

namespace VErp.Services.Accountancy.Model.Category

{
    public class DataTypeModel
    {
        public int DataTypeId { get; set; }
        public string Title { get; set; }
        public string Name { get; set; }
        public int DataSizeDefault { get; set; }

        public string RegularExpression { get; set; }
    }
}
