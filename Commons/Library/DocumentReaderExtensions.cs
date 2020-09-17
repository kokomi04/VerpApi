using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VErp.Commons.Library
{
    public static class DocumentReaderExtensions
    {
        public static Table GetMainTable(this Body body)
        {
            return (Table)(body.Descendants<TableProperties>().Where(x => x.TableCaption?.Val == "main_table").FirstOrDefault()?.Parent);
        }
    }

    public static class WordOpenXmlTools
    {
        /// <summary>
        /// Find/replace within the specified paragraph.
        /// </summary>
        /// <param name="paragraph"></param>
        /// <param name="find"></param>
        /// <param name="replaceWith"></param>
        public static void ReplaceText(Paragraph paragraph, string find, string replaceWith)
        {
            var texts = paragraph.Descendants<Text>();
            for (int t = 0; t < texts.Count(); t++)
            {   // figure out which Text element within the paragraph contains the starting point of the search string
                Text txt = texts.ElementAt(t);
                for (int c = 0; c < txt.Text.Length; c++)
                {
                    var match = IsMatch(texts, t, c, find);
                    if (match != null)
                    {   // now replace the text
                        string[] lines = replaceWith.Replace(Environment.NewLine, "\r").Split('\n', '\r'); // handle any lone n/r returns, plus newline.

                        int skip = lines[lines.Length - 1].Length - 1; // will jump to end of the replacement text, it has been processed.

                        if (c > 0)
                            lines[0] = txt.Text.Substring(0, c) + lines[0];  // has a prefix
                        if (match.EndCharIndex + 1 < texts.ElementAt(match.EndElementIndex).Text.Length)
                            lines[lines.Length - 1] = lines[lines.Length - 1] + texts.ElementAt(match.EndElementIndex).Text.Substring(match.EndCharIndex + 1);

                        txt.Space = new EnumValue<SpaceProcessingModeValues>(SpaceProcessingModeValues.Preserve); // in case your value starts/ends with whitespace
                        txt.Text = lines[0];

                        // remove any extra texts.
                        for (int i = t + 1; i <= match.EndElementIndex; i++)
                        {
                            texts.ElementAt(i).Text = string.Empty; // clear the text
                        }

                        // if 'with' contained line breaks we need to add breaks back...
                        if (lines.Count() > 1)
                        {
                            OpenXmlElement currEl = txt;
                            Break br;

                            // append more lines
                            var run = txt.Parent as Run;
                            for (int i = 1; i < lines.Count(); i++)
                            {
                                br = new Break();
                                run.InsertAfter<Break>(br, currEl);
                                currEl = br;
                                txt = new Text(lines[i]);
                                run.InsertAfter<Text>(txt, currEl);
                                t++; // skip to this next text element
                                currEl = txt;
                            }
                            c = skip; // new line
                        }
                        else
                        {   // continue to process same line
                            c += skip;
                        }
                    }
                }
            }
        }



        /// <summary>
        /// Determine if the texts (starting at element t, char c) exactly contain the find text
        /// </summary>
        /// <param name="texts"></param>
        /// <param name="t"></param>
        /// <param name="c"></param>
        /// <param name="find"></param>
        /// <returns>null or the result info</returns>
        static Match IsMatch(IEnumerable<Text> texts, int t, int c, string find)
        {
            int ix = 0;
            for (int i = t; i < texts.Count(); i++)
            {
                for (int j = c; j < texts.ElementAt(i).Text.Length; j++)
                {
                    var a = texts.ElementAt(i).Text[j];
                    var b = find[ix];
                    if (find[ix] != texts.ElementAt(i).Text[j])
                    {
                        return null; // element mismatch
                    }
                    ix++; // match; go to next character
                    if (ix == find.Length)
                        return new Match() { EndElementIndex = i, EndCharIndex = j }; // full match with no issues
                }
                c = 0; // reset char index for next text element
            }
            return null; // ran out of text, not a string match
        }

        /// <summary>
        /// Defines a match result
        /// </summary>
        class Match
        {
            /// <summary>
            /// Last matching element index containing part of the search text
            /// </summary>
            public int EndElementIndex { get; set; }
            /// <summary>
            /// Last matching char index of the search text in last matching element
            /// </summary>
            public int EndCharIndex { get; set; }
        }

    }
}
