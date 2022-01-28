using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Commons.Enums.MasterEnum
{
    public class RangeValueAttribute : Attribute
    {
        public string[] RangeValue { get; private set; }

        public RangeValueAttribute(string[] rangeValue)
        {
            RangeValue = rangeValue;
        }
    }
}
