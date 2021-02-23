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
    public class ProductionOrderOutputModel : ProductOrderModel, IMapFrom<ProductionOrderEntity>
    {
        public EnumProductionStatus? ProductionOrderStatus { get; set; }
        //public EnumProcessStatus ProcessStatus { get; set; }
        public virtual ICollection<ProductionOrderDetailOutputModel> ProductionOrderDetail { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<ProductionOrderEntity, ProductionOrderOutputModel>()
                .ForMember(dest => dest.ProductionOrderDetail, opt => opt.Ignore())
                .ForMember(dest => dest.ProductionOrderStatus, opt => opt.MapFrom(source => (EnumProductionStatus)source.ProductionOrderStatus))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(source => source.StartDate.GetUnix()))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(source => source.EndDate.GetUnix()))
                .ForMember(dest => dest.Date, opt => opt.MapFrom(source => source.Date.GetUnix()));
        }
    }

    public class ProductionOrderInputModel : ProductOrderModel, IMapFrom<ProductionOrderEntity>
    {
        public virtual ICollection<ProductionOrderDetailInputModel> ProductionOrderDetail { get; set; }
        public void Mapping(Profile profile)
        {
            profile.CreateMap<ProductionOrderInputModel, ProductionOrderEntity>()
                .ForMember(dest => dest.ProductionOrderDetail, opt => opt.Ignore())
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(source => source.StartDate.UnixToDateTime()))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(source => source.EndDate.UnixToDateTime()))
                .ForMember(dest => dest.Date, opt => opt.MapFrom(source => source.Date.UnixToDateTime()));
        }
    }

    public class ProductOrderModel
    {
        public long ProductionOrderId { get; set; }
        public string ProductionOrderCode { get; set; }
        public long StartDate { get; set; }
        public long Date { get; set; }
        public long EndDate { get; set; }
        public string Description { get; set; }
        public bool IsDraft { get; set; }
    }

    public class ProductionOrderStatusModel
    {
        public EnumProductionStatus ProductionOrderStatus { get; set; }
    }
}
