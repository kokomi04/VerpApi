using System;

namespace VErp.Commons.GlobalObject.Attributes
{
    public class PatternHubAttribute : Attribute
    {
        public string Pattern { get; set; }

        public PatternHubAttribute(string pattern)
        {
            Pattern = pattern;
        }
    }
}