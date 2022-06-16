using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace VErp.Infrastructure.ApiCore.Model
{
    public class InternalServerErrorObjectResult : ObjectResult
    {
        public InternalServerErrorObjectResult(object error)
            : base(error)
        {
            StatusCode = StatusCodes.Status500InternalServerError;
        }
    }
}
