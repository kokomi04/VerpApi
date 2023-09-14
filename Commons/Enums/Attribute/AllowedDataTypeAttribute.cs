using System;

namespace VErp.Commons.Enums.MasterEnum
{
    public class AllowedDataTypeAttribute : Attribute
    {
        public EnumDataType[] AllowedDataType { get; private set; }
        public EnumDataType DataType { get; }

        public AllowedDataTypeAttribute(EnumDataType[] allowedDataType)
        {
            AllowedDataType = allowedDataType;
        }

        public AllowedDataTypeAttribute(EnumDataType dataType)
        {
            DataType = dataType;
        }
    }
}
