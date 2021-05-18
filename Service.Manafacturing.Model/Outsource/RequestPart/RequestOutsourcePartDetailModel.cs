using AutoMapper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.ManufacturingDB;
using static VErp.Commons.Enums.Manafacturing.EnumProductionProcess;

namespace VErp.Services.Manafacturing.Model.Outsource.RequestPart
{
    public class RequestOutsourcePartDetailModel : IMapFrom<OutsourcePartRequestDetail>
    {
        public long OutsourcePartRequestDetailId { get; set; }
        public long OutsourcePartRequestId { get; set; }

        public int ProductPartId { get; set; }

        public string PathProductIdInBom { get; set; }

        public decimal Quantity { get; set; }

        public long? OutsourcePartRequestDetailFinishDate { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<OutsourcePartRequestDetail, RequestOutsourcePartDetailModel>()
                .ForMember(m => m.ProductPartId, v => v.MapFrom(m => m.ProductId))
                .ForMember(m => m.OutsourcePartRequestDetailFinishDate, v => v.MapFrom(m => m.OutsourcePartRequestDetailFinishDate.GetUnix()))
                .ReverseMap()
                .ForMember(m => m.ProductId, v => v.MapFrom(m => m.ProductPartId))
                .ForMember(m => m.OutsourcePartRequestDetailFinishDate, v => v.MapFrom(m => m.OutsourcePartRequestDetailFinishDate.UnixToDateTime()));
        }
    }

}
