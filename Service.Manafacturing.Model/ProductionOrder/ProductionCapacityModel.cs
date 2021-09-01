using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ManufacturingDB;
using VErp.Commons.Library;
using VErp.Services.Manafacturing.Model.ProductionHandover;

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
        public decimal Productivity { get; set; }
    }

    public class ProductionOrderDetailCapacityModel
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
        public IList<ProductionOrderDetailCapacityModel> ProductionOrderDetail { get; set; }
        public IDictionary<int, List<ProductionCapacityDetailModel>> ProductionCapacityDetail { get; set; }
        public ProductionOrderCapacityModel()
        {
            ProductionOrderDetail = new List<ProductionOrderDetailCapacityModel>();
            ProductionCapacityDetail = new Dictionary<int, List<ProductionCapacityDetailModel>>();
        }
    }

    public class ProductionCapacityDetailModel
    {
        public long ObjectId { get; set; }
        public int ObjectTypeId { get; set; }
        public decimal Quantity { get; set; }

        public decimal WorkloadQuantity { get; set; }
        public decimal WorkHour { get; set; }

    }

}
