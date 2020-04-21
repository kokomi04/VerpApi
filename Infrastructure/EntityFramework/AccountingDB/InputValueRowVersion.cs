using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.AccountingDB
{
    public partial class InputValueRowVersion : BaseEntity
    {
        public InputValueRowVersion()
        {
            InputValueRowVersionNumbers = new HashSet<InputValueRowVersionNumber>();
        }

        public long InputValueRowVersionId { get; set; }
        public long InputValueRowId { get; set; }
        public string Field0 { get; set; }
        public string Field1 { get; set; }
        public string Field2 { get; set; }
        public string Field3 { get; set; }
        public string Field4 { get; set; }
        public string Field5 { get; set; }
        public string Field6 { get; set; }
        public string Field7 { get; set; }
        public string Field8 { get; set; }
        public string Field9 { get; set; }
        public string Field10 { get; set; }
        public string Field11 { get; set; }
        public string Field12 { get; set; }
        public string Field13 { get; set; }
        public string Field14 { get; set; }
        public string Field15 { get; set; }
        public string Field16 { get; set; }
        public string Field17 { get; set; }
        public string Field18 { get; set; }
        public string Field19 { get; set; }
        public string Field20 { get; set; }

        public virtual InputValueRow InputValueRow { get; set; }

        public virtual ICollection<InputValueRowVersionNumber> InputValueRowVersionNumbers { get; set; }
    }
}
