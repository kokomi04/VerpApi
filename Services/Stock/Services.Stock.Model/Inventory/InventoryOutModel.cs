﻿using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Services.Stock.Model.Inventory
{
    public class InventoryOutModel : InventoryModelBase
    {
        public string Shipper { get; set; }
        public string Content { get; set; }

        public int? CustomerId { get; set; }
        public string Department { get; set; }
        public int? StockKeeperUserId { get; set; }

        public string BillForm { set; get; }
        public string BillCode { set; get; }
        public string BillSerial { set; get; }
        public long? BillDate { set; get; }
        public int? DepartmentId { get; set; }
        /// <summary>
        /// Id file đính kèm
        /// </summary>
        public IList<long> FileIdList { set; get; }

        public IList<InventoryOutProductModel> OutProducts { set; get; }
        //public string AccountancyAccountNumber { get; set; }
    }


    public class InventoryOutRotationModel : InventoryOutModel
    {
        public int ToStockId { get; set; }
        public string ToInventoryCode { get; set; }
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
        //public string AccountancyAccountNumberDu { get; set; }
        public long? InventoryRequirementDetailId { get; set; }
        public string InventoryRequirementCode { set; get; }

        public bool? IsSubCalculationId { get; set; }

        public IList<InventoryDetailSubCalculationModel> InProductSubs { get; set; } = new List<InventoryDetailSubCalculationModel>();
    }
}
