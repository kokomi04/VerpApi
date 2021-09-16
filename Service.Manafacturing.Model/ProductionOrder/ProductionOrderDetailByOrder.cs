using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Services.Manafacturing.Model.ProductionOrder
{
    public class ProductionOrderDetailByOrder
    {
        public long ProductionOrderId { get; set; }
        public string ProductionOrderCode { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; }
        public long ProductionOrderDetailId { get; set; }
        public int ProductId { get; set; }
        public string OrderCode { get; set; }
        public decimal? Quantity { get; set; }
        public decimal? ReserveQuantity { get; set; }
        public string Note { get; set; }
    }
}
