using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Services.Stock.Model.Product
{
    public class ProductListOutput
    {
        public int ProductId { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public long? MainImageFileId { get; set; }
        public int? ProductTypeId { get; set; }
        public string ProductTypeName { get; set; }
        public int ProductCateId { get; set; }
        public string ProductCateName { get; set; }
        public string Barcode { get; set; }
        public string Specification { get; set; }
        public int UnitId { get; set; }
        public string UnitName { get; set; }
    }
}
