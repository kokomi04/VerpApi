using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.Manafacturing;

namespace VErp.Services.Manafacturing.Model.ProductionAssignment
{
    public class DepartmentProductionAssignmentModel
    {
        public long ProductionOrderId { get; set; }
        public string ProductionOrderCode { get; set; }
        public long? OrderDetailId { get; set; }
        public int ProductId { get; set; }
        public long StartDate { get; set; }
        public long EndDate { get; set; }
        public decimal ProductQuantity { get; set; }
        public EnumProductionStatus ProductionOrderStatus { get; set; }
    }    
}
