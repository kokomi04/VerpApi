using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ReportConfigDB;

namespace Verp.Services.ReportConfig.Model
{
    public class ReportTypeGroupModel : IMapFrom<ReportTypeGroup>
    {
        public string ReportTypeGroupName { get; set; }
        public int SortOrder { get; set; }
        public int ModuleTypeId { get; set; }
    }

    public class ReportTypeGroupList : ReportTypeGroupModel
    {
        public int ReportTypeGroupId { get; set; }
    }
}
