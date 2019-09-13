using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.MasterEnum;

namespace VErp.Infrastructure.ApiCore.Attributes
{
    public class VErpActionAttribute : ActionFilterAttribute
    {
        public EnumAction Action { get; set; }
        public VErpActionAttribute(EnumAction action)
        {
            Action = action;
        }
    }
}
