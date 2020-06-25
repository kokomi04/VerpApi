using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Services.Stock.Model.Dictionary
{
    public class ProductCateOutput
    {
        public int ProductCateId { get; set; }
        public string ProductCateName { get; set; }
        public int? ParentProductCateId { get; set; }
        public int SortOrder { get; set; }
    }
}
