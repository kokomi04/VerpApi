using System;
using System.Collections.Generic;
using VErp.Services.Manafacturing.Model.ProductionAssignment;
using static VErp.Commons.Enums.Manafacturing.EnumProductionProcess;

namespace VErp.Services.Manafacturing.Model.ProductionOrder
{
    public class ProductionCapacityModel
    {
        public IList<StepInfo> StepInfo { get; set; }
        public IList<ProductionOrderCapacityModel> ProductionOrder { get; set; }
        public IDictionary<int, decimal> StepHourTotal { get; set; }
        public IDictionary<int, decimal> AssignedStepHours { get; set; }
        public IDictionary<int, IList<StepDepartmentHour>> StepHoursDetail { get; set; }
        public IDictionary<int, decimal> DepartmentHourTotal { get; set; }
        public ProductionCapacityModel()
        {
            StepInfo = new List<StepInfo>();
            ProductionOrder = new List<ProductionOrderCapacityModel>();
            StepHourTotal = new Dictionary<int, decimal>();
            AssignedStepHours = new Dictionary<int, decimal>();
            StepHoursDetail = new Dictionary<int, IList<StepDepartmentHour>>();
            DepartmentHourTotal = new Dictionary<int, decimal>();
        }
    }

    public class StepDepartmentHour
    {
        public int DepartmentId { get; set; }
        public decimal AssignedHours { get; set; }
        public decimal HourTotal { get; set; }
    }

    public class StepInfo
    {
        public int StepId { get; set; }
        public string StepName { get; set; }
    }

    public class ProductionOrderDetailQuantityModel
    {
        public string OrderCode { get; set; }
        public long ProductionOrderDetailId { get; set; }
        public int? ProductId { get; set; }
        public decimal? Quantity { get; set; }
        public decimal? ReserveQuantity { get; set; }

    }

    public class ProductionOrderCapacityModel
    {
        public long ProductionOrderId { get; set; }
        public string ProductionOrderCode { get; set; }
        public long StartDate { get; set; }
        public long EndDate { get; set; }

        public IList<ProductionOrderDetailQuantityModel> ProductionOrderDetail { get; set; }
        public IDictionary<int, IList<ProductionCapacityDetailModel>> ProductionCapacityDetail { get; set; }
        public ProductionOrderCapacityModel()
        {
            ProductionOrderDetail = new List<ProductionOrderDetailQuantityModel>();
            ProductionCapacityDetail = new Dictionary<int, IList<ProductionCapacityDetailModel>>();
        }
    }

    public class ProductionCapacityDetailModel
    {
        public long ProductionOrderId { get; set; }
        public int StepId { get; set; }
        public long ObjectId { get; set; }
        public EnumProductionStepLinkDataObjectType ObjectTypeId { get; set; }
        public decimal Quantity { get; set; }
        public decimal TargetProductivity { get; set; }
        public decimal MinAssignHours { get; set; }

        public decimal WorkloadQuantity { get; set; }
        public decimal WorkHour { get; set; }
        public IList<ProductionStepWorkloadAssignModel> Details { get; set; }

    }

    public class ProductionStepWorkloadModel
    {
        public int StepId { get; set; }
        public long ProductionStepId { get; set; }
        public string ProductionStepTitle { get; set; }
        public long ProductionStepLinkDataId { get; set; }
        public decimal Quantity { get; set; }
        public decimal WorkloadConvertRate { get; set; }
        public decimal WorkloadQuantity { get; set; }
        public decimal WorkHour { get; set; }
        public decimal? Productivity { get; set; }
        public decimal? MinAssignHours { get; set; }
        public decimal OutsourceQuantity { get; set; }
    }


    public class ProductionStepWorkloadAssignModel : ProductionStepWorkloadModel
    {
        //public bool IsSelectionAssign { get; set; }
        //public decimal? AssignQuantity { get; set; }
        //public decimal? AssignWorkloadQuantity { get; set; }

        //public decimal? AssignWorkHour { get; set; }

        //public long? StartDate { get; set; }
        //public long? EndDate { get; set; }
        //public bool IsManualSetDate { get; set; }
        //public decimal RateInPercent { get; set; }

        //public IList<ProductionAssignmentDetailModel> ByDates { get; set; }

        public IList<CapacityAssignInfo> AssignInfos { get; set; }

    }


    public class ProductionWorkloadInfo
    {
        public long ProductionStepId { get; set; }
        public string ProductionStepTitle { get; set; }
        public long ProductionOrderId { get; set; }
        public int StepId { get; set; }

        public long ProductionStepLinkDataId { get; set; }
        public decimal OutsourceQuantity { get; set; }
        public decimal Quantity { get; set; }
        public long ObjectId { get; set; }

        public EnumProductionStepLinkDataObjectType ObjectTypeId { get; set; }
        public decimal? WorkloadConvertRate { get; set; }
    }


    /// <summary>
    /// Key is stepId
    /// </summary>
    public class CapacityByStep : Dictionary<int, IList<ProductionCapacityDetailModel>>
    {

    }

    /// <summary>
    /// Key is ProductionOrderId
    /// </summary>
    public class CapacityStepByProduction : Dictionary<long, CapacityByStep>
    {

    }

    public class ProductionOrderStepWorkloadModel
    {
        public long ProductionOrderId { get; set; }
        public IList<ProductionStepOutputObjectWorkloadModel> StepWorkLoads { get; set; }
    }

    public class ProductionStepOutputObjectWorkloadModel
    {
        public int StepId { get; set; }
        public IList<ProductionCapacityDetailModel> Outputs { get; set; }
    }
}
