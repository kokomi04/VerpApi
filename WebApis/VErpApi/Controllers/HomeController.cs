using IdentityServer4.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using VErp.Infrastructure.AppSettings.Model;

namespace VErpApi.Controllers
{
    public class HomeController : Controller
    {
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IOptionsSnapshot<AppSetting> _settings;

        public HomeController(IIdentityServerInteractionService interaction, IOptionsSnapshot<AppSetting> settings)
        {
            _interaction = interaction;
            _settings = settings;
            
        }
   
        public async Task<IActionResult> Error(string errorId)
        {
            var message = await _interaction.GetErrorContextAsync(errorId);
            return Json(message);
        }     
    }
}