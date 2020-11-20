using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Services.Manafacturing.Model.Outsource.Order
{
    public class OutsourceOrderPartDetailOutput : OutsourceOrderModel
    {
        public long OutsourceOrderDetailId { get; set; }
        public long ObjectId { get; set; }
        public decimal Price { get; set; }
        public decimal Tax { get; set; }
        public string ProductTitle { get; set; }
        public string ProductPartName { get; set; }
        public string ProductionOrderCode { get; set; }
        public string OrderCode { get; set; }
        public string RequestOutsourcePartCode { get; set; }
        public string UnitName { get; set; }
        public decimal Quantity { get; set; }
    }
}
