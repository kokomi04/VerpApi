using AutoMapper;
using System.Collections.Generic;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ManufacturingDB;

namespace VErp.Services.Manafacturing.Model.ProductionProcessMold
{
    public class ProductionStepMoldModel: IMapFrom<ProductionStepMold>
    {
        public long ProductionStepMoldId { get; set; }
        public long ProductionProcessMoldId { get; set; }
        public int StepId { get; set; }
        public decimal? CoordinateX { get; set; }
        public decimal? CoordinateY { get; set; }

        public string StepName { get; set; }

        public virtual ICollection<ProductionStepMoldLinkModel> ProductionStepMoldLink { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<ProductionStepMoldModel, ProductionStepMold>()
                .ForMember(x => x.ProductionStepMoldId, v => v.MapFrom(m => m.ProductionStepMoldId))
                .ForMember(x => x.StepId, v => v.MapFrom(m => m.StepId))
                .ForMember(x => x.ProductionProcessMoldId, v => v.MapFrom(m => m.ProductionProcessMoldId))
                .ForMember(x => x.ProductionStepMoldLinkFromProductionStepMold, v => v.Ignore())
                .ForMember(x => x.Step, v => v.Ignore())
                .ForMember(x => x.CoordinateY, v => v.MapFrom(x=>x.CoordinateY))
                .ForMember(x => x.CoordinateX, v => v.MapFrom(x=>x.CoordinateX))
                .ReverseMap()
                .ForMember(x => x.ProductionStepMoldId, v => v.MapFrom(m => m.ProductionStepMoldId))
                .ForMember(x => x.StepId, v => v.MapFrom(m => m.StepId))
                .ForMember(x => x.StepName, v => v.MapFrom(m => m.Step.StepName))
                .ForMember(x => x.ProductionProcessMoldId, v => v.MapFrom(m => m.ProductionProcessMoldId))
                .ForMember(x => x.ProductionStepMoldLink, v => v.MapFrom(m => m.ProductionStepMoldLinkFromProductionStepMold))
                .ForMember(x => x.CoordinateY, v => v.MapFrom(x => x.CoordinateY))
                .ForMember(x => x.CoordinateX, v => v.MapFrom(x => x.CoordinateX));
        }
    }
}