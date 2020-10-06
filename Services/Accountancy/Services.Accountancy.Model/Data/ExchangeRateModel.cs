using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;

namespace VErp.Services.Accountancy.Model.Data
{
    public class ExchangeRateModel
    {
        public ICollection<NonCamelCaseDictionary> Rows { get; set; }
        public NonCamelCaseDictionary Head { get; set; }
    }
}
