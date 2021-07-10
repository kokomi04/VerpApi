using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ManufacturingDB;

namespace VErp.Services.Manafacturing.Model.Outsource.Order.Part
{
    public class OutsourcePartOrderDetailOutput : IMapFrom<OutsourceOrderDetail>
    {
        public long OutsourceOrderDetailId { get; set; }
        public long OutsourceOrderId { get; set; }
        public long OutsourcePartRequestDetailId { get; set; }
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Tax { get; set; }
        
        public int? ProductUnitConversionId { get; set; }
        public decimal? ProductUnitConversionQuantity { get; set; }
        public decimal? ProductUnitConversionPrice { get; set; }

        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public int UnitId { get; set; }
        public int? DecimalPlace { get; set; }
        public int? ProductId { get; set; }
        public decimal? QuantityOrigin { get; set; }
        public long? OutsourcePartRequestDetailFinishDate { get; set; }
        public string OutsourcePartRequestCode { get; set; }
        public decimal? QuantityProcessed { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<OutsourceOrderDetail, OutsourcePartOrderDetailOutput>()
                .ForMember(m => m.OutsourcePartRequestDetailId, v => v.MapFrom(m => m.ObjectId))
                .ReverseMap()
                .ForMember(m => m.ObjectId, v => v.MapFrom(m => m.OutsourcePartRequestDetailId));
        }
    }
}
