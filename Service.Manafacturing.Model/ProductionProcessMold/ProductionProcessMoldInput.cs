using AutoMapper;
using System.Collections;
using System.Collections.Generic;
using VErp.Commons.GlobalObject;
using ProductionProcessMoldEntity = VErp.Infrastructure.EF.ManufacturingDB.ProductionProcessMold;

namespace VErp.Services.Manafacturing.Model.ProductionProcessMold
{
    public class ProductionProcessMoldInput : ProductionProcessMoldSimple, IMapFrom<ProductionProcessMoldEntity>
    {
        public ProductionProcessMoldInput()
        {
            this.ProductionStepMold = new List<ProductionStepMoldModel>();
        }

        public ICollection<ProductionStepMoldModel> ProductionStepMold { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<ProductionProcessMoldInput, ProductionProcessMoldEntity>()
                .ForMember(m => m.ProductionStepMold, v => v.Ignore())
                .ForMember(m => m.ProductionProcessMoldId, v => v.MapFrom(m => m.ProductionProcessMoldId))
                .ForMember(m => m.Title, v => v.MapFrom(m => m.Title));
        }
    }
}