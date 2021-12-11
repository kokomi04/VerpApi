using System.Collections.Generic;
using System.Net;
using VErp.Commons.GlobalObject;

namespace VErpApi.Models
{
    public class HttpResponseModel
    {
        public string Body { get; set; }

        public bool IsSuccessStatusCode { get; set; }

        public HttpStatusCode StatusCode { get; set; }
        public NonCamelCaseDictionary<string> Headers { get; set; }
    }
}