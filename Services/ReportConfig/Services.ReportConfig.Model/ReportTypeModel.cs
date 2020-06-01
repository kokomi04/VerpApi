using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.AccountantEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.ReportConfigDB;

namespace Verp.Services.ReportConfig.Model
{
    public class ReportTypeListModel : IMapFrom<ReportType>
    {
        public int? ReportTypeId { get; set; }
        public int ReportTypeGroupId { get; set; }
        public string ReportTypeName { get; set; }
    }

    public class ReportTypeModel : ReportTypeListModel
    {
        public string ReportPath { get; set; }
        public int SortOrder { get; set; }
    }

}
