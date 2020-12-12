using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ManufacturingDB;
using AutoMapper;
using static VErp.Commons.Enums.Manafacturing.EnumProductionProcess;
using VErp.Services.Manafacturing.Model.Outsource.RequestStep;
using System.ComponentModel.DataAnnotations;

namespace VErp.Services.Manafacturing.Model.Outsource.Order
{
    public class OutsourceStepOrderDetailModel: OutsourceStepRequestDataInfo, IMapFrom<OutsourceOrderDetail>
    {
        public long OutsourceOrderDetailId { get; set; }
        [Required]
        public long OutsourceOrderId { get; set; }
        [Range(0.0000001, double.MaxValue, ErrorMessage ="Số lượng phải lớn hơn 0")]
        public decimal OutsourceOrderQuantity { get; set; }
        [Required]
        public decimal OutsourceOrderPrice { get; set; }
        [Required]
        public decimal OutsourceOrderTax { get; set; }

        public new  void Mapping(Profile profile)
        {
            profile.CreateMap<OutsourceOrderDetail, OutsourceStepOrderDetailModel>()
                .ForMember(m => m.ProductionStepLinkDataId, v => v.MapFrom(m => m.ObjectId))
                .ForMember(m => m.OutsourceOrderPrice, v => v.MapFrom(m => m.Price))
                .ForMember(m => m.OutsourceOrderQuantity, v => v.MapFrom(m => m.Quantity))
                .ForMember(m => m.OutsourceOrderTax, v => v.MapFrom(m => m.Tax))
                .ReverseMap()
                .ForMember(m => m.ObjectId, v => v.MapFrom(m => m.ProductionStepLinkDataId))
                .ForMember(m => m.Price, v => v.MapFrom(m => m.OutsourceOrderPrice))
                .ForMember(m => m.Quantity, v => v.MapFrom(m => m.OutsourceOrderQuantity))
                .ForMember(m => m.Tax, v => v.MapFrom(m => m.OutsourceOrderTax));

        }
    }
}
