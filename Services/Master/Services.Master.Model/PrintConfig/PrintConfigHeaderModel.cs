using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.MasterDB;

namespace VErp.Services.Master.Model.PrintConfig
{
    public class PrintConfigHeaderModel : PrintConfigHeaderBaseModel
    {
        public string PrintConfigHeaderCode { get; set; }
    }
    public class PrintConfigHeaderViewModel : PrintConfigHeaderBaseModel
    {
        public int PrintConfigHeaderId { get; set; }
    }
    public class PrintConfigHeaderBaseModel : IMapFrom<PrintConfigHeader>
    {
        public string PrintConfigHeaderName { get; set; }

        public string Title { get; set; }

        public string Layout { get; set; }

        public bool? IsShow { get; set; }

        public int SortOrder { get; set; }

        public List<PrintConfigCustomOutputHeaderModel> PrintConfigCustoms { get; set; }
    }

    public class PrintConfigCustomOutputHeaderModel : IMapFrom<PrintConfigCustom>
    {

        public int? PrintConfigCustomId { get; set; }
        public string PrintConfigName { get; set; }
        public string Title { get; set; }
    }
}
