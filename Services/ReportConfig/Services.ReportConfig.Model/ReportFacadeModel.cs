﻿using System.Collections.Generic;

namespace Verp.Services.ReportConfig.Model
{
    public class ReportFacadeModel
    {
        public RHeader Header { get; set; }
        public RBody Body { get; set; }
        public RFooter Footer { get; set; }

        public class RFooter
        {
            public string RDateText { get; set; }
            public string SignText { get; set; }
        }

        public class RHeader
        {
            public long fLogoId { get; set; }
            public string CompanyBreif { get; set; }
            public string FormBreif { get; set; }
        }

        public class RBody
        {
            public string Title { get; set; }
            public List<ReportHeadModel> HeadDetails { get; set; }
            public ReportFilterDataModel FilterData { get; set; }
        }
    }
}
