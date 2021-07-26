using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.StockDB;

namespace VErp.Services.Stock.Model.Product
{
    public class ProductMaterialsConsumptionGroupModel: IMapFrom<ProductMaterialsConsumptionGroup>
    {
        public int ProductMaterialsConsumptionGroupId { get; set; }
        public string ProductMaterialsConsumptionGroupCode { get; set; }
        public string Title { get; set; }
    }
}
