using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.StockDB
{
    public partial class StockTakeAcceptanceCertificate
    {
        public long StockTakePeriodId { get; set; }
        public string StockTakeAcceptanceCertificateCode { get; set; }
        public DateTime StockTakeAcceptanceCertificateDate { get; set; }
        public int StockTakeAcceptanceCertificateStatus { get; set; }
        public string Content { get; set; }

        public virtual StockTakePeriod StockTakePeriod { get; set; }
    }
}
