using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VErp.Infrastructure.ApiCore.Attributes;

namespace VErp.Infrastructure.ApiCore
{
    [ApiController]
    [AllowAnonymous]
    [TypeFilter(typeof(InternalCrossAuthorizeAttribute))]
    public class CrossServiceBaseController : ControllerBase
    {
       
    }
}