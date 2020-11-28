using AutoMapper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.ManufacturingDB;

namespace VErp.Services.Manafacturing.Model.Outsource.RequestPart
{
    public class RequestOutsourcePartModel: IMapFrom<OutsourcePartRequest>
    {
        public long OutsourcePartRequestId { get; set; }
        public string OutsourcePartRequestCode { get; set; }
        public long ProductionOrderDetailId { get; set; }
        public long OutsourcePartRequestDate { get; set; }
        [Required]
        public long OutsourcePartRequestFinishDate { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<OutsourcePartRequest, RequestOutsourcePartModel>()
                .ForMember(m => m.OutsourcePartRequestDate, v => v.MapFrom(m => m.CreatedDatetimeUtc.GetUnix()))
                .ForMember(m => m.OutsourcePartRequestFinishDate, v => v.MapFrom(m => m.OutsourcePartRequestFinishDate.GetUnix()))
                .ReverseMap()
                .ForMember(m => m.CreatedDatetimeUtc, v => v.Ignore())
                .ForMember(m => m.OutsourcePartRequestFinishDate, v => v.MapFrom(m => m.OutsourcePartRequestFinishDate.UnixToDateTime()));
        }
    }

    public class RequestOutsourcePartInfo: RequestOutsourcePartModel, IMapFrom<RequestOutsourcePartDetailInfo>
    {
        public string ProductionOrderCode { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string Status { get; set; }
        public int ProductOrderDetailQuantity { get; set; }
        public string ProductTitle { get; set; }
        public int ProductId { get; set; }
        public string OrderCode { get; set; }

        public IList<RequestOutsourcePartDetailInfo> OutsourcePartRequestDetail { get; set; }

        public new void Mapping(Profile profile) {
            profile.CreateMap<RequestOutsourcePartDetailInfo, RequestOutsourcePartInfo>()
                .ForMember(m => m.OutsourcePartRequestDate, v => v.MapFrom(m => m.OutsourcePartRequestDate))
                .ForMember(m => m.OutsourcePartRequestFinishDate, v => v.MapFrom(m => m.OutsourcePartRequestFinishDate))
                .ForMember(m => m.OutsourcePartRequestId, v => v.MapFrom(m => m.OutsourcePartRequestId))
                .ForMember(m => m.OutsourcePartRequestCode, v => v.MapFrom(m => m.OutsourcePartRequestCode))
                .ForMember(m => m.ProductionOrderCode, v => v.MapFrom(m => m.ProductionOrderCode))
                .ForMember(m => m.ProductionOrderDetailId, v => v.MapFrom(m => m.ProductionOrderDetailId))
                .ForMember(m => m.ProductCode, v => v.MapFrom(m => m.ProductCode))
                .ForMember(m => m.ProductName, v => v.MapFrom(m => m.ProductName))
                .ForMember(m => m.ProductId, v => v.MapFrom(m => m.ProductId))
                .ForMember(m => m.OrderCode, v => v.MapFrom(m => m.OrderCode))
                .ForMember(m => m.ProductOrderDetailQuantity, v => v.MapFrom(m => m.ProductOrderDetailQuantity))
                .ForMember(m => m.ProductTitle, v => v.MapFrom(m => m.ProductTitle));
                
        }
    }

}
