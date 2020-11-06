using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ManufacturingDB;

namespace VErp.Services.Manafacturing.Model.ProductionStep
{
    public class ProductInStepModel: IMapFrom<ProductInStep>
    {
        public int ProductInStepId { get; set; }
        public int ProductId { get; set; }
        public decimal Quantity { get; set; }
        public int UnitId { get; set; }
        public int SortOrder { get; set; }
    }

    public class ProductInStepInfo: ProductInStepModel, IMapFrom<InOutStepLink>
    {

        public EnumInOutStepType InOutStepType { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<InOutStepLink, ProductInStepInfo>()
                .ForMember(m => m.ProductId, v => v.MapFrom(m => m.ProductInStep.ProductId))
                .ForMember(m => m.ProductInStepId, v => v.MapFrom(m => m.ProductInStep.ProductInStepId))
                .ForMember(m => m.Quantity, v => v.MapFrom(m => m.ProductInStep.Quantity))
                .ForMember(m => m.UnitId, v => v.MapFrom(m => m.ProductInStep.UnitId))
                .ForMember(m => m.SortOrder, v => v.MapFrom(m => m.ProductInStep.SortOrder))
                .ReverseMap()
                .ForMember(m => m.ProductInStep, v => v.Ignore());
        }
    }
}
