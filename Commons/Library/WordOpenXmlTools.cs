using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using OpenXmlPowerTools;
using OpenXmlPowerTools.OpenXMLWordprocessingMLToHtmlConverter;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using VErp.Commons.Library.OpenXmlTools;
using VErp.Infrastructure.AppSettings.Model;

namespace VErp.Commons.Library
{
    public static class WordOpenXmlTools
    {
        public static async Task<Stream> ConvertToPdf(string fileDocPath, PuppeteerPdfSetting puppeteerPdfSetting)
        {
           return await ConvertHtmlToPdf(ConvertToHtml(fileDocPath), puppeteerPdfSetting) ;
        }

        public static string ConvertToHtml(string fileDocPath)
        {
            var fileInfo = new FileInfo(fileDocPath);
            byte[] byteArray = File.ReadAllBytes(fileInfo.FullName);
            using (MemoryStream memoryStream = new MemoryStream())
            {
                memoryStream.Write(byteArray, 0, byteArray.Length);
                using (WordprocessingDocument wDoc = WordprocessingDocument.Open(memoryStream, true))
                {
                    if (!fileInfo.Directory.Exists)
                    {
                        throw new OpenXmlPowerToolsException("Output directory does not exist");
                    }

                    var destFileName = new FileInfo(Path.Combine(fileInfo.Directory.FullName, Path.GetFileNameWithoutExtension(fileInfo.Name) + ".html"));

                    var imageDirectoryName = destFileName.FullName.Substring(0, destFileName.FullName.Length - 5) + "_files";

                    var pageTitle = fileInfo.FullName;
                    var part = wDoc.CoreFilePropertiesPart;
                    if (part != null)
                    {
                        pageTitle = (string)part.GetXDocument().Descendants(DC.title).FirstOrDefault() ?? fileInfo.FullName;
                    }

                    var settings = new WmlToHtmlConverterSettings(pageTitle, new CustomImageHandler(imageDirectoryName), new TextDummyHandler(), new SymbolHandler(), new CustomBreakHandler(), true);
                    XElement htmlElement = WmlToHtmlConverter.ConvertToHtml(wDoc, settings);

                    var elements = htmlElement.Elements().ToList();
                    foreach (var e in elements)
                    {
                        if (e.Name == Xhtml.body)
                        {
                            var bodyElements = e.Descendants().Where(x => x.Name == Xhtml.table || x.Name == Xhtml.tr).ToList();
                            foreach (var elementIns in bodyElements)
                            {

                                var styleAttr = elementIns.Attributes().FirstOrDefault(a => a.Name == "style");
                                var value = elementIns.Name == Xhtml.table ? "page-break-inside:auto" : "page-break-inside:avoid; page-break-after:auto ";
                                if (styleAttr == null)
                                {
                                    elementIns.SetAttributeValue(NoNamespace.style, value);
                                }
                                else
                                {
                                    elementIns.SetAttributeValue(styleAttr.Name, value + ";" + styleAttr.Value);
                                }
                            }
                        }
                    }

                    // Produce HTML document with <!DOCTYPE html > declaration to tell the browser
                    // we are using HTML5.
                    var html = new XDocument(
                        new XDocumentType("html", null, null, null),
                        htmlElement);

                    // Note: the xhtml returned by ConvertToHtmlTransform contains objects of type
                    // XEntity.  PtOpenXmlUtil.cs define the XEntity class.  See
                    // http://blogs.msdn.com/ericwhite/archive/2010/01/21/writing-entity-references-using-linq-to-xml.aspx
                    // for detailed explanation.
                    //
                    // If you further transform the XML tree returned by ConvertToHtmlTransform, you
                    // must do it correctly, or entities will not be serialized properly.

                    var htmlString = html.ToString(SaveOptions.DisableFormatting);
                    File.WriteAllText(destFileName.FullName, htmlString, Encoding.UTF8);
                    return destFileName.FullName;
                }
            }
        }

        public static async Task<Stream> ConvertHtmlToPdf(string fileHtmlPath, PuppeteerPdfSetting puppeteerSetting)
        {
            var product = puppeteerSetting?.Product == null ? Product.Chrome : (Product)puppeteerSetting?.Product;
            var executablePath = puppeteerSetting?.ExecutablePath;

            if (string.IsNullOrWhiteSpace(executablePath))
            {
                string path = Path.Combine(puppeteerSetting?.Path, product == Product.Chrome? "chrome" : "firefox");
                int version = !string.IsNullOrWhiteSpace(puppeteerSetting?.Version) ? int.Parse(puppeteerSetting?.Version, CultureInfo.CurrentCulture.NumberFormat) : BrowserFetcher.DefaultRevision;
                string host = puppeteerSetting?.Host;

                var dirPath = new DirectoryInfo(path);
                if (!dirPath.Exists)
                    dirPath = Directory.CreateDirectory(path);

                var option = new BrowserFetcherOptions
                {
                    Path = dirPath.FullName,
                    Product = product,
                    Host = host
                };

                var browserFetcher = new BrowserFetcher(option);
                await browserFetcher.DownloadAsync(version);
                executablePath = browserFetcher.GetExecutablePath(version);
            }


            await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true,
                ExecutablePath = executablePath,
                Product = product
            });

            var absolutePath = Path.GetFullPath(fileHtmlPath);
            await using var page = await browser.NewPageAsync();
            await page.GoToAsync($"file://{absolutePath}").ConfigureAwait(false);
            return await page.PdfStreamAsync(new PdfOptions
            {
                Format = PaperFormat.A4,
                PrintBackground = true,
                MarginOptions = new MarginOptions
                {
                    Bottom = "1cm",
                    Top = "1cm",
                    Left = "1cm",
                    Right = "1cm",

                },
            }).ConfigureAwait(false);
        }

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
