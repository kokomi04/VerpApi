using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.MasterDB;

namespace VErp.Services.Master.Model.Config
{
    public class PrintConfigDetailModel : IMapFrom<PrintConfigDetail>
    {
        public int? PrintConfigId { get; set; }
        public string BodyTable { get; set; }
        public string GenerateCode { get; set; }
        public int? PaperSize { get; set; }
        public string Layout { get; set; }
        public string HeadTable { get; set; }
        public string FootTable { get; set; }
        public bool? StickyFootTable { get; set; }
        public bool? StickyHeadTable { get; set; }
        public bool? HasTable { get; set; }
        public string Background { get; set; }
        public long? TemplateFileId { get; set; }
        public string TemplateFilePath { get; set; }
        public string TemplateFileName { get; set; }
        public string ContentType { get; set; }
        public string GenerateToString { get; set; }
        public bool IsOrigin { get; set; }
    }

}
