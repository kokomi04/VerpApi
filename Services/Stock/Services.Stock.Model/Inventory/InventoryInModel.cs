﻿using System.Collections.Generic;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Services.Stock.Model.Package;

namespace VErp.Services.Stock.Model.Inventory
{
    public class InventoryModelBase
    {
        public int StockId { get; set; }
        public string InventoryCode { get; set; }
        public long Date { get; set; }
        public EnumInventoryAction InventoryActionId { get; set; }
        public EnumInputType? InputTypeSelectedState { get; set; }
        public EnumInputUnitType? InputUnitTypeSelectedState { get; set; }
    }
    public class InventoryInModel : InventoryModelBase
    {
        //public long InventoryId { get; set; }

        public string Shipper { get; set; }
        public string Content { get; set; }

        public int? CustomerId { get; set; }
        public string Department { get; set; }
        public int? StockKeeperUserId { get; set; }

        public string BillForm { set; get; }
        public string BillCode { set; get; }
        public string BillSerial { set; get; }
        public long? BillDate { set; get; }
        //public string AccountancyAccountNumber { get; set; }

        public int? DepartmentId { get; set; }
        /// <summary>
        /// Id file đính kèm
        /// </summary>
        public IList<long> FileIdList { set; get; }

        public IList<InventoryInProductModel> InProducts { set; get; }

        public long UpdatedDatetimeUtc { get; set; }
    }

    public class InventoryInProductModel
    {
        public long? InventoryDetailId { get; set; }
        public int ProductId { get; set; }

        //public bool? IsFreeStyle { set; get; }

        public decimal? RequestPrimaryQuantity { get; set; }

        public decimal PrimaryQuantity { get; set; }
        public decimal? UnitPrice { get; set; }

        public int ProductUnitConversionId { set; get; }
        public decimal? RequestProductUnitConversionQuantity { get; set; }
        public decimal ProductUnitConversionQuantity { get; set; }
        public decimal? ProductUnitConversionPrice { get; set; }
        public decimal? Money { get; set; }
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

        public PackageInputModel ToPackageInfo { get; set; }

        public EnumPackageOption PackageOptionId { set; get; }

        public int SortOrder { get; set; }
        public string Description { get; set; }

        //public string AccountancyAccountNumberDu { get; set; }
        public long? InventoryRequirementDetailId { get; set; }
        public string InventoryRequirementCode { get; set; }

        public bool? IsSubCalculation { get; set; }

        public IList<InventoryDetailSubCalculationModel> InProductSubs { get; set; } = new List<InventoryDetailSubCalculationModel>();

    }

    public class InventoryInProductExtendModel : InventoryInProductModel
    {
        public string ProductCode { get; set; }
    }
}
