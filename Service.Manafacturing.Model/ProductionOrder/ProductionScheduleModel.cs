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
    public class ProductionScheduleModel : ProductionPlanningOrderDetailModel, IMapFrom<ProductionScheduleEntity>
    {
        public int ProductionScheduleId { get; set; }
        public long StartDate { get; set; }
        public long EndDate { get; set; }
        public int ProductionScheduleQuantity { get; set; }
        public string ProductionOrderCode { get; set; }
        public long? ScheduleTurnId { get; set; }
        public void Mapping(Profile profile)
        {
            profile.CreateMap<ProductionScheduleEntity, ProductionScheduleModel>()
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(source => source.StartDate.GetUnix()))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(source => source.EndDate.GetUnix()))
                .ForMember(dest => dest.ProductionScheduleQuantity, opt => opt.MapFrom(source => (EnumProductionStatus)source.ProductionScheduleQuantity));
        }
    }

    public class ProductionScheduleEntity : ProductionPlanningOrderDetailModel
    {
        public int ProductionScheduleId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int ProductionScheduleStatus { get; set; }
        public int ProductionScheduleQuantity { get; set; }
        public string ProductionOrderCode { get; set; }
        public long? ScheduleTurnId { get; set; }
    }

    public class ProductionScheduleInputModel : IMapFrom<ProductionSchedule>
    {
        public int? ProductionScheduleId { get; set; }
        public int ProductionOrderDetailId { get; set; }
        public long StartDate { get; set; }
        public long EndDate { get; set; }
        public int ProductionScheduleQuantity { get; set; }
        public long? ScheduleTurnId { get; set; }
        public void Mapping(Profile profile)
        {
            profile.CreateMap<ProductionScheduleInputModel, ProductionSchedule>()
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(source => source.StartDate.UnixToDateTime()))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(source => source.EndDate.UnixToDateTime()));
        }
    }

}
