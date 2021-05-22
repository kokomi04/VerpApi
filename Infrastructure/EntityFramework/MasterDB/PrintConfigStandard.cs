using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.MasterDB
{
    public partial class PrintConfigStandard
    {
        public int PrintConfigStandardId { get; set; }
        public string PrintConfigName { get; set; }
        public string Title { get; set; }
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
        public string GenerateToString { get; set; }
        public string TemplateFilePath { get; set; }
        public string TemplateFileName { get; set; }
        public string ContentType { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }
        public int? ModuleTypeId { get; set; }
    }
}
