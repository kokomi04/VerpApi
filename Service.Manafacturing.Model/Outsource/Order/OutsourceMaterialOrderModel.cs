using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Services.Manafacturing.Model.Outsource.Order
{   
    public class OutsourcePropertyOrderList
    {
        public long OutsourceOrderId { get; set; }
        public string OutsourceOrderCode { get; set; }
        public long OutsourceOrderFinishDate { get; set; }
        public long OutsourceOrderDate { get; set; }
        public string OutsourceStepRequestCode { get; set; }
        public long? PropertyCalcId { get; set; }
        public string PropertyCalcCode { get; set; }

        public long ObjectId { get; set; }
        public decimal Quantity { get; set; }

        public int? ProductId { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public int? unitId { get; set; }
        
        public int? CustomerId { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }
    }

    public class OutsourcePropertyOrderInput: OutsourceStepOrderInput
    {
        public long PropertyCalcId { get; set; }
    }
}
