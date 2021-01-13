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
        public long ProductionProcessId { get; set; }
        [Required]
        public long OutsourceStepRequestFinishDate { get; set; }
        public long OutsourceStepRequestDate { get; set; }
        public string ProductionOrderCode { get; set; }
        public string ProductionProcessTitle { get; set; }
        public bool MarkInValid { get; set; }

        public IList<OutsourceStepRequestDataModel> OutsourceStepRequestData { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<OutsourceStepRequest, OutsourceStepRequestModel>()
                .ForMember(m => m.OutsourceStepRequestFinishDate, v => v.MapFrom(m => m.OutsourceStepRequestFinishDate.GetUnix()))
                .ForMember(m => m.OutsourceStepRequestDate, v => v.MapFrom(m => m.CreatedDatetimeUtc.GetUnix()))
                .ForMember(m => m.OutsourceStepRequestData, v => v.MapFrom(m => m.OutsourceStepRequestData))
                .ForMember(m => m.ProductionOrderCode, v => v.MapFrom(m => m.ProductionOrder.ProductionOrderCode))
                .ForMember(m => m.ProductionProcessTitle, v => v.MapFrom(m => m.ProductionStep.Title))
                .ForMember(m => m.ProductionProcessId, v => v.MapFrom(m => m.ProductionStepId))
                .ReverseMap()
                .ForMember(m => m.OutsourceStepRequestFinishDate, v => v.MapFrom(m => m.OutsourceStepRequestFinishDate.UnixToDateTime()))
                .ForMember(m => m.ProductionStepId, v => v.MapFrom(m => m.ProductionProcessId))
                .ForMember(m => m.CreatedDatetimeUtc, v => v.Ignore())
                .ForMember(m => m.ProductionStep, v => v.Ignore())
                .ForMember(m => m.ProductionOrder, v => v.Ignore())
                .ForMember(m => m.OutsourceStepRequestData, v => v.Ignore());
        }
    }

    public class OutsourceStepRequestInfo : OutsourceStepRequestModel, IMapFrom<OutsourceStepRequest>
    {
        public IList<ProductionStepModel> ProductionSteps { get; set; }
        public IList<ProductionStepLinkDataRoleModel> roles { get; set; }
        public string OrderCode { get; set; }
        public string ProductTitle { get; set; }
        public string OutsourceStepRequestStatus { get; set; }

        public new  void Mapping(Profile profile)
        {
            profile.CreateMap<OutsourceStepRequest, OutsourceStepRequestInfo>()
                .ForMember(m => m.OutsourceStepRequestFinishDate, v => v.MapFrom(m => m.OutsourceStepRequestFinishDate.GetUnix()))
                .ForMember(m => m.OutsourceStepRequestDate, v => v.MapFrom(m => m.CreatedDatetimeUtc.GetUnix()))
                .ForMember(m => m.OutsourceStepRequestData, v => v.MapFrom(m => m.OutsourceStepRequestData))
                .ForMember(m => m.ProductionProcessId, v => v.MapFrom(m => m.ProductionStepId))
                .ForMember(m => m.MarkInValid, v => v.MapFrom(m => m.MarkInValid))
                .ReverseMap()
                .ForMember(m => m.OutsourceStepRequestFinishDate, v => v.MapFrom(m => m.OutsourceStepRequestFinishDate.UnixToDateTime()))
                .ForMember(m => m.CreatedDatetimeUtc, v => v.Ignore())
                .ForMember(m => m.ProductionStepId, v => v.MapFrom(m => m.ProductionProcessId))
                .ForMember(m => m.OutsourceStepRequestData, v => v.Ignore());

        }
    }


}
