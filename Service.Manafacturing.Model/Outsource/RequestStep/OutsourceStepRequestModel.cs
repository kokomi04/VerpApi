﻿using AutoMapper;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ManufacturingDB;
using static VErp.Commons.Enums.Manafacturing.EnumProductionProcess;

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
        public bool IsInvalid { get; set; }
        public EnumOutsourceRequestStatusType OutsourceStepRequestStatusId { get; set; }

        public IList<OutsourceStepRequestDataModel> OutsourceStepRequestData { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<OutsourceStepRequest, OutsourceStepRequestModel>()
                .ForMember(m => m.OutsourceStepRequestFinishDate, v => v.MapFrom(m => m.OutsourceStepRequestFinishDate.GetUnix()))
                .ForMember(m => m.OutsourceStepRequestDate, v => v.MapFrom(m => m.CreatedDatetimeUtc.GetUnix()))
                .ForMember(m => m.OutsourceStepRequestData, v => v.MapFrom(m => m.OutsourceStepRequestData))
                .ForMember(m => m.ProductionOrderCode, v => v.MapFrom(m => m.ProductionOrder.ProductionOrderCode))
                .ReverseMap()
                .ForMember(m => m.OutsourceStepRequestFinishDate, v => v.MapFrom(m => m.OutsourceStepRequestFinishDate.UnixToDateTime()))
                .ForMember(m => m.CreatedDatetimeUtc, v => v.Ignore())
                .ForMember(m => m.ProductionOrder, v => v.Ignore())
                .ForMember(m => m.OutsourceStepRequestData, v => v.Ignore());
        }
    }

    public class OutsourceStepRequestPrivateKey
    {
        public long OutsourceStepRequestId { get; set; }
        public string OutsourceStepRequestCode { get; set; }
    }
}
