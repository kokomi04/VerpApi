using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace VErp.Commons.Enums.StandardEnum
{
    public class EnumStatusCodeAttribute : Attribute
    {
        public HttpStatusCode StatusCode { get; private set; }
        public EnumStatusCodeAttribute(HttpStatusCode statusCode)
        {
            StatusCode = statusCode;
        }
    }
}
