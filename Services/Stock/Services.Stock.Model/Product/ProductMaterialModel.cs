using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.StockDB;

namespace VErp.Services.Stock.Model.Product
{
    public class ProductMaterialModel : IMapFrom<ProductMaterial>
    {
        public int ProductMaterialId { get; set; }
        public int RootProductId { get; set; }
        public string PathProductIds { get; set; }
        public int ProductId { get; set; }
    }
}
