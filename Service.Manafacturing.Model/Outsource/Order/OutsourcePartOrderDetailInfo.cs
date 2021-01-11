using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;

namespace VErp.Services.Manafacturing.Model.Outsource.Order
{
    public class OutsourcePartOrderDetailInfo: OutsourcePartOrderDetailExtractInfoBase, IMapFrom<OutsourcePartOrderDetailExtractInfo>
    {
        public long OutsourcePartRequestFinishDate { get; set; }
        public long OutsourceOrderDate { get; set; }
        public long OutsourceOrderFinishDate { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<OutsourcePartOrderDetailExtractInfo, OutsourcePartOrderDetailInfo>()
                .ForMember(m => m.OutsourcePartRequestFinishDate, v => v.MapFrom(m => m.OutsourcePartRequestFinishDate.GetUnix()))
                .ForMember(m => m.OutsourceOrderDate, v => v.MapFrom(m => m.OutsourceOrderDate.GetUnix()))
                .ForMember(m => m.OutsourceOrderFinishDate, v => v.MapFrom(m => m.OutsourceOrderFinishDate.GetUnix()))
                .ReverseMap()
                .ForMember(m => m.OutsourcePartRequestFinishDate, v => v.MapFrom(m => m.OutsourcePartRequestFinishDate.UnixToDateTime()))
                .ForMember(m => m.OutsourceOrderDate, v => v.MapFrom(m => m.OutsourceOrderDate.UnixToDateTime()))
                .ForMember(m => m.OutsourceOrderFinishDate, v => v.MapFrom(m => m.OutsourceOrderFinishDate.UnixToDateTime()));
        }
    }

    public class OutsourcePartOrderDetailExtractInfo: OutsourcePartOrderDetailExtractInfoBase
    {
        public DateTime OutsourcePartRequestFinishDate { get; set; }
        public DateTime OutsourceOrderDate { get; set; }
        public DateTime OutsourceOrderFinishDate { get; set; }
    }

    public class OutsourcePartOrderDetailExtractInfoBase: OutsourceOrderBase
    {
        public long OutsourceOrderDetailId { get; set; }
        public long ObjectId { get; set; }
        public decimal Price { get; set; }
        public decimal Tax { get; set; }
        public string ProductTitle { get; set; }
        public string ProductPartName { get; set; }
        public string ProductPartCode { get; set; }
        public string ProductionOrderCode { get; set; }
        public string OrderCode { get; set; }
        public string OutsourcePartRequestCode { get; set; }
        public string UnitName { get; set; }
        public decimal Quantity { get; set; }
        public decimal QuantityOrigin { get; set; }
        public decimal QuantityProcessed { get; set; }
    }
}
