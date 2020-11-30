using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ManufacturingDB;
using AutoMapper;
using static VErp.Commons.Enums.Manafacturing.EnumProductionProcess;

namespace VErp.Services.Manafacturing.Model.Outsource.Order
{
    public class OutsourceStepOrderDetailModel: IMapFrom<OutsourceOrderDetail>
    {
        public long OutsourceOrderDetailId { get; set; }
        public long OutsourceOrderId { get; set; }
        public long ProductionStepLinkDataId { get; set; }
        public decimal OutsourceOrderQuantity { get; set; }
        public decimal OutsourceOrderPrice { get; set; }
        public decimal OutsourceOrderTax { get; set; }
        public EnumProductionStepLinkDataRoleType productionStepLinkDataRoleTypeId { get; set; }

        public void Mapping(Profile profile)
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
