using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.MasterEnum;

namespace VErp.Services.Stock.Model.Inventory
{
    public class InventoryInModel
    {        
        //public long InventoryId { get; set; }

        public int StockId { get; set; }

        /// <summary>
        /// Mã code phiếu nhập / xuất kho
        /// </summary>
        public string InventoryCode { get; set; }
              
        public string Shipper { get; set; }
        public string Content { get; set; }
        public long DateUtc { get; set; }
        public int? CustomerId { get; set; }
        public string Department { get; set; }
        public int? StockKeeperUserId { get; set; }

        public string BillCode { set; get; }

        public string BillSerial { set; get; }

        public long BillDate { set; get; }

        /// <summary>
        /// Id file đính kèm
        /// </summary>
        public IList<long> FileIdList { set; get; }

        public IList<InventoryInProductModel> InProducts { set; get; }
    }

    public class InventoryInProductModel
    {
        public long? InventoryDetailId { get; set; }
        public int ProductId { get; set; }
        public int? ProductUnitConversionId { set; get; }

        //public bool? IsFreeStyle { set; get; }

        public decimal PrimaryQuantity { get; set; }

        public decimal ProductUnitConversionQuantity { get; set; }

        public decimal UnitPrice { get; set; }

        public int? RefObjectTypeId { get; set; }
        public long? RefObjectId { get; set; }
        public string RefObjectCode { get; set; }

        public string OrderCode { get; set; }

        /// <summary>
        /// Purchase order code 
        /// </summary>
        public string POCode { get; set; }

        public string ProductionOrderCode { get; set; }

        public long? ToPackageId { set; get; }

        public EnumPackageOption PackageOptionId { set; get; }

        public int SortOrder { get; set; }
    }

    public class InventoryInProductExtendModel : InventoryInProductModel
    {
        public string ProductCode { get; set; }
    }
}
