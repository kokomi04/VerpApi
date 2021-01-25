using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using static VErp.Commons.Enums.Manafacturing.EnumProductionProcess;

namespace VErp.Services.Manafacturing.Model.Outsource.RequestPart
{
    public class OutsourcePartRequestDetailInfo : OutsourcePartRequestDetailBase, IMapFrom<OutsourcePartRequestDetailExtractInfo>
    {
        public long OutsourcePartRequestDate { get; set; }
        public long OutsourcePartRequestFinishDate { get; set; }

        public new void Mapping(Profile profile)
        {
            profile.CreateMap<OutsourcePartRequestDetailExtractInfo, OutsourcePartRequestDetailInfo>()
                .ForMember(m => m.OutsourcePartRequestDate, v => v.MapFrom(m => m.OutsourcePartRequestDate.GetUnix()))
                .ForMember(m => m.OutsourcePartRequestFinishDate, v => v.MapFrom(m => m.OutsourcePartRequestFinishDate.GetUnix()))
                .ReverseMap()
                .ForMember(m => m.OutsourcePartRequestDate, v => v.MapFrom(m => m.OutsourcePartRequestDate.UnixToDateTime()))
                .ForMember(m => m.OutsourcePartRequestFinishDate, v => v.MapFrom(m => m.OutsourcePartRequestFinishDate.UnixToDateTime()));
        }
    }

    public class OutsourcePartRequestDetailExtractInfo : OutsourcePartRequestDetailBase
    {
        public DateTime OutsourcePartRequestDate { get; set; }
        public DateTime OutsourcePartRequestFinishDate { get; set; }
    }

    public class OutsourcePartRequestDetailBase : RequestOutsourcePartDetailModel
    {
        public string OutsourcePartRequestCode { get; set; }
        public long ProductionOrderDetailId { get; set; }
        public string ProductionOrderCode { get; set; }
        public long ProductionOrderId { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string UnitName { get; set; }
        public string ProductPartName { get; set; }
        public string ProductPartCode { get; set; }
        public string ProductPartTitle { get; set; }
        public string OrderCode { get; set; }
        public int ProductId { get; set; }
        public decimal ProductOrderDetailQuantity { get; set; }
        public string ProductTitle { get; set; }
        public decimal QuantityProcessed { get; set; }
        public EnumOutsourceRequestStatusType OutsourcePartRequestStatusId { get; set; }
        public bool MarkInvalid { get; set; }
    }

}
