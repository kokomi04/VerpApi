using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace VErp.Infrastructure.ApiCore.Attributes
{
    public class ActionButtonDataApiAttribute : Attribute, IFilterMetadata
    {
        public string RouterActionButtonIdKey { get; set; }
        public ActionButtonDataApiAttribute(string routerActionButtonIdKey)
        {
            RouterActionButtonIdKey = routerActionButtonIdKey;
        }
    }
}
