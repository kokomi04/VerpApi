using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.StockDB;

namespace VErp.Services.Stock.Model.Product
{
    public class ProductMaterialsConsumptionInput: ProductMaterialsConsumptionBaseModel
    {
    }

    public class ProductMaterialsConsumptionBaseModel: IMapFrom<ProductMaterialsConsumption>
    {
        public long ProductMaterialsConsumptionId { get; set; }
        public int ProductMaterialsConsumptionGroupId { get; set; }
        public int ProductId { get; set; }
        public int MaterialsConsumptionId { get; set; }
        public decimal Quantity { get; set; }
        public int? StepId { get; set; }
        public int? DepartmentId { get; set; }
        public string Description { get; set; }
    }
}
