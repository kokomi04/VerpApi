using AutoMapper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.ManufacturingDB;
using VErp.Services.Manafacturing.Model.ProductionStep;

namespace VErp.Services.Manafacturing.Model.Outsource.RequestStep
{
    public class OutsourceStepRequestModel : IMapFrom<OutsourceStepRequest>
    {
        public long OutsourceStepRequestId { get; set; }
        public string OutsourceStepRequestCode { get; set; }
        [Required]
        public long ProductionOrderId { get; set; }
        [Required]
        public long OutsourceStepRequestFinishDate { get; set; }
        public long OutsourceStepRequestDate { get; set; }

        public IList<OutsourceStepRequestDataModel> OutsourceStepRequestData { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<OutsourceStepRequest, OutsourceStepRequestModel>()
                .ForMember(m => m.OutsourceStepRequestFinishDate, v => v.MapFrom(m => m.OutsourceStepRequestFinishDate.GetUnix()))
                .ForMember(m => m.OutsourceStepRequestDate, v => v.MapFrom(m => m.CreatedDatetimeUtc.GetUnix()))
                .ForMember(m => m.OutsourceStepRequestData, v => v.MapFrom(m => m.OutsourceStepRequestData))
                .ReverseMap()
                .ForMember(m => m.OutsourceStepRequestFinishDate, v => v.MapFrom(m => m.OutsourceStepRequestFinishDate.UnixToDateTime()))
                .ForMember(m => m.CreatedDatetimeUtc, v => v.Ignore())
                .ForMember(m => m.OutsourceStepRequestData, v => v.Ignore());

        }
    }

    public class OutsourceStepRequestInfo : OutsourceStepRequestModel, IMapFrom<OutsourceStepRequest>
    {
        public IList<ProductionStepModel> ProductionSteps { get; set; }
        public IList<ProductionStepLinkDataRoleModel> roles { get; set; }
        public string OrderCode { get; set; }
        public string ProductTitle { get; set; }

        public new  void Mapping(Profile profile)
        {
            profile.CreateMap<OutsourceStepRequest, OutsourceStepRequestInfo>()
                .ForMember(m => m.OutsourceStepRequestFinishDate, v => v.MapFrom(m => m.OutsourceStepRequestFinishDate.GetUnix()))
                .ForMember(m => m.OutsourceStepRequestDate, v => v.MapFrom(m => m.CreatedDatetimeUtc.GetUnix()))
                .ForMember(m => m.OutsourceStepRequestData, v => v.MapFrom(m => m.OutsourceStepRequestData))
                .ReverseMap()
                .ForMember(m => m.OutsourceStepRequestFinishDate, v => v.MapFrom(m => m.OutsourceStepRequestFinishDate.UnixToDateTime()))
                .ForMember(m => m.CreatedDatetimeUtc, v => v.Ignore())
                .ForMember(m => m.OutsourceStepRequestData, v => v.Ignore());

        }
    }
}
