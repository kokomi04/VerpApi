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
        public string OrderCode { get; set; }
        public int ProductId { get; set; }       
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public EnumScheduleStatus ProductionScheduleStatus { get; set; }
    }
}
