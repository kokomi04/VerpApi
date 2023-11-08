using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Infrastructure.EF.MasterDB
{
    public interface IPrintConfigEntity
    {
        string PrintConfigName { get; set; }
        string Title { get; set; }
        string BodyTable { get; set; }
        string GenerateCode { get; set; }
        int? PaperSize { get; set; }
        string Layout { get; set; }
        string HeadTable { get; set; }
        string FootTable { get; set; }
        bool? StickyFootTable { get; set; }
        bool? StickyHeadTable { get; set; }
        bool? HasTable { get; set; }
        string Background { get; set; }
        string GenerateToString { get; set; }
        string TemplateFilePath { get; set; }
        string TemplateFileName { get; set; }
        string ContentType { get; set; }
        int CreatedByUserId { get; set; }
        DateTime CreatedDatetimeUtc { get; set; }
        int UpdatedByUserId { get; set; }
        DateTime UpdatedDatetimeUtc { get; set; }
        bool IsDeleted { get; set; }
        DateTime? DeletedDatetimeUtc { get; set; }
        int? ModuleTypeId { get; set; }
        string JsCodeBeforePrint { get; set; }
    }

    public partial class PrintConfigStandard : IPrintConfigEntity
    {


    }

    public partial class PrintConfigCustom : IPrintConfigEntity
    {


    }
}
