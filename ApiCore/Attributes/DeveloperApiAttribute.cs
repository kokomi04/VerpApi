using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VErp.Infrastructure.ApiCore.Attributes
{
    public class DeveloperApiAttribute : Attribute, IFilterMetadata
    {
    }
}
