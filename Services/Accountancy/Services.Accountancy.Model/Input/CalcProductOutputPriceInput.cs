using System.Collections.Generic;
using VErp.Commons.GlobalObject;

namespace VErp.Services.Accountancy.Model.Input
{
    public class CalcProductOutputPriceInput
    {
        public int? ProductId { get; set; }

        public long FromDate { get; set; }
        public long ToDate { get; set; }
        public string Tk { get; set; }
        public bool IsIgnoreZeroPrice { get; set; }
        public bool IsUpdate { get; set; }
    }


    public class CalcProductOutputPriceModel
    {
        public IList<NonCamelCaseDictionary> Data { get; set; }
        public bool IsInvalid { get; set; }
        public bool IsError { get; set; }
    }
}
