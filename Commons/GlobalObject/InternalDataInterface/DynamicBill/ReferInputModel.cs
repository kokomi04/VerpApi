using System.Collections.Generic;

namespace VErp.Commons.GlobalObject.InternalDataInterface.DynamicBill
{
    public class ReferInputModel
    {
        public IList<string> CategoryCodes { get; set; }
        public IList<string> FieldNames { get; set; }

        public ReferInputModel()
        {
            CategoryCodes = new List<string>();
            FieldNames = new List<string>();
        }
    }
}
