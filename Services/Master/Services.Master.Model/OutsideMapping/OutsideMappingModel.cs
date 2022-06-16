﻿using AutoMapper;
using System.Collections.Generic;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.MasterDB;

namespace VErp.Services.Master.Model.OutsideMapping
{
    public class OutsideMappingModelBase
    {
        public EnumObjectType? SourceObjectTypeId { get; set; }
        public int? SourceInputTypeId { get; set; }
        public EnumObjectType ObjectTypeId { get; set; }
        public int InputTypeId { get; set; }
        public string MappingFunctionKey { get; set; }
        public string FunctionName { get; set; }
        public string Description { get; set; }
        public bool IsWarningOnDuplicated { get; set; }
        public string SourceDetailsPropertyName { get; set; }
        public string DestinationDetailsPropertyName { get; set; }
        public string ObjectIdFieldName { get; set; }

        public string JsCodeVisible { get; set; }
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

        public string JsCodeAfterSourceDataLoaded { get; set; }
        public string JsCodeBeforeDataMapped { get; set; }
        public string JsCodeAfterDataMapped { get; set; }
        public string JsCodeAfterTargetBillCreated { get; set; }

        public IList<OutsiteMappingModel> FieldMappings { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<OutsideMappingModel, OutsideImportMappingFunction>()
                .ForMember(d => d.OutsideImportMapping, s => s.Ignore())
                .ForMember(d => d.OutsideImportMappingObject, s => s.Ignore())
                .ForMember(d => d.ObjectTypeId, s => s.MapFrom(f => (int)f.ObjectTypeId))
                .ForMember(d => d.SourceObjectTypeId, s => s.MapFrom(f => (int?)f.SourceObjectTypeId))
                .ReverseMap()
                .ForMember(s => s.FieldMappings, d => d.Ignore())
                .ForMember(d => d.ObjectTypeId, s => s.MapFrom(f => (EnumObjectType)f.ObjectTypeId))
                .ForMember(d => d.SourceObjectTypeId, s => s.MapFrom(f => (EnumObjectType?)f.SourceObjectTypeId));
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
