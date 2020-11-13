using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ManufacturingDB;
using VErp.Commons.Library;

namespace VErp.Services.Manafacturing.Model.ProductionOrder
{
    public class ProductionScheduleModel : ProductionPlaningOrderDetailModel, IMapFrom<ProductionScheduleEntity>
    {
        public int ProductionScheduleId { get; set; }
        public long StartDate { get; set; }
        public long EndDate { get; set; }
        public int ProductionScheduleQuantity { get; set; }
        public string ProductionOrderCode { get; set; }
        public void Mapping(Profile profile)
        {
            profile.CreateMap<ProductionScheduleEntity, ProductionScheduleModel>()
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(source => source.StartDate.GetUnix()))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(source => source.EndDate.GetUnix()))
                .ForMember(dest => dest.ProductionScheduleQuantity, opt => opt.MapFrom(source => (EnumProductionOrderStatus)source.ProductionScheduleQuantity));
        }
    }

    public class ProductionScheduleEntity : ProductionPlaningOrderDetailModel
    {
        public int ProductionScheduleId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int ProductionScheduleStatus { get; set; }
        public int ProductionScheduleQuantity { get; set; }
        public string ProductionOrderCode { get; set; }
    }

    public class ProductionScheduleInputModel : IMapFrom<ProductionSchedule>
    {
        public int ProductionOrderDetailId { get; set; }
        public long StartDate { get; set; }
        public long EndDate { get; set; }
        public int ProductionScheduleQuantity { get; set; }
    }

}
