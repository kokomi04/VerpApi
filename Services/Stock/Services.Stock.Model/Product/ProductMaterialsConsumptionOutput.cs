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
        //public decimal TotalQuantityInheritance { get; set; }
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
}
