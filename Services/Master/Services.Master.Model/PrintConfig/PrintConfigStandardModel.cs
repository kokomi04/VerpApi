using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.MasterDB;

namespace VErp.Services.Master.Model.PrintConfig
{
    public class PrintConfigStandardModel : PrintConfigBaseModel, IMapFrom<PrintConfigStandard>
    {
        public int? PrintConfigStandardId { get; set; }
        public int? PrintConfigHeaderStandardId { get; set; }
    }
}
