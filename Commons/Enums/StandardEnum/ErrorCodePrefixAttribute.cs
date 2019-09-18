using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Commons.Enums.StandardEnum
{
    public class ErrorCodePrefixAttribute: Attribute
    {
        public string Prefix { get; private set; }
        public ErrorCodePrefixAttribute(string prefix)
        {
            Prefix = prefix;
        }
    }
}
