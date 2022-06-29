using System;

namespace VErp.Commons.Enums.MasterEnum
{
    public class DataSizeAttribute : Attribute
    {
        public int DataSize { get; private set; }
        public DataSizeAttribute(int dataSize)
        {
            DataSize = dataSize;
        }
    }
}
