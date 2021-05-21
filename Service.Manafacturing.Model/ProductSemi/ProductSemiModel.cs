using AutoMapper;
using System.Collections.Generic;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.GlobalObject;
using ProductSemiEntity = VErp.Infrastructure.EF.ManufacturingDB.ProductSemi;

namespace VErp.Services.Manafacturing.Model.ProductSemi
{
    public class ProductSemiModel: IMapFrom<ProductSemiEntity>
    {
        public long ProductSemiId { get; set; }
        public long ContainerId { get; set; }
        public EnumProductionProcess.EnumContainerType ContainerTypeId { get; set; }
        public string Title { get; set; }
        public string Specification { get; set; }
        public string Note { get; set; }
        public int UnitId { get; set; }
        public IList<ProductSemiConversionModel> ProductSemiConversions { get; set; }
        public int? DecimalPlace { get; set; }
        public void Mapping(Profile profile)
        {
            profile.CreateMap<ProductSemiEntity, ProductSemiModel>()
                .ForMember(m => m.ProductSemiConversions, v => v.MapFrom(m => m.ProductSemiConversion))
                .ReverseMap()
                .ForMember(m => m.ProductSemiConversion, v => v.Ignore());
        }
    }
}
