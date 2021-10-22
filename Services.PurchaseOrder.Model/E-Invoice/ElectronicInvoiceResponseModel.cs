
using System;
using System.Collections.Generic;
using VErp.Commons.GlobalObject;

namespace VErp.Services.PurchaseOrder.Model.E_Invoice
{
    public class ElectronicInvoiceResponseModel
    {
        public int Status { get; set; }
        public string Message { get; set; }
        public string Data { get; set; }
    }

    public class CreateElectronicInvoiceSuccess
    {
        public string Parttern { get; set; }
        public string Serial { get; set; }
        public IList<InvoiceResponse> Invoices { get; set; }

    }

    public class ModifyElectronicInvoiceSuccess
    {
        public string Parttern { get; set; }
        public string Serial { get; set; }
        public NonCamelCaseDictionary KeyInvoiceNo { get; set; }
        public IList<InvoiceResponse> Invoices { get; set; }
        public NonCamelCaseDictionary DigestData { get; set; }

    }

    public class PublishElectronicInvoiceSuccess
    {
        public string Parttern { get; set; }
        public string Serial { get; set; }
        public KeyInvoiceNoModel KeyInvoiceNo { get; set; }

    }

    public class KeyInvoiceNoModel
    {
        public string Ikey {get;set;}
        public IList<InvoiceResponse> Invoices { get; set; }
    }

    public class InvoiceResponse
    {
        public int InvoiceStatus { get; set; }

        public string Pattern { get; set; }

        public string Serial { get; set; }

        public string No { get; set; }

        public string LookupCode { get; set; }

        public string Ikey { get; set; }

        public string ArisingDate { get; set; }

        public DateTime IssueDate { get; set; }

        public string CustomerName { get; set; }

        public string CustomerCode { get; set; }

        public string Buyer { get; set; }

        public Decimal Amount { get; set; }
    }

    public class ElectronicInvoiceError
    {
        public NonCamelCaseDictionary KeyInvoiceMsg { get; set; }

        public static string KEY_ERROR_PREFIX = "ikey";
    }


}