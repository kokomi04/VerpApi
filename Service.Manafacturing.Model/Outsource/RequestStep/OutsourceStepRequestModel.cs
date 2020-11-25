using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.ManufacturingDB;

namespace VErp.Services.Manafacturing.Model.Outsource.RequestStep
{
    public class OutsourceStepRequestModel : IMapFrom<OutsourceStepRequest>
    {
        public long OutsourceStepRequestId { get; set; }
        public string OutsourceStepRequestCode { get; set; }
        public long ProductionOrderId { get; set; }
        public long OutsourceStepRequestFinishDate { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<OutsourceStepRequest, OutsourceStepRequestModel>()
                .ForMember(m => m.OutsourceStepRequestFinishDate, v => v.MapFrom(m => m.OutsourceStepRequestFinishDate.GetUnix()))
                .ReverseMap()
                .ForMember(m => m.OutsourceStepRequestFinishDate, v => v.MapFrom(m => m.OutsourceStepRequestFinishDate.UnixToDateTime()));
            
        }
    }

    public class OutsourceStepRequestInfo : OutsourceStepRequestModel
    {
        public IList<OutsourceStepRequestDataInfo> OutsourceStepRequestData { get; set; }

        public new void Mapping(Profile profile)
        {
            profile.CreateMap<OutsourceStepRequest, OutsourceStepRequestInfo>()
                .ForMember(m => m.OutsourceStepRequestFinishDate, v => v.MapFrom(m => m.OutsourceStepRequestFinishDate.GetUnix()))
                .ForMember(m => m.OutsourceStepRequestData, v => v.MapFrom(m => m.OutsourceStepRequestData))
                .ReverseMap()
                .ForMember(m => m.OutsourceStepRequestFinishDate, v => v.MapFrom(m => m.OutsourceStepRequestFinishDate.UnixToDateTime()))
                .ForMember(m => m.OutsourceStepRequestData, v => v.Ignore());

        }
    }
}
