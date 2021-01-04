using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.MasterEnum;

namespace VErp.Infrastructure.ApiCore.Attributes
{
    public class ObjectDataApiAttribute: Attribute, IFilterMetadata
    {
        public EnumObjectType ObjectTypeId { get; set; }
        public string RouterDataKey { get; set; }
        public ObjectDataApiAttribute(EnumObjectType objectTypeId, string routerDataKey)
        {
            ObjectTypeId = objectTypeId;
            RouterDataKey = routerDataKey;
        }
    }

    public interface 
}
