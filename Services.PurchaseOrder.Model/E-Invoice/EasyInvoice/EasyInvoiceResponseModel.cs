using System.Collections.Generic;

namespace VErp.Services.PurchaseOrder.Model.E_Invoice.EasyInvoice
{
    public class EasyInvoiceResponseModel
    {
        public int? Status { get; set; }
        public string Message { get; set; }
        public ResponseData Data { get; set; }
    }

    public class ResponseData
    {
        public string Pattern { get; set; }
        public string Serial { get; set; }
        public Dictionary<string, string> KeyInvoiceNo { get; set; }
        public Dictionary<string, string> KeyInvoiceMsg { get; set; }
        public string Html { get; set; }
        public int? InvoiceStatus { get; set; }
        public List<int> InvoiceNo { get; set; }
        public Dictionary<string, string> DigestData { set; get; }
        public string Ikey { get; set; }
        public string[] Ikeys { get; set; }
        public List<InvoiceInfo> Invoices { get; set; }
    }

    public class InvoiceInfo
    {
        public int InvoiceStatus { get; set; }
        public string Pattern { get; set; }
        public string Serial { get; set; }
        public int No { get; set; }
        public string LookupCode { get; set; }
        public string Ikey { get; set; }
        public string ArisingDate { get; set; }
        public string IssueDate { get; set; }
        public string CustomerName { get; set; }
        public string CustomerCode { get; set; }
        public string Buyer { get; set; }
        public decimal Amount { get; set; }
    }
}