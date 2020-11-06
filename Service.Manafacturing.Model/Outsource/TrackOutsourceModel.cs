using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.ManufacturingDB;

namespace VErp.Services.Manafacturing.Model.Outsource
{
    public class TrackOutsourceModel: IMapFrom<TrackOutsource>
    {
        public int TrackOutsourceId { get; set; }
        public EnumTrackOutsourceType OutsourceType { get; set; }
        public int OutsourceId { get; set; }
        public string Description { get; set; }
        public EnumTrackOutsourceStatus Status { get; set; }
        public long DateTrack { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<TrackOutsource, TrackOutsourceModel>()
                .ForMember(m => m.DateTrack, v => v.MapFrom(m => m.DateTrack.GetUnix()))
                .ReverseMap()
                .ForMember(m => m.DateTrack, v => v.MapFrom(m => m.DateTrack.UnixToDateTime()));
        }
    }
}
