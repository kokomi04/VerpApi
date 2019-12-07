using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Services.Stock.Model.Dictionary
{
    public class ProductCateInput
    {
        public string ProductCateName { get; set; }
        public int? ParentProductCateId { get; set; }
    }
}
