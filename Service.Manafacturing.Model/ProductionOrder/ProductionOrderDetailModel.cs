using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ManufacturingDB;

namespace VErp.Services.Manafacturing.Model.ProductionOrder
{
    public class ProductionOrderDetailModel : IMapFrom<ProductionOrderDetail>
    {
        public int ProductionOrderDetailId { get; set; }
        public int ProductionOrderId { get; set; }
        public int? ProductId { get; set; }
        public int? Quantity { get; set; }
        public int? ReserveQuantity { get; set; }
        public string Note { get; set; }
        public long? PurchaseOrderId { get; set; }
        public string PurchaseOrderCode { get; set; }

        public int OrderQuantity { get; set; }
        public int OrderedQuantity { get; set; }
    }
}
