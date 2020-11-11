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

    public class ProductionScheduleModel : IMapFrom<ProductionSchedule>
    {
        public int ProductionOrderDetailId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public EnumProductionOrderStatus? Status { get; set; }
        public void Mapping(Profile profile)
        {
            profile.CreateMap<ProductionScheduleModel, ProductionSchedule>()
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ReverseMap()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(source => (EnumProductionOrderStatus)source.Status));
        }
    }
}
