using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.PurchaseOrderDB;

namespace VErp.Services.PurchaseOrder.Model.ProductPrice
{
    public class ProductPriceConfigItemPricingUpdate
    {
        public string Currency { get; set; }
        public IList<ProductPriceConfigItemPriceModel> Items { get; set; }
    }

    public class ProductPriceConfigItemPriceModel : IMapFrom<ProductPriceConfigItemPrice>
    {
        public long ProductPriceConfigItemPriceId { get; set; }                
        public bool? IsForeignPrice { get; set; }
        public bool? IsEditable { get; set; }
        public string ItemKey { get; set; }
        public decimal? Price { get; set; }
        public string Description { get; set; }
    }
}
