using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.Manafacturing;

namespace VErp.Services.Manafacturing.Model.ProductionAssignment
{
    public class DepartmentProductionAssignmentModel
    {
        public long ScheduleTurnId { get; set; }
        public long ProductionOrderId { get; set; }
        public string ProductionOrderCode { get; set; }
        public string OrderCode { get; set; }
        public int ProductId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public EnumScheduleStatus ProductionScheduleStatus { get; set; }
    }

    public class DepartmentProductionAssignmentDetailModel : DepartmentProductionAssignmentModel
    {
        public long ProductionStepId { get; set; }
        public decimal ProductionScheduleQuantity { get; set; }
        public decimal AssignmentQuantity { get; set; }
        public int ObjectTypeId { get; set; }
        public long ObjectId { get; set; }
    }
}
