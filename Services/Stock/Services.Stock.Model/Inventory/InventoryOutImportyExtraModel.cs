using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Library.Model;

namespace VErp.Services.Stock.Model.Inventory
{
    public class InventoryInputImportExtraModel_Bak
    {
        public string InventoryCode { get; set; }
        public int StockId { get; set; }

        /// <summary>
        /// Ngày phát sinh (UnixTime)
        /// </summary>
        public long IssuedDate { get; set; }

        public string Description { get; set; }

        public IList<long> FileIdList { set; get; }

        // public string AccountancyAccountNumber { get; set; }

        //public bool CreateNewPuIfNotExists { get; set; }

    }

    public class InventoryOutImportyExtraModel_Bak
    {
        public string InventoryCode { get; set; }
        public int StockId { get; set; }

        /// <summary>
        /// Ngày phát sinh (UnixTime)
        /// </summary>
        public long IssuedDate { get; set; }

        public string Description { get; set; }

        public IList<long> FileIdList { set; get; }

        // public string AccountancyAccountNumber { get; set; }

        //public bool CreateNewPuIfNotExists { get; set; }

    }

}
