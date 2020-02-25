using System;
using System.Collections.Generic;
using System.Text;
using VErp.Services.Stock.Model.Inventory;

namespace VErp.Services.Stock.Model.Stock
{
    public class StockSumaryReportForm04Output
    {
        public long InventoryId { get; set; }
        public string InventoryCode { get; set; }
        public int InventoryTypeId { get; set; }

        public long DateUtc { get; set; }
        public string BillCode { set; get; }

        public string Content { set; get; }

        public int? CustomerId { get; set; }
        public string CustomerCode { set; get; }
        public string CustomerName { set; get; }

        public int CreatedByUserId { get; set; }
        public int UpdatedByUserId { get; set; }
                
        public long CreatedDatetimeUtc { set; get; }

        public long UpdatedDatetimeUtc { set; get; }

        public string CreatedByUserName { set; get; }

        public string UpdatedByUserName { set; get; }

        public string Censor { set; get; }

        public long CensorDate { set; get; }

        public List<ReportForm04InventoryDetailsOutputModel> InventoryDetailsOutputModel { set; get; }
    }
        
    public class ReportForm04InventoryDetailsOutputModel
    {
        public long InventoryDetailId { get; set; }
        public long InventoryId { get; set; }
        public int ProductId { get; set; }

        public int PrimaryUnitId { get; set; }
        public decimal PrimaryQuantity { get; set; }

        public decimal UnitPrice { get; set; }

        public int? ProductUnitConversionId { set; get; }
        public string ProductName { set; get; }

        public string ProductCode { set; get; }

        public string UnitName { set; get; }

        //public int? RefObjectTypeId { get; set; }
        //public long? RefObjectId { get; set; }
        //public string RefObjectCode { get; set; }

        public string OrderCode { get; set; }

        /// <summary>
        /// Purchase order code 
        /// </summary>
        public string POCode { get; set; }

        public string ProductionOrderCode { get; set; }
    }
}
