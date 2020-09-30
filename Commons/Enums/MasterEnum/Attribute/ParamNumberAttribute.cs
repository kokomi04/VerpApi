using System;
using System.Collections.Generic;
using System.Text;

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
