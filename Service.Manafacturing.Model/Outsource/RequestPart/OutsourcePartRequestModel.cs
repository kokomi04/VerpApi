using AutoMapper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.ManufacturingDB;
using static VErp.Commons.Enums.Manafacturing.EnumProductionProcess;

namespace VErp.Services.Manafacturing.Model.Outsource.RequestPart
{
    public class OutsourcePartRequestModel : IMapFrom<OutsourcePartRequest>
    {
        public long OutsourcePartRequestId { get; set; }
        public string OutsourcePartRequestCode { get; set; }
        public long ProductionOrderDetailId { get; set; }
        public EnumOutsourceRequestStatusType OutsourcePartRequestStatusId { get; set; } = EnumOutsourceRequestStatusType.Unprocessed;

        public IList<OutsourcePartRequestDetailModel> Detail { get; set; }
        public long? RootProductId { get; set; }
        public long? ProductionOrderId { get; set; }
        public string ProductionOrderCode { get; set; }
        public decimal? RootProductQuantity { get; set; }

        public long OutsourcePartRequestDate { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<OutsourcePartRequestModel, OutsourcePartRequest>()
            .ForMember(m => m.CreatedDatetimeUtc, v => v.Ignore())
            .ReverseMap()
            .ForMember(m => m.OutsourcePartRequestDate, v => v.MapFrom(m => m.CreatedDatetimeUtc.GetUnix()));
        }
    }

    public class OutsourcePartRequestInfo : OutsourcePartRequestModel, IMapFrom<OutsourcePartRequestDetailInfo>
    {
        //public string ProductionOrderCode { get; set; }
        //public long ProductionOrderId { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string UnitName { get; set; }
        public int ProductOrderDetailQuantity { get; set; }
        public string ProductTitle { get; set; }
        public int ProductId { get; set; }
        public string OrderCode { get; set; }
        //public long OutsourcePartRequestDate { get; set; }
        public int UnitId { get; set; }

        public IList<OutsourcePartRequestDetailInfo> OutsourcePartRequestDetail { get; set; }

        public new void Mapping(Profile profile)
        {
            profile.CreateMap<OutsourcePartRequestDetailInfo, OutsourcePartRequestInfo>()
                .ForMember(m => m.OutsourcePartRequestDate, v => v.MapFrom(m => m.OutsourcePartRequestDate))
                .ForMember(m => m.OutsourcePartRequestId, v => v.MapFrom(m => m.OutsourcePartRequestId))
                .ForMember(m => m.OutsourcePartRequestCode, v => v.MapFrom(m => m.OutsourcePartRequestCode))
                .ForMember(m => m.ProductionOrderCode, v => v.MapFrom(m => m.ProductionOrderCode))
                .ForMember(m => m.ProductionOrderId, v => v.MapFrom(m => m.ProductionOrderId))
                .ForMember(m => m.ProductionOrderDetailId, v => v.MapFrom(m => m.ProductionOrderDetailId))
                .ForMember(m => m.ProductCode, v => v.MapFrom(m => m.ProductCode))
                .ForMember(m => m.ProductName, v => v.MapFrom(m => m.ProductName))
                .ForMember(m => m.ProductId, v => v.MapFrom(m => m.ProductId))
                .ForMember(m => m.OrderCode, v => v.MapFrom(m => m.OrderCode))
                .ForMember(m => m.ProductOrderDetailQuantity, v => v.MapFrom(m => m.ProductOrderDetailQuantity))
                .ForMember(m => m.ProductTitle, v => v.MapFrom(m => m.ProductTitle))
                .ForMember(m => m.OutsourcePartRequestStatusId, v => v.MapFrom(m => m.OutsourcePartRequestStatusId));

        }
    }

}
