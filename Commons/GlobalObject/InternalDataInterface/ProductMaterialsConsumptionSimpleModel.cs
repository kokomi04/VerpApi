using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Commons.GlobalObject.InternalDataInterface
{
    public class ProductMaterialsConsumptionSimpleModel
    {
        public long ProductMaterialsConsumptionId { get; set; }
        public int ProductMaterialsConsumptionGroupId { get; set; }
        public int ProductId { get; set; }
        public int MaterialsConsumptionId { get; set; }
        public decimal Quantity { get; set; }
        public int? StepId { get; set; }
        public int? DepartmentId { get; set; }

        public decimal TotalQuantityInheritance { get; set; }

        public IList<ProductMaterialsConsumptionSimpleModel> MaterialsConsumptionInheri { get; set; }
    }
}
