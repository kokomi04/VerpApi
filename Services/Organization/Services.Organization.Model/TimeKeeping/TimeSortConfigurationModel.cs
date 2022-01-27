using System;
using System.Collections.Generic;
using AutoMapper;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.OrganizationDB;

namespace Services.Organization.Model.TimeKeeping
{
    public class TimeSortConfigurationModel: IMapFrom<TimeSortConfiguration>
    {
        public int TimeSortConfigurationId { get; set; }
        public string TimeSortCode { get; set; }
        public string TimeSortDescription { get; set; }
        public int TimeSortType { get; set; }
        public long MinMinutes { get; set; }
        public long MaxMinutes { get; set; }
        public long BetweenMinutes { get; set; }
        public int NumberOfCycles { get; set; }
        public double TimeEndCycles { get; set; }
        public bool IsIgnoreNightShift { get; set; }
        public double StartTimeIgnoreTimeShift { get; set; }
        public double EndTimeIgnoreTimeShift { get; set; }
        public bool IsApplySpecialCase { get; set; }

        public IList<SplitHourModel> SplitHour { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<TimeSortConfiguration, TimeSortConfigurationModel>()
            .ForMember(m=> m.TimeEndCycles, v=>v.MapFrom(m=>m.TimeEndCycles.TotalSeconds))
            .ForMember(m=> m.StartTimeIgnoreTimeShift, v=>v.MapFrom(m=>m.StartTimeIgnoreTimeShift.TotalSeconds))
            .ForMember(m=> m.EndTimeIgnoreTimeShift, v=>v.MapFrom(m=>m.EndTimeIgnoreTimeShift.TotalSeconds))
            .ForMember(m=> m.SplitHour, v=>v.MapFrom(m => m.SplitHour))
            .ReverseMap()
            .ForMember(m => m.TimeEndCycles, v => v.MapFrom(m => TimeSpan.FromSeconds(m.TimeEndCycles)))
            .ForMember(m => m.StartTimeIgnoreTimeShift, v => v.MapFrom(m => TimeSpan.FromSeconds(m.StartTimeIgnoreTimeShift)))
            .ForMember(m => m.EndTimeIgnoreTimeShift, v => v.MapFrom(m => TimeSpan.FromSeconds(m.EndTimeIgnoreTimeShift)))
            .ForMember(m => m.SplitHour, v => v.Ignore());
        }
    }
}