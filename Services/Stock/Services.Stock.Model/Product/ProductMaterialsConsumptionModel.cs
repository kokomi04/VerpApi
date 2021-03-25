using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.StockDB;

namespace VErp.Services.Stock.Model.Product
{
    public class ProductMaterialsConsumptionModel: IMapFrom<ProductMaterialsConsumption>
    {
        public long ProductMaterialsConsumptionId { get; set; }
        public int ProductId { get; set; }
        public string GroupCode { get; set; }
        public string GroupTitle { get; set; }
        public int MaterialConsumptionId { get; set; }
        public decimal Quantity { get; set; }
        public int? StepId { get; set; }
        public int? DepartmentId { get; set; }

    }
}
