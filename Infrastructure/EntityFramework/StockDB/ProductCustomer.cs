using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.StockDB
{
    public partial class ProductCustomer
    {
        public long ProductCustomerId { get; set; }
        public int ProductId { get; set; }
        public int? CustomerId { get; set; }
        public string CustomerProductCode { get; set; }
        public string CustomerProductName { get; set; }
        public int CreatedByUserId { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }

        public virtual Product Product { get; set; }
    }
}
