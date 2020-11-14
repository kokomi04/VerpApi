using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.ManufacturingDB;

namespace VErp.Services.Manafacturing.Model.Outsource.RequestPart
{
    public class RequestOutsourcePartModel: IMapFrom<RequestOutsourcePart>
    {
        public int RequestOutsourcePartId { get; set; }
        public string RequestOutsourcePartCode { get; set; }
        public int ProductionOrderDetailId { get; set; }
        public long CreateDateRequest { get; set; }
        public long DateRequiredComplete { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<RequestOutsourcePart, RequestOutsourcePartModel>()
                .ForMember(m => m.CreateDateRequest, v => v.MapFrom(m => m.CreatedDatetimeUtc.GetUnix()))
                .ForMember(m => m.DateRequiredComplete, v => v.MapFrom(m => m.DateRequiredComplete.GetUnix()))
                .ReverseMap()
                .ForMember(m => m.CreatedDatetimeUtc, v => v.Ignore());
        }
    }

    public class RequestOutsourcePartInfo: RequestOutsourcePartModel, IMapFrom<RequestOutsourcePartDetailInfo>
    {
        public string ProductionOrderCode { get; set; }

        public IList<RequestOutsourcePartDetailInfo> RequestOutsourcePartDetail { get; set; }
    }

}
