using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Infrastructure.EF.MasterDB;

namespace VErp.Services.Master.Model
{
    public class CustomMappingModel : ICustomMapping
    {
        public void Mapping(Profile profile)
        {
            profile.CreateMap<ActionButton, ActionButtonModel>()
                .ForMember(d => d.ObjectTypeId, s => s.MapFrom(m => (EnumObjectType?)m.ObjectTypeId))
                .ReverseMap()
                .ForMember(d => d.ObjectTypeId, s => s.MapFrom(m => (int)m.ObjectTypeId));

            profile.CreateMap<ActionButton, ActionButtonSimpleModel>()
               .ForMember(d => d.ObjectTypeId, s => s.MapFrom(m => (EnumObjectType?)m.ObjectTypeId))
               .ReverseMap()
               .ForMember(d => d.ObjectTypeId, s => s.MapFrom(m => (int)m.ObjectTypeId));
            
        }
    }
}
