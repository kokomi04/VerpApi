using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Commons.Enums.MasterEnum
{
    public class AllowedDataTypeAttribute : Attribute
    {
        public EnumDataType[] AllowedDataType { get; private set; }
        public AllowedDataTypeAttribute(EnumDataType[] allowedDataType)
        {
            AllowedDataType = allowedDataType;
        }
    }
}
