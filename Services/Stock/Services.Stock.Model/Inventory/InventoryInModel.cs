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
        public DateTime DateUtc { get; set; }
        public int? CustomerId { get; set; }
        public string Department { get; set; }
        public int? StockKeeperUserId { get; set; }

        public string DeliveryCode { set; get; }
        
        /// <summary>
        /// Id file đính kèm
        /// </summary>
        public IList<long> FileIdList { set; get; }

        public IList<InventoryInProductModel> InProducts { set; get; }
    }

    public class InventoryInProductModel
    {
        public int ProductId { get; set; }
        public int ProductUnitConversionId { set; get; }
        public decimal ProductUnitConversionQuantity { get; set; }
        public int? RefObjectTypeId { get; set; }
        public long? RefObjectId { get; set; }
        public string RefObjectCode { get; set; }
        public long? ToPackageId { set; get; }

        public EnumPackageOption PackageOptionId { set; get; }
    }
}
