using System;

namespace VErp.Commons.Enums.StandardEnum
{
    public class ErrorCodePrefixAttribute : Attribute
    {
        public string Prefix { get; private set; }
        public ErrorCodePrefixAttribute(string prefix)
        {
            Prefix = prefix;
        }
    }
}
