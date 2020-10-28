using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.MasterEnum;

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
