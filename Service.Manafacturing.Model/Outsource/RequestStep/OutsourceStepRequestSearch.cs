using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;

namespace VErp.Services.Manafacturing.Model.Outsource.RequestStep
{
    public class OutsourceStepRequestSearch: OutsourceStepRequestProductionStepInfo, IMapFrom<OutsourceStepRequestEntity>
    {
        public long OutsourceStepRequestId { get; set; }
        public string OutsourceStepRequestCode { get; set; }
        public string ProductionStepTitle { get; set; }
        public long ProductionOrderId { get; set; }
        public long OutsourceStepRequestFinishDate { get; set; }
        public long OutsourceStepRequestDate { get; set; }
        public string ProductTitle { get; set; }
        public string ProductionStepInRequestStatus { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<OutsourceStepRequestEntity, OutsourceStepRequestSearch>()
                .ForMember(m => m.OutsourceStepRequestFinishDate, v => v.MapFrom(m => m.OutsourceStepRequestFinishDate.GetUnix()))
                .ForMember(m => m.OutsourceStepRequestDate, v => v.MapFrom(m => m.OutsourceStepRequestDate.GetUnix()))
                .ReverseMap()
                .ForMember(m => m.OutsourceStepRequestFinishDate, v => v.MapFrom(m => m.OutsourceStepRequestFinishDate.UnixToDateTime()))
                .ForMember(m => m.OutsourceStepRequestDate, v => v.MapFrom(m => m.OutsourceStepRequestDate.UnixToDateTime()));
        }
    }

    public class OutsourceStepRequestEntity : OutsourceStepRequestProductionStepInfo
    {
        public long OutsourceStepRequestId { get; set; }
        public string OutsourceStepRequestCode { get; set; }
        public long ProductionOrderId { get; set; }
        public DateTime OutsourceStepRequestFinishDate { get; set; }
        public DateTime OutsourceStepRequestDate { get; set; }
        public string ProductionStepTitle { get; set; }
        public string ProductTitle { get; set; }
    }
    
}
