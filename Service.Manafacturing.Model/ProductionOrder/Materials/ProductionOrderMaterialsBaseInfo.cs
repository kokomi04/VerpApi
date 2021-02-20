using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ManufacturingDB;

namespace VErp.Services.Manafacturing.Model.ProductionOrder.Materials
{
    public class ProductionOrderMaterialsBaseInfo : IMapFrom<ProductionOrderMaterials>
    {
        public long ProductionOrderMaterialsId { get; set; }
        public long ProductionOrderId { get; set; }
        public long ProductId { get; set; }
        public decimal Quantity { get; set; }
        public int UnitId { get; set; }
        public int? DepartmentId { get; set; }
        public long? ParentId { get; set; }
        public bool IsReplacement { get; set; }
        public long ProductionStepLinkDataId { get; set; }
    }

    public class ProductionOrderMaterialsInput : ProductionOrderMaterialsBaseInfo
    {
        public ProductionOrderMaterialsInput()
        {
            materialsReplacement = new List<ProductionOrderMaterialsInput>();
        }

        public IList<ProductionOrderMaterialsInput> materialsReplacement { get; set; }

    }

    public class ProductionOrderMaterialsOutput : ProductionOrderMaterialsBaseInfo
    {
    }
}
