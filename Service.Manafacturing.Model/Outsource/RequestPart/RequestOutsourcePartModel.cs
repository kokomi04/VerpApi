using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.ManufacturingDB;
using static VErp.Commons.Enums.Manafacturing.EnumProductionProcess;

namespace VErp.Services.Manafacturing.Model.Outsource.RequestPart
{
    public class RequestOutsourcePartModel : IMapFrom<RequestOutsourcePart>
    {
        public int RequestOutsourcePartId { get; set; }
        public string RequestOutsourcePartCode { get; set; }
        public int ProductionOrderDetailId { get; set; }
        public long CreateDateRequest { get; set; }
        public long DateRequiredComplete { get; set; }
        public int ProductId { get; set; }
        public int Quanity { get; set; }
        public int UnitId { get; set; }
        public OutsourcePartProcessType Status { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<RequestOutsourcePart, RequestOutsourcePartModel>()
                .ForMember(m => m.CreateDateRequest, v => v.MapFrom(m => m.CreatedDatetimeUtc.GetUnix()))
                .ForMember(m => m.DateRequiredComplete, v => v.MapFrom(m => m.DateRequiredComplete.GetUnix()))
                .ReverseMap()
                .ForMember(m => m.CreatedDatetimeUtc, v => v.Ignore())
                .ForMember(m => m.DateRequiredComplete, v => v.MapFrom(m => m.DateRequiredComplete.UnixToDateTime()));
        }
    }

    public class RequestOutsourcePartInfo: RequestOutsourcePartModel
    {
        public string ProductionOrderCode { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string UnitName { get; set; }
        public string ProductPartName { get; set; }
        public string ProductPartCode { get; set; }
        public string OrderCode { get; set; }

    }


}
