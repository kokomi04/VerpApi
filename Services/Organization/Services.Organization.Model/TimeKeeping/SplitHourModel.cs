using AutoMapper;
using System;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.OrganizationDB;

namespace Services.Organization.Model.TimeKeeping
{
    public class SplitHourModel : IMapFrom<SplitHour>
    {
        public int SplitHourId { get; set; }
        public int TimeSortConfigurationId { get; set; }
        public double StartTimeOn { get; set; }
        public double EndTimeOn { get; set; }
        public double StartTimeOut { get; set; }
        public double EndTimeOut { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<SplitHour, SplitHourModel>()
            .ForMember(m => m.StartTimeOn, v => v.MapFrom(m => m.StartTimeOn.TotalSeconds))
            .ForMember(m => m.EndTimeOn, v => v.MapFrom(m => m.EndTimeOn.TotalSeconds))
            .ForMember(m => m.StartTimeOut, v => v.MapFrom(m => m.StartTimeOut.TotalSeconds))
            .ForMember(m => m.EndTimeOut, v => v.MapFrom(m => m.EndTimeOut.TotalSeconds))
            .ReverseMap()
            .ForMember(m => m.StartTimeOn, v => v.MapFrom(m => TimeSpan.FromSeconds(m.StartTimeOn)))
            .ForMember(m => m.EndTimeOn, v => v.MapFrom(m => TimeSpan.FromSeconds(m.EndTimeOn)))
            .ForMember(m => m.StartTimeOut, v => v.MapFrom(m => TimeSpan.FromSeconds(m.StartTimeOut)))
            .ForMember(m => m.EndTimeOut, v => v.MapFrom(m => TimeSpan.FromSeconds(m.EndTimeOut)));
        }
    }
}