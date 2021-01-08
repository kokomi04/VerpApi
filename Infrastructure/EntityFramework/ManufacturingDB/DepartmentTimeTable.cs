using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.ManufacturingDB
{
    public partial class DepartmentTimeTable
    {
        public int DepartmentId { get; set; }
        public DateTime WorkDate { get; set; }
        public decimal? HourPerDay { get; set; }
    }
}
