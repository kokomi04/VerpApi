using System;
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
    }

    public partial class ProductionOutsourcePartMappingInput : ProductionOutsourcePartMappingModel
    {
        public string ProductionStepLinkDataCode { get; set; }
    }
}
