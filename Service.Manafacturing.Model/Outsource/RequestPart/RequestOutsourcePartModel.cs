﻿using AutoMapper;
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
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string Status { get; set; }

        public IList<RequestOutsourcePartDetailInfo> RequestOutsourcePartDetail { get; set; }

        public new void Mapping(Profile profile) {
            profile.CreateMap<RequestOutsourcePartDetailInfo, RequestOutsourcePartInfo>()
                .ForMember(m => m.CreateDateRequest, v => v.MapFrom(m => m.CreateDateRequest))
                .ForMember(m => m.DateRequiredComplete, v => v.MapFrom(m => m.DateRequiredComplete))
                .ForMember(m => m.RequestOutsourcePartId, v => v.MapFrom(m => m.RequestOutsourcePartId))
                .ForMember(m => m.RequestOutsourcePartCode, v => v.MapFrom(m => m.RequestOutsourcePartCode))
                .ForMember(m => m.ProductionOrderCode, v => v.MapFrom(m => m.ProductionOrderCode))
                .ForMember(m => m.ProductionOrderDetailId, v => v.MapFrom(m => m.ProductionOrderDetailId))
                .ForMember(m => m.ProductCode, v => v.MapFrom(m => m.ProductCode))
                .ForMember(m => m.ProductName, v => v.MapFrom(m => m.ProductName));
                
        }
    }

}
