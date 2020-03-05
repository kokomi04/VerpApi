using System;
using System.Collections.Generic;
using System.Text;
using VErp.Infrastructure.EF.StockDB;

namespace VErp.Services.Stock.Model.Stock
{
    /// <summary>
    /// Model Báo cáo chi tiết nhập xuất sp trong kỳ
    /// </summary>
    public class StockProductDetailsReportOutput
    {
        /// <summary>
        /// Số dư đầu kỳ
        /// </summary>
        public List<OpeningStockProductModel> OpeningStock { set; get; }

        /// <summary>
        /// Chi tiết nhập xuất kho
        /// </summary>
        public List<StockProductDetailsModel> Details { set; get; }
    }

    /// <summary>
    /// Chi tiết nhập xuất kho
    /// </summary>
    public class StockProductDetailsModel
    {
        public long InventoryId { get; set; }
        public int StockId { get; set; }
        public string StockName { get; set; }

        public long IssuedDate { set; get; }

        public string InventoryCode { set; get; }

        public int InventoryTypeId { set; get; }

        public string Description { set; get; }

        public string SecondaryUnitName { set; get; }

        public long InventoryDetailId { get; set; }

        public string RefObjectCode { get; set; }

        public int PrimaryUnitId { get; set; }
        public decimal PrimaryQuantity { get; set; }
        public int? SecondaryUnitId { get; set; }
        public decimal? SecondaryQuantity { get; set; }

        public int? ProductUnitConversionId { set; get; }

        public decimal EndOfPerdiodPrimaryQuantity { set; get; }
        public decimal EndOfPerdiodProductUnitConversionQuantity { set; get; }

        public ProductUnitConversion ProductUnitConversion { set; get; }

    }

    /// <summary>
    /// Tồn kho đầu kỳ
    /// </summary>
    public class OpeningStockProductModel
    {
        public int PrimaryUnitId { set; get; }

        public decimal Total { set; get; }
    }
}
