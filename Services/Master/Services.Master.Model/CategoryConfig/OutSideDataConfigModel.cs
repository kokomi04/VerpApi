﻿using AutoMapper;
using System.Collections.Generic;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.MasterDB;

namespace VErp.Services.Master.Model.CategoryConfig
{
    //public class OutSideDataConfigModel : IMapFrom<OutSideDataConfig>
    //{
    //    public OutSideDataConfigModel()
    //    {
    //        OutsideDataFieldConfig = new List<OutSideDataFieldConfigModel>();
    //    }
    //    public int ModuleType { get; set; }
    //    public string Url { get; set; }
    //    public string Key { get; set; }
    //    public string ParentKey { get; set; }
    //    public string Description { get; set; }
    //    public string ModuleTypeTitle { get; set; }
    //    public string Joins { get; set; }
    //    public string RawSql { get; set; }
    //    public ICollection<OutSideDataFieldConfigModel> OutsideDataFieldConfig { get; set; }

    //    public void Mapping(Profile profile)
    //    {
    //        profile.CreateMapCustom<OutSideDataConfigModel, OutSideDataConfig>();
    //        profile.CreateMapCustom<OutSideDataConfig, OutSideDataConfigModel>()
    //            .ForMember(dest => dest.ModuleTypeTitle, opt => opt.MapFrom(s => ((EnumModuleType)s.ModuleType).GetEnumDescription()));
    //    }
    //}

    //public class OutSideDataFieldConfigModel : IMapFrom<OutsideDataFieldConfig>
    //{
    //    public int OutsideDataFieldConfigId { get; set; }
    //    public int CategoryId { get; set; }
    //    public string Value { get; set; }
    //    public string Alias { get; set; }
    //}
}
