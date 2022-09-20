using AutoMapper;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ManufacturingDB;

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
            profile.CreateMapCustom<ProductionStepWorkInfo, ProductionStepWorkInfoOutputModel>()
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
            profile.CreateMapCustom<ProductionStepWorkInfoInputModel, ProductionStepWorkInfo>()
                .ForMember(s => s.HandoverType, d => d.MapFrom(m => (int)m.HandoverType));
        }
    }
}
