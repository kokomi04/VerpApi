﻿using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Commons.Enums.AccountantEnum
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