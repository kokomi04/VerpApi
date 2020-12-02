using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.ManufacturingDB;

namespace VErp.Services.Manafacturing.Model.Outsource.Track
{
    public class OutsourceTrackModel: IMapFrom<OutsourceTrack>
    {
        public long OutsourceTrackId { get; set; }
        public long OutsourceOrderId { get; set; }
        public int OutsourceTrackTypeId { get; set; }
        public long OutsourceTrackDate { get; set; }
        public long ObjectId { get; set; }
        public string OutsourceTrackDescription { get; set; }
        public int OutsourceTrackStatusId { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<OutsourceTrack, OutsourceTrackModel>()
                .ForMember(m => m.OutsourceTrackDate, v => v.MapFrom(m => m.OutsourceTrackDate.GetUnix()))
                .ReverseMap()
                .ForMember(m => m.OutsourceTrackDate, v => v.MapFrom(m => m.OutsourceTrackDate.UnixToDateTime()));
        }
    }

}
