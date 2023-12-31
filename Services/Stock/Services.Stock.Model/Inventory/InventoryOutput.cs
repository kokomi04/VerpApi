﻿using System.Collections.Generic;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Services.Stock.Model.FileResources;
using VErp.Services.Stock.Model.Stock;

namespace VErp.Services.Stock.Model.Inventory
{
    public class InventoryOutputQueryModel
    {
        public InventoryOutputQueryModel()
        {
           
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
        //public int? DepartmentId { get; set; }

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
        //public string AccountancyAccountNumber { get; set; }
        public int? DepartmentId { get; set; }
        public bool IsInputBillCreated { get; set; }
        public int? CensorByUserId { get; set; }
        public EnumInventoryAction InventoryActionId { get; set; }
        public int InventoryStatusId { get; set; }

        public long? RefInventoryId { get; set; }
        public string RefInventoryCode { get; set; }
        public int? RefStockId { get; set; }


    }

    public class InventoryListOutput : InventoryOutputQueryModel
    {
        public InventoryListOutput()
        {
         
        }

        public StockOutput StockOutput { get; set; }
        public IList<MappingInputBillModel> InputBills { get; set; }


    }

    public class InventoryListProductOutput: InventoryListOutput
    {
        public long InventoryDetailId { get; set; }
        public int ProductId { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string Description { get ; set; }
        public int UnitId { get; set; }
        public string UnitName { get; set; }
        public int ProductUnitConversionId { set; get; }
        public string ProductUnitConversionName { set; get; }
        public decimal PrimaryQuantity { get; set; }
        public decimal ProductUnitConversionQuantity { get; set; }
        public string PoCode { get; set; }
        public string OrderCode { get; set; }
        public string ProductionOrderCode { get; set; }
    }


    public class InventoryOutput: InventoryListOutput
    {
        public InventoryOutput()
        {
            InventoryDetailOutputList = new List<InventoryDetailOutput>(50);
        }


        public EnumInputType? InputTypeSelectedState { get; set; }
        public EnumInputUnitType? InputUnitTypeSelectedState { get; set; }
        public IList<InventoryDetailOutput> InventoryDetailOutputList { get; set; }
        public IList<FileToDownloadInfo> FileList { set; get; }
    }

    public class MappingInputBillModel
    {
        //public string MappingFunctionKey { get; set; }
        public string SourceBillCode { get; set; }
        public string SoCt { get; set; }
        public int InputTypeId { get; set; }
        public string InputType_Title { get; set; }
        //public string SourceId { get; set; }
        public long InputBillFId { get; set; }
        public EnumObjectType BillObjectTypeId { get; set; }
    }
}
