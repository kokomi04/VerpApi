using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ManufacturingDB;

namespace VErp.Services.Manafacturing.Model.ProductionOrder
{
    public class ProductionOrderDetailModel : IMapFrom<ProductionOrderDetail>
    {
        public int ProductionOrderDetailId { get; set; }
        public int ProductionOrderId { get; set; }
        public int? ProductId { get; set; }
        public int? Quantity { get; set; }
        public int? ReserveQuantity { get; set; }
        public string Note { get; set; }
        public long? OrderId { get; set; }
        public ProductionOrderExtraInfo ExtraInfo { get; set; }

        public EnumProductionOrderStatus? Status { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<ProductionOrderDetail, ProductionOrderDetailModel>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(source => (EnumProductionOrderStatus)source.Status))
                .ReverseMap()
                .ForMember(dest => dest.Status, opt => opt.Ignore());
        }
    }


    public class ProductionOrderExtraInfo
    {
        public int? ProductionOrderDetailId { get; set; }
        public decimal? OrderQuantity { get; set; }
        public int? OrderedQuantity { get; set; }
        public string PartnerId { get; set; }
        public string PartnerCode { get; set; }
        public string PartnerName { get; set; }
        public string OrderCode { get; set; }
        public int? ProductId { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string UnitName { get; set; }
    }
}
