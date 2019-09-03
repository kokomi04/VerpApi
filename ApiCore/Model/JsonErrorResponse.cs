using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Infrastructure.ApiCore.Model
{
    public class JsonErrorResponse
    {
        public string[] Messages { get; set; }

        public object DeveloperMessage { get; set; }
    }
}
