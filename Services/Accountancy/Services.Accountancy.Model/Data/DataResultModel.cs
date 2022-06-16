using System.Collections.Generic;
using VErp.Commons.GlobalObject;

namespace VErp.Services.Accountancy.Model.Data
{
    public class DataResultModel
    {
        public ICollection<NonCamelCaseDictionary> Rows { get; set; }
        public NonCamelCaseDictionary Head { get; set; }
    }
}
