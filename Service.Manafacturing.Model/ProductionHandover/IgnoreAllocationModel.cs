﻿using System.Collections.Generic;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ManufacturingDB;
using VErp.Services.Manafacturing.Model.ProductionOrder.Materials;

namespace VErp.Services.Manafacturing.Model.ProductionHandover
{
    public class IgnoreAllocationModel : IMapFrom<IgnoreAllocation>
    {
        public long ProductionOrderId { get; set; }
        public string InventoryCode { get; set; }
        public int ProductId { get; set; }
        public long? InventoryDetailId { get; set; }
    }

    public class ConflictHandoverModel
    {
        public IList<ProductionInventoryRequirementModel> ConflictExportStockInventories { get; set; }
        public IList<ProductionInventoryRequirementModel> ConflictOtherInventories { get; set; }
        public IList<ProductionMaterialsRequirementDetailListModel> ConflictMaterialRequirements { get; set; }
        public IList<ProductionHandoverModel> ConflictHandovers { get; set; }

        public IList<ProductionStepSimpleModel> InputProductionSteps { get; set; }
        public IList<InputStepObjectDepartmentModel> InputStepObjectMap { get; set; }
        public IDictionary<long, List<InOutMaterialModel>> InputDataMap { get; set; }
        public IDictionary<long, List<int>> Assignments { get; set; }
    }

    public class InOutProductionStepModel
    {
        public List<InOutMaterialModel> InputData { get; set; }
        public List<InOutMaterialModel> OutputData { get; set; }
    }

    public class ProductionStepSimpleModel
    {
        public long ProductionStepId { get; set; }
        public string Title { get; set; }
    }

    public class InOutMaterialModel
    {
        public long ObjectId { get; set; }
        public int ObjectTypeId { get; set; }
    }

    public class InputStepObjectDepartmentModel
    {
        public long ProductionStepId { get; set; }
        public List<long> ProductIds { get; set; }
        public List<int> DepartmentIds { get; set; }
    }
}
