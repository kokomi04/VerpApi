using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.MasterDB;

namespace VErp.Services.Master.Model.OutsideMapping
{
    public class OutsideMappingModelBase
    {
        public EnumObjectType ObjectTypeId { get; set; }
        public int InputTypeId { get; set; }
        public string MappingFunctionKey { get; set; }
        public string FunctionName { get; set; }
        public string Description { get; set; }
        public bool IsWarningOnDuplicated { get; set; }
        public string SourceDetailsPropertyName { get; set; }
        public string DestinationDetailsPropertyName { get; set; }
        public string ObjectIdFieldName { get; set; }
    }
    public class OutsideMappingModelList : OutsideMappingModelBase, IMapFrom<OutsideImportMappingFunction>
    {
        public int OutsideImportMappingFunctionId { get; set; }      

        public virtual void Mapping(Profile profile)
        {
            profile.CreateMap<OutsideMappingModelList, OutsideImportMappingFunction>()
               .ForMember(d => d.OutsideImportMapping, s => s.Ignore())
               .ForMember(d => d.OutsideImportMappingObject, s => s.Ignore())
               .ReverseMap();
        }

    }

    public class OutsideMappingModel : OutsideMappingModelBase, IMapFrom<OutsideImportMappingFunction>
    {       
        public OutsideMappingModel()
        {
            FieldMappings = new List<OutsiteMappingModel>();
        }
        public IList<OutsiteMappingModel> FieldMappings { get; set; }

        public void Mapping(Profile profile)
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
        public bool IsDetail { get; set; }
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
