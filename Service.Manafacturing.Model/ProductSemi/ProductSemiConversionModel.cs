using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ManufacturingDB;

namespace VErp.Services.Manafacturing.Model.ProductSemi
{
    public class ProductSemiConversionModel : IMapFrom<ProductSemiConversion>
    {
        public long ProductSemiConversionId { get; set; }
        public long ProductSemiId { get; set; }
        public int ConversionGroup { get; set; }
        public EnumProductionProcess.EnumProductionStepLinkDataObjectType ConversionTypeId { get; set; }
        public long ConversionId { get; set; }
        public decimal ConversionRate { get; set; }
    }
}
