using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.AccountingDB
{
    public partial class InputValueRowVersionNumber : BaseEntity
    {
        public InputValueRowVersionNumber()
        {
        }

        public long InputValueRowVersionId { get; set; }
        public long Field0 { get; set; }
        public long Field1 { get; set; }
        public long Field2 { get; set; }
        public long Field3 { get; set; }
        public long Field4 { get; set; }
        public long Field5 { get; set; }
        public long Field6 { get; set; }
        public long Field7 { get; set; }
        public long Field8 { get; set; }
        public long Field9 { get; set; }
        public long Field10 { get; set; }
        public long Field11 { get; set; }
        public long Field12 { get; set; }
        public long Field13 { get; set; }
        public long Field14 { get; set; }
        public long Field15 { get; set; }
        public long Field16 { get; set; }
        public long Field17 { get; set; }
        public long Field18 { get; set; }
        public long Field19 { get; set; }
        public long Field20 { get; set; }

        public virtual InputValueRowVersion InputValueRowVersion { get; set; }


    }
}
