using AutoMapper;
using System.Collections;
using System.Collections.Generic;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.MasterDB;

namespace VErp.Services.Master.Model.PrintConfig
{
    public class PrintConfigBaseModel
    {
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
        public long? TemplateFileId { get; set; }
        public string GenerateToString { get; set; }
        public string TemplateFilePath { get; set; }
        public string TemplateFileName { get; set; }
        public string ContentType { get; set; }
        //public int ModuleTypeId { get; set; }
        public IList<int> ModuleTypeIds { get; set; }
    }

    public class PrintConfigRollbackModel : PrintConfigBaseModel, IMapFrom<PrintConfigStandard>
    {

    }

    public class PrintConfigModuleMapping: ICustomMapping
    {
        public int ConfigId { get; set; }
        public int ModuleTypeId { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<PrintConfigStandardModuleType, PrintConfigModuleMapping>()
                .ForMember(d => d.ConfigId, s => s.MapFrom(f => f.PrintConfigStandardId))
                .ForMember(d => d.ModuleTypeId, s => s.MapFrom(f => f.ModuleTypeId))
                .ReverseMap();

            profile.CreateMap<PrintConfigCustomModuleType, PrintConfigModuleMapping>()
                .ForMember(d => d.ConfigId, s => s.MapFrom(f => f.PrintConfigCustomId))
                .ForMember(d => d.ModuleTypeId, s => s.MapFrom(f => f.ModuleTypeId))
                .ReverseMap();
        }
    }
}
