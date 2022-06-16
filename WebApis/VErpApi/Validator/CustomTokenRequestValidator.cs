using IdentityServer4.Validation;
using System.Threading.Tasks;

namespace VErp.WebApis.VErpApi.Validator
{
    public class CustomTokenRequestValidator : ICustomTokenRequestValidator
    {
        public Task ValidateAsync(CustomTokenRequestValidationContext context)
        {
            return Task.FromResult(0);
        }
    }
}
