using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ManufacturingDB;
using ProductionOrderEntity = VErp.Infrastructure.EF.ManufacturingDB.ProductionOrder;
using VErp.Commons.Library;

namespace VErp.Services.Manafacturing.Model.ProductionOrder
{
    public class ProductionOrderModel : ProductionOrderListModel
    {
        public ProductionOrderModel()
        {
            ProductionOrderDetail = new HashSet<ProductionOrderDetailModel>();
        }

        public virtual ICollection<ProductionOrderDetailModel> ProductionOrderDetail { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<ProductionOrderModel, ProductionOrderEntity>()
                .ForMember(dest => dest.ProductionOrderDetail, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.VoucherDate, opt => opt.MapFrom(source => source.VoucherDate.UnixToDateTime()))
                .ForMember(dest => dest.FinishDate, opt => opt.MapFrom(source => source.FinishDate.HasValue? source.FinishDate.Value.UnixToDateTime() : null))
                .ReverseMap()
                .ForMember(dest => dest.ProductionOrderDetail, opt => opt.MapFrom(source => source.ProductionOrderDetail))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(source => (EnumProductionOrderStatus)source.Status))
                .ForMember(dest => dest.VoucherDate, opt => opt.MapFrom(source => source.VoucherDate.GetUnix()))
                .ForMember(dest => dest.FinishDate, opt => opt.MapFrom(source => source.FinishDate.GetUnix()));
        }
    }

    public class ProductionOrderListModel : IMapFrom<ProductionOrderEntity>
    {
        public int ProductionOrderId { get; set; }
        public string ProductionOrderCode { get; set; }
        public long VoucherDate { get; set; }
        public long? FinishDate { get; set; }
        public string Description { get; set; }
        public EnumProductionOrderStatus? Status { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<ProductionOrderEntity, ProductionOrderListModel>()
                .ForMember(dest => dest.VoucherDate, opt => opt.MapFrom(source => source.VoucherDate.GetUnix()))
                .ForMember(dest => dest.FinishDate, opt => opt.MapFrom(source => source.FinishDate.GetUnix()))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(source => (EnumProductionOrderStatus)source.Status));
        }
    }
}
