using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ManufacturingDB;
using AutoMapper;
using VErp.Commons.Library;

namespace VErp.Services.Manafacturing.Model.ProductionOrder
{

    public class ProductionOrderListModel : ProductionOrderDetailModel, IMapFrom<ProductionOrderDetail>
    {
        public string ProductionOrderCode { get; set; }
        public long VoucherDate { get; set; }
        public long? FinishDate { get; set; }
        public string Description { get; set; }

        public EnumProductionOrderStatus? Status { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<ProductionOrderDetail, ProductionOrderListModel>()
                .ForMember(dest => dest.ProductionOrderCode, opt => opt.MapFrom(source => source.ProductionOrder.ProductionOrderCode))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(source => source.ProductionOrder.Description))
                .ForMember(dest => dest.VoucherDate, opt => opt.MapFrom(source => source.ProductionOrder.VoucherDate.GetUnix()))
                .ForMember(dest => dest.FinishDate, opt => opt.MapFrom(source => source.ProductionOrder.FinishDate.GetUnix()))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(source => (EnumProductionOrderStatus)source.Status));
        }
    }
}
