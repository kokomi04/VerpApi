using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;

namespace VErp.Commons.GlobalObject.InternalDataInterface
{
    public class ReferFromCategoryModel
    {
        public string CategoryCode { get; set; }
        public IList<string> FieldNames { get; set; }
        public NonCamelCaseDictionary CategoryRow { get; set; }
        public ReferFromCategoryModel()
        {
            FieldNames = new List<string>();
        }
    }
}
