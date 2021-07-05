using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Commons.GlobalObject.InternalDataInterface
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
