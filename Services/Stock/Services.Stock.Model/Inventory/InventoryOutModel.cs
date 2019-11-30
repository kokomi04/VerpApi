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
        public DateTime DateUtc { get; set; }
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
        public int ProductUnitConversionId { set; get; }
        public decimal ProductUnitConversionQuantity { get; set; }
        public int? RefObjectTypeId { get; set; }
        public long? RefObjectId { get; set; }
        public string RefObjectCode { get; set; }
        public long FromPackageId { set; get; }
    }
}
