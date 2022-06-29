using System;

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
