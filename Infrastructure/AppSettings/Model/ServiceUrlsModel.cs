using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Infrastructure.AppSettings.Model
{
    public class ServiceUrlsModel
    {
        public ServiceEndpointModel ApiService { get; set; }
        public ServiceEndpointModel FileService { get; set; }
    }

    public class ServiceEndpointModel
    {
        public string Endpoint { get; set; }
    }
}
