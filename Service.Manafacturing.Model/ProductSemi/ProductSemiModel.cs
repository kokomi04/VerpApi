using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using ProductSemiEntity = VErp.Infrastructure.EF.ManufacturingDB.ProductSemi;

namespace VErp.Services.Manafacturing.Model.ProductSemi
{
    public class ProductSemiModel: IMapFrom<ProductSemiEntity>
    {
        public long ProductSemiId { get; set; }
        public long ContainerId { get; set; }
        public EnumProductionProcess.EnumContainerType ContainerTypeId { get; set; }
        public string Title { get; set; }
        public int UnitId { get; set; }
        public ProductSemiConversion Conversion { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<ProductSemiEntity, ProductSemiModel>()
                .ForMember(m => m.Conversion, v => v.MapFrom(m => m.Conversion.JsonDeserialize<ProductSemiConversion>()))
                .ReverseMap()
                .ForMember(m => m.Conversion, v => v.MapFrom(m => m.Conversion.JsonSerialize()));
        }
    }

    public class ProductSemiConversion
    {
        public long ConversionId { get; set; }
        public EnumProductionProcess.EnumContainerType ConversionTypeId { get; set; }
        public decimal ConversionRate { get; set; }
    }
}
