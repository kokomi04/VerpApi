using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Library.Model;

namespace VErp.Services.Stock.Model.Inventory
{
    public class InventoryOpeningBalanceModel
    {
        public int StockId { get; set; }

        /// <summary>
        /// Loại 1: nhập kho | 2: xuất kho
        /// </summary>
        public EnumInventoryType Type { set; get; }

        /// <summary>
        /// Ngày phát sinh (UnixTime)
        /// </summary>
        public long IssuedDate { get; set; }

        public string Description { get; set; }

        public IList<long> FileIdList { set; get; }

        public string AccountancyAccountNumber { get; set; }
    }

    public class InventoryOpeningImportModel
    {
        public ImportExcelMapping Mapping { get; set; }
        public InventoryOpeningBalanceModel Info { get; set; }
    }

}
