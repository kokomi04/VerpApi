using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VErp.Services.Accountancy.Model.Input
{
    public class CalcFixExchangeRateRequestModel
    {
        public int CurrencyId { get; set; }
        public decimal ExchangeRate { get; set; }
        public long ToDate { get; set; }
        public string AccountNumber { get; set; }
        public IList<string> PartnerIds { get; set; }
    }
}
