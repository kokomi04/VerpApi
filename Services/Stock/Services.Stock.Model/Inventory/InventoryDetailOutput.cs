using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Services.Stock.Model.Inventory
{
    public class InventoryDetailOutput
    {
        public long InventoryDetailId { get; set; }
        public long InventoryId { get; set; }
        public int ProductId { get; set; }
        //public DateTime CreatedDatetimeUtc { get; set; }
        //public DateTime UpdatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
        public int PrimaryUnitId { get; set; }
        public decimal PrimaryQuantity { get; set; }
        public int? SecondaryUnitId { get; set; }
        public decimal? SecondaryQuantity { get; set; }
        public DateTime ManufactureDatetimeUtc { get; set; }
        public long PackageId { get; set; }
        public int? RefObjectTypeId { get; set; }
        public long? RefObjectId { get; set; }
        public string RefObjectCode { get; set; }

        //public virtual Inventory Inventory { get; set; }
        //public virtual Package Package { get; set; }
        //public virtual Product Product { get; set; }
    }
}
