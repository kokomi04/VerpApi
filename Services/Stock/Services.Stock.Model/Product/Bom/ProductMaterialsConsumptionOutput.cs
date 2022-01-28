using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VErp.Infrastructure.EF.StockDB;

namespace VErp.Services.Stock.Model.Product
{
    public class ProductMaterialsConsumptionOutput: ProductMaterialsConsumptionBaseModel
    {
        public decimal TotalQuantityInheritance { get; set; }
        public decimal BomQuantity { get; set; }
        public int UnitId { get; set; }

        public IList<ProductMaterialsConsumptionOutput> MaterialsConsumptionInheri { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<ProductMaterialsConsumption, ProductMaterialsConsumptionOutput>()
                .ForMember(m => m.UnitId, v => v.MapFrom(m => m.MaterialsConsumption != null ? m.MaterialsConsumption.UnitId : 0))
                .ReverseMap()
                .ForMember(m => m.MaterialsConsumption, v => v.Ignore());
        }
    }

    public class ProductMaterialsConsumptionBaseComparer : IEqualityComparer<ProductMaterialsConsumptionOutput>
    {
        public bool Equals(ProductMaterialsConsumptionOutput x, ProductMaterialsConsumptionOutput y)
        {

            if (Object.ReferenceEquals(x, y)) return true;

            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            return x.ProductMaterialsConsumptionGroupId == y.ProductMaterialsConsumptionGroupId && x.MaterialsConsumptionId == y.MaterialsConsumptionId;
        }

        // If Equals() returns true for a pair of objects
        // then GetHashCode() must return the same value for these objects.

        public int GetHashCode(ProductMaterialsConsumptionOutput product)
        {
            if (Object.ReferenceEquals(product, null)) return 0;

            return product.ProductMaterialsConsumptionGroupId ^ product.MaterialsConsumptionId;
        }
    }
}
