using AutoMapper;
using System.Collections.Generic;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.ManufacturingDB;

namespace VErp.Services.Manafacturing.Model.ProductionStep
{
    public class ProductionStepCollectionModel : ProductionStepCollectionBase, IMapFrom<ProductionStepCollection>
    {
        public IList<StepCollection> Collections { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMapCustom<ProductionStepCollection, ProductionStepCollectionModel>()
                .ForMember(m => m.Collections, v => v.MapFrom(x => x.Collections.JsonDeserialize<IList<StepCollection>>()))
                .ReverseMapCustom()
                .ForMember(m => m.Collections, v => v.MapFrom(x => x.Collections.JsonSerialize()));
        }
    }

    public class ProductionStepCollectionSearch : ProductionStepCollectionBase, IMapFrom<ProductionStepCollection>
    {
        public IList<StepCollectionSearch> Collections { get; set; }
        public int Frequence { get; set; }
        public string Description { get; set; }
        public void Mapping(Profile profile)
        {
            profile.CreateMapCustom<ProductionStepCollection, ProductionStepCollectionSearch>()
                .ForMember(m => m.Collections, v => v.MapFrom(x => x.Collections.JsonDeserialize<IList<StepCollectionSearch>>()))
                .ReverseMapCustom()
                .ForMember(m => m.Collections, v => v.MapFrom(x => x.Collections.JsonSerialize()));
        }

    }

    public class ProductionStepCollectionBase
    {
        public long ProductionStepCollectionId { get; set; }
        public string Title { get; set; }
    }
    public class StepCollectionSearch : StepCollection
    {
        public string StepName { get; set; }
    }

    public class StepCollection
    {
        public int StepId { get; set; }
        public int Order { get; set; }
    }
}
