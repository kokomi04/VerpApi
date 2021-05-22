using System.Collections.Generic;
using VErp.Services.Stock.Model.Stock;

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
        public decimal? EstimatePrice { get; set; }
        public bool IsProductSemi { get; set; }
        public bool IsProduct { get; set; }
        public int Coefficient { get; set; }
        public decimal? Long { get; set; }
        public decimal? Height { get; set; }
        public decimal? Width { get; set; }
        public int DecimalPlace { get; set; }

        public List<StockProductOutput> StockProductModelList { set; get; }
    }
}
