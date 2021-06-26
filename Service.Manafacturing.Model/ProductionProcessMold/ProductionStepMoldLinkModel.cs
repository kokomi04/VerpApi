using AutoMapper;
using System;
using System.Collections.Generic;
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

    public class ProductionStepMoldLinkComparer : IEqualityComparer<ProductionStepMoldLink>
    {
        // Products are equal if their names and product numbers are equal.
        public bool Equals(ProductionStepMoldLink x, ProductionStepMoldLink y)
        {

            //Check whether the compared objects reference the same data.
            if (Object.ReferenceEquals(x, y)) return true;

            //Check whether any of the compared objects is null.
            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            //Check whether the products' properties are equal.
            return x.FromProductionStepMoldId == y.FromProductionStepMoldId && x.ToProductionStepMoldId == y.ToProductionStepMoldId;
        }

        // If Equals() returns true for a pair of objects
        // then GetHashCode() must return the same value for these objects.

        public int GetHashCode(ProductionStepMoldLink link)
        {
            //Check whether the object is null
            if (Object.ReferenceEquals(link, null)) return 0;

            //Get hash code for the Name field if it is not null.
            //int hashProductName = link.Name == null ? 0 : link.Name.GetHashCode();

            //Get hash code for the Code field.
            //int hashProductCode = link.Code.GetHashCode();

            //Calculate the hash code for the product.
            return (int)(link.FromProductionStepMoldId ^ link.ToProductionStepMoldId);
        }
    }
}