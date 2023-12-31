﻿using AutoMapper;
using System.Collections.Generic;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ManufacturingDB;

namespace VErp.Services.Manafacturing.Model.Outsource.RequestPart
{
    public class OutsourcePartRequestOutput : IMapFrom<OutsourcePartRequest>
    {
        public long OutsourcePartRequestId { get; set; }
        public string OutsourcePartRequestCode { get; set; }
        public long ProductionOrderDetailId { get; set; }
        public long OutsourcePartRequestFinishDate { get; set; }
        public long OutsourcePartRequestDate { get; set; }

        public long ProductionOrderId { get; set; }

        public IList<OutsourcePartRequestDetailModel> OutsourcePartRequestDetail { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMapCustom<OutsourcePartRequest, OutsourcePartRequestOutput>()
                .ForMember(m => m.OutsourcePartRequestFinishDate, v => v.MapFrom(m => m.OutsourcePartRequestFinishDate.GetUnix()))
                .ForMember(m => m.OutsourcePartRequestDate, v => v.MapFrom(m => m.CreatedDatetimeUtc.GetUnix()))
                .ForMember(m => m.OutsourcePartRequestDetail, v => v.MapFrom(m => m.OutsourcePartRequestDetail))
                .ReverseMapCustom()
                .ForMember(m => m.OutsourcePartRequestFinishDate, v => v.MapFrom(m => m.OutsourcePartRequestFinishDate.UnixToDateTime()))
                .ForMember(m => m.OutsourcePartRequestDetail, v => v.Ignore())
                .ForMember(m => m.CreatedDatetimeUtc, v => v.Ignore());
        }
    }
}
