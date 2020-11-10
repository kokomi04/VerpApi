using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.ManufacturingDB;

namespace VErp.Services.Manafacturing.Model.Outsource.RequestPart
{
    public class RequestOutsourcePartModel : IMapFrom<RequestOutsourcePart>
    {
        public int RequestOutsourcePartId { get; set; }
        public string RequestOutsourcePartCode { get; set; }
        public int ProductionOrderDetailId { get; set; }
        public long CreateDateRequest { get; set; }
        public long DateRequiredComplete { get; set; }

        public IList<RequestOutsourcePartDetailModel> RequestOutsourcePartDetail { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<RequestOutsourcePart, RequestOutsourcePartModel>()
                .ForMember(m => m.CreateDateRequest, v => v.MapFrom(m => m.CreatedDatetimeUtc.GetUnix()))
                .ForMember(m => m.DateRequiredComplete, v => v.MapFrom(m => m.DateRequiredComplete.GetUnix()))
                .ForMember(m => m.RequestOutsourcePartDetail, v => v.MapFrom(m => m.RequestOutsourcePartDetail))
                .ReverseMap()
                .ForMember(m => m.CreatedDatetimeUtc, v => v.Ignore())
                .ForMember(m => m.DateRequiredComplete, v => v.MapFrom(m => m.DateRequiredComplete.UnixToDateTime()))
                .ForMember(m => m.RequestOutsourcePartDetail, v => v.Ignore());
        }
    }
}
