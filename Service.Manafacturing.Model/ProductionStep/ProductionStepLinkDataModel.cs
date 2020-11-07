using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ManufacturingDB;

namespace VErp.Services.Manafacturing.Model.ProductionStep
{
    public class ProductionStepLinkDataModel: IMapFrom<ProductionStepLinkData>
    {
        public long ProductionStepLinkDataId { get; set; }
        public int ProductId { get; set; }
        public int ProductUnitConversionId { get; set; }
        public decimal Quantity { get; set; }
        public int SortOrder { get; set; }
    }

    public class ProductionStepLinkDataInfo : ProductionStepLinkDataModel, IMapFrom<ProductionStepLinkDataRole>
    {

        public EnumProductionProcess.ProductionStepLinkDataRoleType ProductionStepLinkDataRoleTypeId { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<ProductionStepLinkDataRole, ProductionStepLinkDataInfo >()
                .ForMember(m => m.ProductId, v => v.MapFrom(m => m.ProductionStepLinkData.ProductId))
                .ForMember(m => m.ProductionStepLinkDataId, v => v.MapFrom(m => m.ProductionStepLinkData.ProductionStepLinkDataId))
                .ForMember(m => m.Quantity, v => v.MapFrom(m => m.ProductionStepLinkData.Quantity))
                .ForMember(m => m.ProductUnitConversionId, v => v.MapFrom(m => m.ProductionStepLinkData.ProductUnitConversionId))
                .ForMember(m => m.SortOrder, v => v.MapFrom(m => m.ProductionStepLinkData.SortOrder))
                .ReverseMap()
                .ForMember(m => m.ProductionStepLinkData, v => v.Ignore());
        }
    }
}
