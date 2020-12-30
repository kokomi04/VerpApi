using AutoMapper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.MasterDB;

namespace VErp.Services.Master.Model.Config
{
    public class PrintConfigModel : PrintConfigDetailModel, IMapFrom<PrintConfigExtract>
    {
        public int? ActiveForId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập tên cấu hình")]
        public string PrintConfigName { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập tiêu đề phiếu in")]
        public string Title { get; set; }
        public int ModuleTypeId { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<PrintConfigExtract, PrintConfigModel>()
                .ForMember(m => m.BodyTable, v => v.MapFrom(m => m.PrintConfigDetailModel.Count == 0 ? null : m.PrintConfigDetailModel.Last(x => !x.IsOrigin).BodyTable))
                .ForMember(m => m.GenerateCode, v => v.MapFrom(m => m.PrintConfigDetailModel.Count == 0 ? null : m.PrintConfigDetailModel.Last(x => !x.IsOrigin).GenerateCode))
                .ForMember(m => m.PaperSize, v => v.MapFrom(m => m.PrintConfigDetailModel.Count == 0 ? null : m.PrintConfigDetailModel.Last(x => !x.IsOrigin).PaperSize))
                .ForMember(m => m.Layout, v => v.MapFrom(m => m.PrintConfigDetailModel.Count == 0 ? null : m.PrintConfigDetailModel.Last(x => !x.IsOrigin).Layout))
                .ForMember(m => m.HeadTable, v => v.MapFrom(m => m.PrintConfigDetailModel.Count == 0 ? null : m.PrintConfigDetailModel.Last(x => !x.IsOrigin).HeadTable))
                .ForMember(m => m.FootTable, v => v.MapFrom(m => m.PrintConfigDetailModel.Count == 0 ? null : m.PrintConfigDetailModel.Last(x => !x.IsOrigin).FootTable))
                .ForMember(m => m.StickyFootTable, v => v.MapFrom(m => m.PrintConfigDetailModel.Count == 0 ? null : m.PrintConfigDetailModel.Last(x => !x.IsOrigin).StickyFootTable))
                .ForMember(m => m.StickyHeadTable, v => v.MapFrom(m => m.PrintConfigDetailModel.Count == 0 ? null : m.PrintConfigDetailModel.Last(x => !x.IsOrigin).StickyHeadTable))
                .ForMember(m => m.HasTable, v => v.MapFrom(m => m.PrintConfigDetailModel.Count == 0 ? null : m.PrintConfigDetailModel.Last(x => !x.IsOrigin).HasTable))
                .ForMember(m => m.Background, v => v.MapFrom(m => m.PrintConfigDetailModel.Count == 0 ? null : m.PrintConfigDetailModel.Last(x => !x.IsOrigin).Background))
                .ForMember(m => m.TemplateFileId, v => v.MapFrom(m => m.PrintConfigDetailModel.Count == 0 ? null : m.PrintConfigDetailModel.Last(x => !x.IsOrigin).TemplateFileId))
                .ForMember(m => m.GenerateToString, v => v.MapFrom(m => m.PrintConfigDetailModel.Count == 0 ? null : m.PrintConfigDetailModel.Last(x => !x.IsOrigin).GenerateToString))
                .ReverseMap()
                .ForMember(m => m.PrintConfigDetailModel, v => v.Ignore());
        }
    }

    public class PrintConfigExtract : IMapFrom<PrintConfig>
    {
        public int PrintConfigId { get; set; }
        public int? ActiveForId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập tên cấu hình")]
        public string PrintConfigName { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập tiêu đề phiếu in")]
        public string Title { get; set; }
        public int ModuleTypeId { get; set; }

        public IList<PrintConfigDetailModel> PrintConfigDetailModel { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<PrintConfig, PrintConfigExtract>()
                .ForMember(m => m.PrintConfigDetailModel, v => v.MapFrom(m => m.PrintConfigDetail))
                .ReverseMap()
                .ForMember(m => m.PrintConfigDetail, v => v.Ignore());
        }
    }

    public class PrintTemplateInput
    {
        public List<NonCamelCaseDictionary> data { get; set; }
    }
}
