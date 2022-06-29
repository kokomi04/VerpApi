using System;

namespace VErp.Commons.Enums.MasterEnum
{
    public class ParamNumberAttribute : Attribute
    {
        public int ParamNumber { get; private set; }
        public ParamNumberAttribute(int paramNumber)
        {
            ParamNumber = paramNumber;
        }
    }
}
