using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.MasterDB;

namespace VErp.Services.Master.Model.PrintConfig
{
    public class PrintConfigHeaderStandardModel : PrintConfigHeaderStandardBaseModel
    {
        public string PrintConfigHeaderStandardCode { get; set; }

        public string JsAction { get; set; }
    }
    public class PrintConfigHeaderStandardViewModel : PrintConfigHeaderStandardBaseModel
    {
        public int PrintConfigHeaderStandardId { get; set; }
    }
    public class PrintConfigHeaderStandardBaseModel : IMapFrom<PrintConfigHeaderStandard>
    {
        public string Title { get; set; }

        public bool? IsShow { get; set; }

        public int SortOrder { get; set; }
    }
}
