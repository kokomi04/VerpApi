﻿using AutoMapper;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.MasterDB;

namespace VErp.Services.Master.Model.Config
{
    public class ObjectPrintConfigMappingModel : IMapFrom<ObjectPrintConfigMapping>
    {
        public int PrintConfigId { get; set; }
        public EnumObjectType ObjectTypeId { get; set; }
        public int ObjectId { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMapIgnoreNoneExist<ObjectPrintConfigMapping, ObjectPrintConfigMappingModel>()
                .ForMember(m => m.PrintConfigId, v => v.MapFrom(m => m.PrintConfigCustomId))
                .ReverseMapIgnoreNoneExist()
                .ForMember(m => m.PrintConfigCustomId, v => v.MapFrom(m => m.PrintConfigId));
        }
    }
    public class ObjectPrintConfig
    {
        public EnumObjectType ObjectTypeId { get; set; }
        public int ObjectId { get; set; }
        public int[] PrintConfigIds { get; set; }
    }
}
