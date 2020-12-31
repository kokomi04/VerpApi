using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Services.Manafacturing.Model.ProductionAssignment
{
    public class CapacityDepartmentChartsModel
    {
        public int DepartmentId { get; set; }
        public long ScheduleTurnId { get; set; }
        public decimal Capacity { get; set; }
    }

    public class CapacityDepartmentDetailModel
    {
        public long StartDate { get; set; }
        public long EndDate { get; set; }
        public decimal Capacity { get; set; }
        public long UpdatedDatetimeUtc { get; set; }
    }
}
