using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ManufacturingDB;

namespace VErp.Services.Manafacturing.Model.Outsource.Order.Part
{
    public class OutsourcePartOrderDetailInput: IMapFrom<OutsourceOrderDetail>
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

        public void Mapping(Profile profile)
        {
            profile.CreateMap<OutsourceOrderDetail, OutsourcePartOrderDetailInput>()
                .ForMember(m => m.OutsourcePartRequestDetailId, v => v.MapFrom(m => m.ObjectId))
                .ReverseMap()
                .ForMember(m => m.ObjectId, v => v.MapFrom(m => m.OutsourcePartRequestDetailId));
        }
    }
}
