using System.Collections.Generic;
using static VErp.Commons.Enums.Manafacturing.EnumProductionProcess;

namespace VErp.Services.Manafacturing.Model.ProductionOrder
{
    public class ProductionCapacityModel
    {
        public IList<StepInfo> StepInfo { get; set; }
        public IList<ProductionOrderCapacityModel> ProductionOrder { get; set; }
        public IDictionary<int, decimal> DepartmentHour { get; set; }
        public ProductionCapacityModel()
        {
            StepInfo = new List<StepInfo>();
            ProductionOrder = new List<ProductionOrderCapacityModel>();
            DepartmentHour = new Dictionary<int, decimal>();
        }
    }

    public class StepInfo
    {
        public int StepId { get; set; }
        public string StepName { get; set; }
    }

    public class ProductionOrderDetailQuantityModel
    {

        public long ProductionOrderDetailId { get; set; }
        public int? ProductId { get; set; }
        public decimal? Quantity { get; set; }
        public decimal? ReserveQuantity { get; set; }

    }

    public class ProductionOrderCapacityModel
    {
        public long ProductionOrderId { get; set; }
        public string ProductionOrderCode { get; set; }
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
        public long ObjectId { get; set; }
        public EnumProductionStepLinkDataObjectType ObjectTypeId { get; set; }
        public decimal Quantity { get; set; }
        public decimal TargetProductivity { get; set; }

        public decimal WorkloadQuantity { get; set; }
        public decimal WorkHour { get; set; }
        public IList<CapacityStepDetailModel> Details { get; set; }

    }

    public class CapacityStepDetailModel
    {
        public long ProductionStepId { get; set; }
        public string ProductionStepTitle { get; set; }
        public long ProductionStepLinkDataId { get; set; }        
        public decimal Quantity { get; set; }
        public decimal WorkloadQuantity { get; set; }
        public decimal WorkHour { get; set; }

        public decimal? AssignQuantity { get; set; }
        public decimal? AssignWorkloadQuantity { get; set; }

        public decimal? AssignWorkHour { get; set; }
    }

    public class ProductionWorkloadInfo
    {
        public long ProductionStepId { get; set; }
        public string ProductionStepTitle { get; set; }
        public long ProductionOrderId { get; set; }
        public int StepId { get; set; }

        public long ProductionStepLinkDataId { get; set; }
        public decimal Quantity { get; set; }
        public long ObjectId { get; set; }

        public EnumProductionStepLinkDataObjectType ObjectTypeId { get; set; }
        public decimal? WorkloadConvertRate { get; set; }
    }    

}
