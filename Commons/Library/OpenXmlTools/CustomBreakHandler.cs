using OpenXmlPowerTools;
using OpenXmlPowerTools.OpenXMLWordprocessingMLToHtmlConverter;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace VErp.Commons.Library.OpenXmlTools
{
    public class CustomBreakHandler: IBreakHandler
    {
        BreakHandler defaultHandle = new BreakHandler();
        public IEnumerable<XNode> TransformBreak(XElement element)
        {
            XElement span = default!;

            if (element.Name == W.br && element.Attribute(W.type)?.Value == "page")
            {
                XElement div = new XElement(Xhtml.div);
                div.SetAttributeValue(NoNamespace.style, "page-break-before:always;");
                return new XNode[]
                {
                    div,
                    new XEntity("#x200e"),
                    span
                };
            }
            return defaultHandle.TransformBreak(element);
        }
    }
}
