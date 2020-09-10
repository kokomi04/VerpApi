using IdentityServer4.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using VErp.Infrastructure.AppSettings.Model;

namespace VErpApi.Controllers
{
    [Route("")]
    [AllowAnonymous]
    public class HomeController : Controller
    {
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IOptionsSnapshot<AppSetting> _settings;

        public HomeController(IIdentityServerInteractionService interaction, IOptionsSnapshot<AppSetting> settings)
        {
            _interaction = interaction;
            _settings = settings;
            
        }
        [Route("")]
        public async Task<IActionResult> Index()
        {
            await Task.CompletedTask;
            return Content("Ok, I'm working!");
        }

        [Route("Error")]
        public async Task<IActionResult> Error(string errorId)
        {
            var message = await _interaction.GetErrorContextAsync(errorId);
            return Json(message);
        }     
    }
}