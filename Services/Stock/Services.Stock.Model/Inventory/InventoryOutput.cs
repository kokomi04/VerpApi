using System;
using System.Collections.Generic;
using VErp.Infrastructure.EF.StockDB;
using VErp.Services.Stock.Model.FileResources;
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
        //public DateTime DateUtc { get; set; }
        public long Date { get; set; }

        public int? CustomerId { get; set; }
        public string Department { get; set; }
        public int? StockKeeperUserId { get; set; }

        public string BillForm { set; get; }
        public string BillCode { set; get; }
        public string BillSerial { set; get; }

        //public DateTime? BillDate { set; get; }
        public long? BillDate { set; get; }

        public decimal TotalMoney { get; set; }

        public int CreatedByUserId { get; set; }
        public int UpdatedByUserId { get; set; }

        //public DateTime CreatedDatetimeUtc { set; get; }

        //public DateTime UpdatedDatetimeUtc { set; get; }
        public long CreatedDatetimeUtc { set; get; }

        public long UpdatedDatetimeUtc { set; get; }


        public bool IsApproved { set; get; }

        public StockOutput StockOutput { get; set; }
        public IList<InventoryDetailOutput> InventoryDetailOutputList { get; set; }

        public IList<FileToDownloadInfo> FileList { set; get; }

        public IList<MappingInputBillModel> InputBills { get; set; }
    }

    public class MappingInputBillModel
    {
        public string MappingFunctionKey { get; set; }
        public int InputTypeId { get; set; }
        public string SourceId { get; set; }
        public long InputBillFId { get; set; }
    }
}
