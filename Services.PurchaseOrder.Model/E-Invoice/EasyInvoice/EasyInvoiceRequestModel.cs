using System.Collections.Generic;

namespace VErp.Services.PurchaseOrder.Model.E_Invoice.EasyInvoice
{
    public class EasyInvoiceRequestModel
    {
        public string Ikey { get; set; }
        public string[] Ikeys { get; set; }
        public string Pattern { get; set; }
        public string Serial { get; set; }
        public string XmlData { get; set; }
        public int? Convert { get; set; }
        public string SignDate { get; set; }
        public int Quantity { get; set; }
        public Dictionary<string, string> IkeyEmail { get; set; }
        public string CertString { get; set; }
        public Dictionary<string, string> Signature { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public Dictionary<string, string> IkeyDate { get; set; }
        public int Option { get; set; }
    }
}