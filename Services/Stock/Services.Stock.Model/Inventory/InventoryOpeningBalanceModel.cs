using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Library.Model;

namespace VErp.Services.Stock.Model.Inventory
{
    public class InventoryOpeningBalanceModel
    {
        public string InventoryCode { get; set; }
        public int StockId { get; set; }

        /// <summary>
        /// Loại 1: nhập kho | 2: xuất kho
        /// </summary>
        public EnumInventoryType InventoryTypeId { set; get; }

        /// <summary>
        /// Ngày phát sinh (UnixTime)
        /// </summary>
        public long IssuedDate { get; set; }

        public string Description { get; set; }

        public IList<long> FileIdList { set; get; }

        public string AccountancyAccountNumber { get; set; }
    }

}
