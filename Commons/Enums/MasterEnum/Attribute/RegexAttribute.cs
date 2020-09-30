using System;
using System.Collections.Generic;
using System.Text;

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
