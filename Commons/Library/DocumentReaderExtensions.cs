using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using OpenXmlPowerTools;
using SelectPdf;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace VErp.Commons.Library
{
    public static class DocumentReaderExtensions
    {
    }

    public static class WordOpenXmlTools
    {
        public static void ConvertToPdf(string file, string destFileName)
        {
            string htmlString = ConvertToHtml(file, Path.GetDirectoryName(destFileName));
            ConvertHtmlToPdf(htmlString, destFileName);
        }

        public static string ConvertToHtml(string file, string outputDirectory)
        {
            var fi = new FileInfo(file);
            byte[] byteArray = File.ReadAllBytes(fi.FullName);
            using (MemoryStream memoryStream = new MemoryStream())
            {
                memoryStream.Write(byteArray, 0, byteArray.Length);
                using (WordprocessingDocument wDoc = WordprocessingDocument.Open(memoryStream, true))
                {
                    var destFileName = new FileInfo(fi.Name.Replace(".docx", ".html"));
                    if (outputDirectory != null && outputDirectory != string.Empty)
                    {
                        DirectoryInfo di = new DirectoryInfo(outputDirectory);
                        if (!di.Exists)
                        {
                            throw new OpenXmlPowerToolsException("Output directory does not exist");
                        }
                        destFileName = new FileInfo(Path.Combine(di.FullName, destFileName.Name));
                    }
                    var imageDirectoryName = destFileName.FullName.Substring(0, destFileName.FullName.Length - 5) + "_files";

                    int imageCounter = 0;

                    var pageTitle = fi.FullName;
                    var part = wDoc.CoreFilePropertiesPart;
                    if (part != null)
                    {
                        pageTitle = (string)part.GetXDocument().Descendants(DC.title).FirstOrDefault() ?? fi.FullName;
                    }

                    // TODO: Determine max-width from size of content area.
                    WmlToHtmlConverterSettings settings = new WmlToHtmlConverterSettings()
                    {
                        AdditionalCss = "body { margin: 1cm auto; max-width: 23.5cm; padding: 0; }",
                        PageTitle = pageTitle,
                        FabricateCssClasses = true,
                        CssClassPrefix = "pt-",
                        RestrictToSupportedLanguages = false,
                        RestrictToSupportedNumberingFormats = false,
                        ImageHandler = imageInfo =>
                        {
                            ++imageCounter;
                            string extension = imageInfo.ContentType.Split('/')[1].ToLower();
                            ImageFormat imageFormat = null;
                            if (extension == "png")
                                imageFormat = ImageFormat.Png;
                            else if (extension == "gif")
                                imageFormat = ImageFormat.Gif;
                            else if (extension == "bmp")
                                imageFormat = ImageFormat.Bmp;
                            else if (extension == "jpeg")
                                imageFormat = ImageFormat.Jpeg;
                            else if (extension == "tiff")
                            {
                                // Convert tiff to gif.
                                extension = "gif";
                                imageFormat = ImageFormat.Gif;
                            }
                            else if (extension == "x-wmf")
                            {
                                extension = "wmf";
                                imageFormat = ImageFormat.Wmf;
                            }

                            // If the image format isn't one that we expect, ignore it,
                            // and don't return markup for the link.
                            if (imageFormat == null)
                                return null;
                            string base64 = null;
                            try
                            {
                                using (MemoryStream ms = new MemoryStream())
                                {
                                    imageInfo.Bitmap.Save(ms, imageFormat);
                                    var ba = ms.ToArray();
                                    base64 = System.Convert.ToBase64String(ba);
                                }
                            }
                            catch (System.Runtime.InteropServices.ExternalException)
                            {
                                return null;
                            }

                            ImageFormat format = imageInfo.Bitmap.RawFormat;
                            ImageCodecInfo codec = ImageCodecInfo.GetImageDecoders().First(c => c.FormatID == format.Guid);
                            string mimeType = codec.MimeType;

                            string imageSource = string.Format("data:{0};base64,{1}", mimeType, base64);

                            XElement img = new XElement(Xhtml.img,
                                new XAttribute(NoNamespace.src, imageSource),
                                imageInfo.ImgStyleAttribute,
                                imageInfo.AltText != null ?
                                    new XAttribute(NoNamespace.alt, imageInfo.AltText) : null);
                            return img;
                        }
                    };
                    XElement htmlElement = WmlToHtmlConverter.ConvertToHtml(wDoc, settings);

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
                    return htmlString;
                }
            }
        }

        public static void ConvertHtmlToPdf(string htmlString, string destFileName)
        {
            HtmlToPdf converter = new HtmlToPdf();
            PdfDocument pdfDocument = converter.ConvertHtmlString(htmlString);
            pdfDocument.Save(destFileName);
            pdfDocument.Close();
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
