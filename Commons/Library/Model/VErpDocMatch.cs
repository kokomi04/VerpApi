using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using VErp.Commons.GlobalObject;

namespace VErp.Commons.Library.Model
{
    public class VErpDocMatch
    {
        public Paragraph paragraph { get; set; }
        public NonCamelCaseDictionary<string> matchs { get; set; }

        public VErpDocMatch(Paragraph paragraph, string pattern)
        {
            this.paragraph = paragraph;
            this.matchs = new NonCamelCaseDictionary<string>();
            foreach (Match match in Regex.Matches(paragraph.InnerText, pattern))
            {
                matchs.Add(match.Groups[0].Value, match.Groups[1].Value);
            }
        }
    }
}
