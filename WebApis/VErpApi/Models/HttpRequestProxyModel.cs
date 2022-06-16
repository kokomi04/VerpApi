using VErp.Commons.GlobalObject;

namespace VErpApi.Models
{
    public class HttpRequestProxyModel
    {
        public string Url { get; set; }
        public string Method { get; set; }
        public NonCamelCaseDictionary<string> Headers { get; set; }
        public string Body { get; set; }
    }
}
