
using System;
using System.Collections.Generic;
using VErp.Commons.GlobalObject;

namespace VErp.Services.PurchaseOrder.Model.E_Invoice
{
    public class ElectronicInvoiceRequestModel
    {
        public string XmlData { get; set; }
        public string Pattern { get; set; }
        public string Serial { get; set; }
        public string Ikey { get; set; }
        public IList<string> Ikeys { get; set; }
        public string CertString { get; set; }
        public string Signature { get; set; }
        public int Option { get; set; }

        public ElectronicInvoiceRequestModel(string xmlData, string pattern, string serial)
        {
            XmlData = xmlData;
            Pattern = pattern;
            Serial = serial;
        }
    }

}