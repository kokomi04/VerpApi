using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.AccountancyDB;

namespace VErp.Services.Accountancy.Model.OutsideMapping
{
    public class OutsideMappingModelList : IMapFrom<OutsideImportMappingFunction>
    {
        public string MappingFunctionKey { get; set; }
        public string FunctionName { get; set; }
        public string Description { get; set; }
        public bool IsWarningOnDuplicated { get; set; }

        public virtual void Mapping(Profile profile)
        {
            profile.CreateMap<OutsideMappingModelList, OutsideImportMappingFunction>()
               .ForMember(d => d.OutsideImportMapping, s => s.Ignore())
               .ForMember(d => d.OutsideImportMappingObject, s => s.Ignore())
               .ReverseMap();
        }

    }

    public class OutsideMappingModel : OutsideMappingModelList
    {
        public IList<OutsiteMappingModel> FieldMappings { get; set; }

        public override void Mapping(Profile profile)
        {
            profile.CreateMap<OutsideMappingModel, OutsideImportMappingFunction>()
                .ForMember(d => d.OutsideImportMapping, s => s.Ignore())
                .ForMember(d => d.OutsideImportMappingObject, s => s.Ignore())
                .ReverseMap()
                .ForMember(s => s.FieldMappings, d => d.Ignore());
        }

    }
    public class OutsiteMappingModel : IMapFrom<OutsideImportMapping>
    {
        public string SourceFieldName { get; set; }
        public string DestinationFieldName { get; set; }
        public void Mapping(Profile profile)
        {
            profile.CreateMap<OutsiteMappingModel, OutsideImportMapping>()
                .ForMember(d => d.OutsideImportMappingFunction, s => s.Ignore())
                .ReverseMap();
        }
    }
}
