﻿using System.Collections.Generic;
using static VErp.Commons.Enums.Manafacturing.EnumProductionProcess;

namespace VErp.Services.Manafacturing.Model.ProductionAssignment
{
    public class CapacityOutputModel
    {
        public IDictionary<int, List<CapacityModel>> CapacityData { get; set; }
    }

    public class CapacityModel
    {
        public long? StartDate { get; set; }
        public long? EndDate { get; set; }
        public long CreatedDatetimeUtc { get; set; }
        public string StepName { get; set; }
        public string ProductionOrderCode { get; set; }
        public decimal? Workload { get; set; }
        public decimal OutputQuantity { get; set; }
        public decimal AssingmentQuantity { get; set; }
        //public decimal LinkDataQuantity { get; set; }
        public long ObjectId { get; set; }
        public EnumProductionStepLinkDataObjectType ObjectTypeId { get; set; }
        //public decimal CompletedQuantity { get; set; }

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
        public decimal Productivity { get; set; }
        public string ProductionOrderCode { get; set; }
    }

    //public class ProductivityModel
    //{
    //    public decimal ProductivityPerPerson { get; set; }
    //    public int UnitId { get; set; }
    //}

    public class DepartmentAssignFreeDate
    {
        public int DepartmentId { get; set; }
        public long FreeDate { get; set; }
    }

    public class DepartmentAssignFreeDateInput
    {
        public IList<int> DepartmentIds { get; set; }
        public IList<long> ExceptProductionOrderIds { get; set; }
    }

}
