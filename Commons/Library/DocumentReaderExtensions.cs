using DocumentFormat.OpenXml.Wordprocessing;
using System.Collections.Generic;
using System.Linq;
using VErp.Commons.Library.Model;

namespace VErp.Commons.Library
{
    public static class DocumentReaderExtensions
    {
        public static List<VErpDocMatch> MatchingLinesWithRegex(this IEnumerable<Paragraph> paragraphs, string pattern)
        {
            return paragraphs.Select(p => new VErpDocMatch(p, pattern)).ToList();
        }

        public static void AutoFitWindow(this Table table)
        {
            var tablePr = table.Elements<TableProperties>().FirstOrDefault();

            tablePr.TableWidth.Width = "5000";
            tablePr.TableWidth.Type = TableWidthUnitValues.Pct;
        }

        public static void AutoFitContents(this Table table)
        {
            var tablePr = table.Elements<TableProperties>().FirstOrDefault();

            tablePr.TableWidth.Width = "0";
            tablePr.TableWidth.Type = TableWidthUnitValues.Auto;
        }
    }
}
