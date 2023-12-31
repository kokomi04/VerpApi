using System.Collections.Generic;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ManufacturingDB;

namespace VErp.Services.Manafacturing.Model.ProductionOrder
{
    public partial class ProductionOutsourcePartMappingModel : IMapFrom<ProductionOutsourcePartMapping>
    {
        public long ProductionOutsourcePartMappingId { get; set; }
        public long ContainerId { get; set; }
        public long OutsourcePartRequestDetailId { get; set; }
        public int ProductId { get; set; }
        public decimal Quantity { get; set; }
        public bool IsDefault { get; set; }
    }

    public partial class ProductionOutsourcePartMappingInput : ProductionOutsourcePartMappingModel
    {
        public List<string> ProductionStepLinkDataCodes { get; set; }

        public ProductionOutsourcePartMappingInput()
        {
            ProductionStepLinkDataCodes = new List<string>();
        }
    }
}
