using System;

namespace VErp.Commons.Enums.MasterEnum
{
    public class RegexAttribute : Attribute
    {
        public string Regex { get; private set; }
        public RegexAttribute(string regex)
        {
            Regex = regex;
        }
    }
}
