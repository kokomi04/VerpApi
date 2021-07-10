using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Services.Manafacturing.Model.Outsource.Order
{
    public class OutsourceOrderMaterialsLSX
    {
        public long OutsourceOrderId { get; set; }
        public string OutsourceOrderCode { get; set; }
        public string Description { get; set; }
        public decimal Quantity { get; set; }
        public int UnitId { get; set; }
        public long ProductId { get; set; }
        public string OrderCode { get; set; }
        public string ProductionOrdeCode { get; set; }
        public int? CustomerId { get; set; } 
        public long OutsourceRequestId { get; set; }
        public string OutsourceRequestCode { get; set; }
    }
}
