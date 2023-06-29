using System.Collections.Generic;

namespace VErp.Commons.GlobalObject.InternalDataInterface.DynamicBill
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
