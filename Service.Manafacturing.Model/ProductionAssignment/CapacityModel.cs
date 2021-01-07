using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Services.Manafacturing.Model.ProductionAssignment
{
    public class CapacityModel
    {
        public long StartDate { get; set; }
        public long EndDate { get; set; }
        public decimal Capacity { get; set; }
        public long CreatedDatetimeUtc { get; set; }

        public virtual ICollection<CapacityDetailModel> CapacityDetail { get; set; }

        public CapacityModel()
        {
            CapacityDetail = new List<CapacityDetailModel>();
        }
    }

    public class CapacityDetailModel
    {
        public long WorkDate { get; set; }
        public decimal? CapacityPerDay { get; set; }
        public string StepName { get; set; }
        public string ProductionOrderCode { get; set; }
    }
}
