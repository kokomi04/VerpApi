using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ManufacturingDB;
using VErp.Commons.Library;

namespace VErp.Services.Manafacturing.Model.ProductionAssignment
{
    public class ProductionStepWorkInfoOutputModel : IMapFrom<ProductionStepWorkInfo>
    {
        public long ProductionStepId { get; set; }
        public long ProductionOrderId { get; set; }
        public EnumHandoverTypeStatus HandoverType { get; set; }
        public decimal? MinHour { get; set; }
        public decimal? MaxHour { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<ProductionStepWorkInfo, ProductionStepWorkInfoOutputModel>()
                .ForMember(s => s.HandoverType, d => d.MapFrom(m => (EnumHandoverTypeStatus)m.HandoverType));
        }
    }

    public class ProductionStepWorkInfoInputModel : IMapFrom<ProductionStepWorkInfo>
    {
        public EnumHandoverTypeStatus HandoverType { get; set; }
        public decimal? MinHour { get; set; }
        public decimal? MaxHour { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<ProductionStepWorkInfoInputModel, ProductionStepWorkInfo>()
                .ForMember(s => s.HandoverType, d => d.MapFrom(m => (int)m.HandoverType));
        }
    }
}
