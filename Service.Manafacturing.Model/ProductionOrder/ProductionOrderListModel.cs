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

    public class ProductionOrderListModel : ProductionOrderDetailOutputModel, IMapFrom<ProductionOrderListEntity>
    {
        public string ProductionOrderCode { get; set; }
        public long StartDate { get; set; }
        public long EndDate { get; set; }
        public string Description { get; set; }
        public EnumProductionStatus ProductionOrderStatus { get; set; }
        public void Mapping(Profile profile)
        {
            profile.CreateMap<ProductionOrderListEntity, ProductionOrderListModel>()
                .ForMember(dest => dest.ProductionOrderCode, opt => opt.MapFrom(source => source.ProductionOrderCode))
                .ForMember(dest => dest.ProductionOrderStatus, opt => opt.MapFrom(source => (EnumProductionStatus)source.ProductionOrderStatus))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(source => source.Description))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(source => source.StartDate.GetUnix()))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(source => source.EndDate.GetUnix()));
        }
    }

    public class ProductionOrderListEntity : ProductionOrderDetailOutputModel
    {
        public string ProductionOrderCode { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Description { get; set; }
        public int ProductionOrderStatus { get; set; }
    }
}
