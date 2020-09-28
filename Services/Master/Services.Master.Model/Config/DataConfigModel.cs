using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.MasterDB;

namespace VErp.Services.Master.Model.Config
{
    public class DataConfigModel : IMapFrom<DataConfig>
    {
        public int SubsidiaryId { get; set; }
        public long ClosingDate { get; set; }
        public bool AutoClosingDate { get; set; }
        public FreqClosingDate FreqClosingDate { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<DataConfig, DataConfigModel>()
                .ForMember(m => m.FreqClosingDate, m => m.MapFrom(v => v.FreqClosingDate.JsonDeserialize<FreqClosingDate>()))
                .ForMember(m => m.ClosingDate, m => m.MapFrom(v => v.ClosingDate.GetUnix()))
                .ReverseMap()
                .ForMember(d => d.FreqClosingDate, m => m.MapFrom(v => v.FreqClosingDate.JsonSerialize()))
                .ForMember(m => m.ClosingDate, m => m.MapFrom(v => v.ClosingDate.UnixToDateTime()));
        }
    }
}
