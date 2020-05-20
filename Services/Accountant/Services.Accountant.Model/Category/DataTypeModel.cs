
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.AccountingDB;

namespace VErp.Services.Accountant.Model.Category

{
    public class DataTypeModel: IMapFrom<DataType>
    {
        public int DataTypeId { get; set; }
        public string Title { get; set; }
        public string Name { get; set; }
        public int DataSizeDefault { get; set; }

        public string RegularExpression { get; set; }
    }
}
