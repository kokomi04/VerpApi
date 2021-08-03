using System;
using System.Collections.Generic;
using System.Text;
using static VErp.Commons.GlobalObject.InternalDataInterface.ProductModel;

namespace VErp.Services.Stock.Model.Product.Partial
{
    public class ProductPartialSellModel
    {
        //public int? CustomerId { get; set; }
        public decimal? Measurement { get; set; }
        public string PackingMethod { get; set; }
        public decimal? GrossWeight { get; set; }
        public decimal? NetWeight { get; set; }
        public decimal? LoadAbility { get; set; }
        public string SellDescription { get; set; }

        public IList<ProductModelCustomer> ProductCustomers { get; set; }
    }
}
