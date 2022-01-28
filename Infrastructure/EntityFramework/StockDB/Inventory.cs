﻿using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.StockDB
{
    public partial class Inventory
    {
        public Inventory()
        {
            InventoryDetail = new HashSet<InventoryDetail>();
            InverseRefInventory = new HashSet<Inventory>();
        }

        public long InventoryId { get; set; }
        public int SubsidiaryId { get; set; }
        public int StockId { get; set; }
        public string InventoryCode { get; set; }
        public int InventoryTypeId { get; set; }
        public string Shipper { get; set; }
        public string Content { get; set; }
        public DateTime Date { get; set; }
        public int? CustomerId { get; set; }
        public string Department { get; set; }
        public int? StockKeeperUserId { get; set; }
        public string BillForm { get; set; }
        public string BillCode { get; set; }
        public string BillSerial { get; set; }
        public DateTime? BillDate { get; set; }
        public decimal TotalMoney { get; set; }
        public int CreatedByUserId { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public int? CensorByUserId { get; set; }
        public DateTime? CensorDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsApproved { get; set; }
        public bool IsOpening { get; set; }
        //public string AccountancyAccountNumber { get; set; }
        public int? DepartmentId { get; set; }
        public int InventoryActionId { get; set; }
        public int InventoryStatusId { get; set; }
        public DateTime? CheckedDatetimeUtc { get; set; }
        public int? CheckedByUserId { get; set; }
        public long? RefInventoryId { get; set; }

        public virtual Inventory RefInventory { get; set; }
        public virtual Stock Stock { get; set; }
        public virtual ICollection<InventoryDetail> InventoryDetail { get; set; }
        public virtual ICollection<Inventory> InverseRefInventory { get; set; }
    }
}
