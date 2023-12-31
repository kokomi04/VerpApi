﻿namespace VErp.Services.Stock.Model.Stock
{
    /// <summary>
    /// Cảnh báo số lượng tồn kho theo min max của sản phẩm
    /// </summary>
    public class StockProductQuantityWarning
    {
        public int ProductId { get; set; }

        public string ProductCode { get; set; }

        public string ProductName { get; set; }

        public string Specification { set; get; }

        public int PrimaryUnitId { set; get; }

        public string PrimaryUnitName { set; get; }

        public long AmountWarningMin { set; get; }

        public long AmountWarningMax { set; get; }

        public decimal? TotalPrimaryQuantityRemaining { set; get; }

        // public List<StockProductQuantity> StockProductQuantityList { set; get; }
        public long? MainImageFileId { get; set; }
        public int DecimalPlace { get; set; }
        public bool IsReachMin { get; set; }
        public bool IsReachMax { get; set; }
    }

    public class StockProductQuantity
    {
        public int StockId { set; get; }

        public string StockName { set; get; }

        //public int ProductUnitConversionId { set; get; }

        public decimal PrimaryQuantityRemaining { set; get; }

        //public decimal ProductUnitConversionRemaining { set; get; }

    }

}
