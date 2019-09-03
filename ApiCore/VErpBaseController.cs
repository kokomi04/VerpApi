using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace VErp.Infrastructure.ApiCore
{
    [ApiController]
    [Authorize]
    public class VErpBaseController : ControllerBase
    {
        private int _userId = 0;
        private string _clientId = string.Empty;
        private string _sub = string.Empty;

        private bool _isFirstCall = true;

        protected int UserId
        {
            get
            {
                if (!_isFirstCall)
                    return _userId;

                _isFirstCall = false;                               
                foreach (var claim in User.Claims)
                {
                    if (claim.Type != "userId")
                        continue;

                    int.TryParse(claim.Value, out _userId);
                    break;
                }

                return _userId;
            }
        }

        protected string ClientId
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_clientId))
                    return _clientId;

                foreach (var claim in User.Claims)
                {
                    if (claim.Type != "clientId")
                        continue;
                    _clientId = claim.Value;
                    break;
                }

                return _clientId;
            }
        }
        protected string Sub
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_sub))
                    return _sub;

                foreach (var claim in User.Claims)
                {
                    if (claim.Type != "sub")
                        continue;
                    _sub = claim.Value;
                    break;
                }

                return _sub;
            }
        }
    }
}