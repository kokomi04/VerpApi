using System.Collections.Generic;

namespace VErp.Services.Stock.Model.Product
{
    public class ProductQueryByProductCodeOrInternalNameRequest
    {
        public IList<string> ProductCodes { get; set; }
        public IList<string> ProductInternalNames { get; set; }
    }
}
