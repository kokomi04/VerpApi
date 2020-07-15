
using AutoMapper;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VErp.Commons.Enums.AccountantEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.AccountingDB;

namespace VErp.Services.Accountant.Model.Category
{
    public class OutSideDataConfigModel : IMapFrom<OutSideDataConfig>
    {
        public int ModuleType { get; set; }
        public string Url { get; set; }
        public string Key { get; set; }
        public string Description { get; set; }
        public string ModuleTypeTitle { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<OutSideDataConfigModel, OutSideDataConfig>();
            profile.CreateMap<OutSideDataConfig, OutSideDataConfigModel>()
                .ForMember(dest => dest.ModuleTypeTitle, opt => opt.MapFrom(s => ((EnumModuleType)s.ModuleType).GetEnumDescription()));
        }
    }
}
