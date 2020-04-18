using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Commons.Enums.AccountantEnum
{
    public class IsRefAttribute : Attribute
    {
        public bool IsReference { get; private set; }
        public IsRefAttribute(bool isReference)
        {
            IsReference = isReference;
        }

    }
}
