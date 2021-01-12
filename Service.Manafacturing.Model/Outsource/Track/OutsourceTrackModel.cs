using AutoMapper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.ManufacturingDB;
using static VErp.Commons.Enums.Manafacturing.EnumOutsourceTrack;

namespace VErp.Services.Manafacturing.Model.Outsource.Track
{
    public class OutsourceTrackModel: IMapFrom<OutsourceTrack>
    {
        public long OutsourceTrackId { get; set; }
        [Required]
        public long OutsourceOrderId { get; set; }
        [Required]
        public EnumOutsourceTrackType OutsourceTrackTypeId { get; set; }
        [Required]
        public long OutsourceTrackDate { get; set; }
        public long? ObjectId { get; set; }
        public string OutsourceTrackDescription { get; set; }
        [Required]
        public EnumOutsourceTrackStatus OutsourceTrackStatusId { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<OutsourceTrack, OutsourceTrackModel>()
                .ForMember(m => m.OutsourceTrackDate, v => v.MapFrom(m => m.OutsourceTrackDate.GetUnix()))
                .ForMember(m => m.OutsourceTrackDescription, v => v.MapFrom(m => m.Description))
                .ReverseMap()
                .ForMember(m => m.OutsourceTrackDate, v => v.MapFrom(m => m.OutsourceTrackDate.UnixToDateTime()))
                .ForMember(m => m.Description, v => v.MapFrom(m => m.OutsourceTrackDescription));
        }
    }

}
