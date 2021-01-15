using AutoMapper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ManufacturingDB;

namespace VErp.Services.Manafacturing.Model.Outsource.Order
{
    public class OutsourceOrderDetailModel: IMapFrom<OutsourceOrderDetail>
    {
        public long OutsourceOrderDetailId { get; set; }
        public long OutsourceOrderId { get; set; }
        [Required]
        public long ObjectId { get; set; }
        [Required]
        [Range(0.00001, double.MaxValue)]
        public decimal Quantity { get; set; }
        [Required]
        public decimal Price { get; set; }
        [Required]
        public decimal Tax { get; set; }
    }

    public class OutsourceOrderDetailInfo: OutsourceOrderDetailModel, IMapFrom<OutsourcePartOrderDetailInfo>
    {
        public string ProductPartCode { get; set; }
        public string ProductPartName { get; set; }
        public string UnitName { get; set; }
        public decimal QuantityOrigin { get; set; }
        public decimal QuantityProcessed{ get; set; }
        public string OutsourcePartRequestCode { get; set; }
        public long OutsourcePartRequestId{ get; set; }
        public long OutsourcePartRequestFinishDate { get; set; }

        public new void Mapping(Profile profile)
        {
            profile.CreateMap<OutsourcePartOrderDetailInfo, OutsourceOrderDetailInfo>()
                .ForMember(m => m.OutsourceOrderDetailId, v => v.MapFrom(m => m.OutsourceOrderDetailId))
                .ForMember(m => m.OutsourceOrderId, v => v.MapFrom(m => m.OutsourceOrderId))
                .ForMember(m => m.ObjectId, v => v.MapFrom(m => m.ObjectId))
                .ForMember(m => m.Price, v => v.MapFrom(m => m.Price))
                .ForMember(m => m.Tax, v => v.MapFrom(m => m.Tax))
                .ForMember(m => m.ProductPartName, v => v.MapFrom(m => m.ProductPartName))
                .ForMember(m => m.ProductPartCode, v => v.MapFrom(m => m.ProductPartCode))
                .ForMember(m => m.OutsourcePartRequestCode, v => v.MapFrom(m => m.OutsourcePartRequestCode))
                .ForMember(m => m.OutsourcePartRequestId, v => v.MapFrom(m => m.OutsourcePartRequestId))
                .ForMember(m => m.OutsourcePartRequestFinishDate, v => v.MapFrom(m => m.OutsourcePartRequestFinishDate))
                .ForMember(m => m.UnitName, v => v.MapFrom(m => m.UnitName))
                .ForMember(m => m.Quantity, v => v.MapFrom(m => m.Quantity))
                .ReverseMap();
        }
    }
}
