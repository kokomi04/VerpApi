using AutoMapper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.AccountancyDB;

namespace VErp.Services.Accountancy.Model.Input
{
    public class PrintConfigModel : IMapFrom<PrintConfig>
    {
        public int PrintConfigId { get; set; }
        public int? ActiveForId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập tên cấu hình")]
        public string PrintConfigName { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập tiêu đề phiếu in")]
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
        public long? TemplateFileId { get; set; }
        public string GenerateToString { get; set; }
        public int ModuleTypeId { get; set; }
    }

    public class PrintTemplateInput
    {
        public List<NonCamelCaseDictionary> data { get; set; }
    }
}
