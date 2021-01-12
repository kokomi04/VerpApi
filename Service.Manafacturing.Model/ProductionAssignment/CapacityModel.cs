using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Services.Manafacturing.Model.ProductionAssignment
{
    public class CapacityOutputModel
    {
        public IDictionary<int, List<CapacityModel>> CapacityData { get; set; }
        public IList<ZeroWorkloadModel> ZeroWorkload { get; set; }
    }

    public class ZeroWorkloadModel
    {
        public string StepName { get; set; }
        public int UnitId { get; set; }
        public string ProductionOrderCode { get; set; }
        public long ProductionStepId { get; set; }
        public long ProductionOrderId { get; set; }
    }

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

    public class ProductivityModel
    {
        public decimal Quantity { get; set; }
        public int UnitId { get; set; }
    }
}
