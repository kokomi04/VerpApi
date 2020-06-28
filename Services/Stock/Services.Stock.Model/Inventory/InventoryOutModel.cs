using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Services.Stock.Model.Inventory
{
    public class InventoryOutModel
    {       
        public int StockId { get; set; }
     
        public string InventoryCode { get; set; }
        
        public string Shipper { get; set; }
        public string Content { get; set; }
        public long Date { get; set; }
        public int? CustomerId { get; set; }
        public string Department { get; set; }
        public int? StockKeeperUserId { get; set; }      
        
        /// <summary>
        /// Id file đính kèm
        /// </summary>
        public IList<long> FileIdList { set; get; }

        public IList<InventoryOutProductModel> OutProducts { set; get; }
    }

    public class InventoryOutProductModel
    {
        public int ProductId { get; set; }        

        //public bool? IsFreeStyle { set; get; }
        public decimal? RequestPrimaryQuantity { get; set; }
        public decimal PrimaryQuantity { get; set; }
        public decimal UnitPrice { get; set; }

        public int ProductUnitConversionId { set; get; }
        public decimal? RequestProductUnitConversionQuantity { get; set; }
        public decimal ProductUnitConversionQuantity { get; set; }
        public decimal ProductUnitConversionPrice { get; set; }
        

        public int? RefObjectTypeId { get; set; }
        public long? RefObjectId { get; set; }
        public string RefObjectCode { get; set; }
        public long FromPackageId { set; get; }

        public string OrderCode { get; set; }

        /// <summary>
        /// Purchase order code 
        /// </summary>
        public string POCode { get; set; }

        public string ProductionOrderCode { get; set; }

        public int SortOrder { get; set; }
        public string Description { get; set; }
    }
}
