using System;
using System.Collections.Generic;
using VErp.Infrastructure.EF.StockDB;
using VErp.Services.Stock.Model.Stock;

namespace VErp.Services.Stock.Model.Inventory
{
    public class InventoryOutput
    {
        public InventoryOutput()
        {
            InventoryDetailOutputList = new List<InventoryDetailOutput>(50);
        }

        public long InventoryId { get; set; }
        public int StockId { get; set; }
        public string InventoryCode { get; set; }
        public int InventoryTypeId { get; set; }
        public string Shipper { get; set; }
        public string Content { get; set; }
        public DateTime DateUtc { get; set; }
        public int? CustomerId { get; set; }
        public string Department { get; set; }
        public int? UserId { get; set; }        
        public int CreatedByUserId { get; set; }
        public int UpdatedByUserId { get; set; }
        //public DateTime CreatedDatetimeUtc { get; set; }
        //public DateTime UpdatedDatetimeUtc { get; set; }
        //public bool IsDeleted { get; set; }
        public bool IsApproved { set; get; }

        public StockOutput StockOutput { get; set; }
        public List<InventoryDetailOutput> InventoryDetailOutputList { get; set; }

        public List<File> FileList { set; get; }
    }
}
