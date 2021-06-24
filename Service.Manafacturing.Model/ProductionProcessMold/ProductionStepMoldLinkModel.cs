using AutoMapper;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ManufacturingDB;

namespace VErp.Services.Manafacturing.Model.ProductionProcessMold
{
    public class ProductionStepMoldLinkModel : IMapFrom<ProductionStepMoldLink>
    {
        public long FromProductionStepMoldId { get; set; }
        public long ToProductionStepMoldId { get; set; }

        public int StepFromId { get; set; }
        public int StepToId { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<ProductionStepMoldLinkModel, ProductionStepMoldLink>()
                .ForMember(m => m.FromProductionStepMold, v => v.Ignore())
                .ForMember(m => m.ToProductionStepMold, v => v.Ignore())
                .ForMember(m => m.FromProductionStepMoldId, v => v.MapFrom(m => m.FromProductionStepMoldId))
                .ForMember(m => m.ToProductionStepMoldId, v => v.MapFrom(m => m.ToProductionStepMoldId))
                .ReverseMap()
                .ForMember(m => m.FromProductionStepMoldId, v => v.MapFrom(m => m.FromProductionStepMoldId))
                .ForMember(m => m.ToProductionStepMoldId, v => v.MapFrom(m => m.ToProductionStepMoldId))
                .ForMember(m => m.StepFromId, v => v.MapFrom(m => m.FromProductionStepMold.StepId))
                .ForMember(m => m.StepToId, v => v.MapFrom(m => m.ToProductionStepMold.StepId));
        }
    }
}