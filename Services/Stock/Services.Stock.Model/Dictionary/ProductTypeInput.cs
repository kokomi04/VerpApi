using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Services.Stock.Model.Dictionary
{
    public class ProductTypeInput
    {
        public string ProductTypeName { get; set; }
        public int? ParentProductTypeId { get; set; }
    }
}
