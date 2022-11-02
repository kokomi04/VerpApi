using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;
using Verp.Cache.RedisCache;
using VErp.Commons.Constants;
using VErp.Commons.GlobalObject.Attributes;

namespace VErp.Infrastructure.ServiceCore.SignalR
{
    public interface IBroadcastHubClient
    {
        Task BroadcastMessage();
        Task LongTaskStatus(ILongTaskResourceInfo info);
    }

    [PatternHub("notify")]
    [Authorize]
    public class BroadcastSignalRHub : Hub<IBroadcastHubClient>
    {
        private readonly IPrincipalBroadcasterService _principalBroadcaster;

        public BroadcastSignalRHub(IPrincipalBroadcasterService principalBroadcaster)
        {
            _principalBroadcaster = principalBroadcaster;
        }

        public override Task OnConnectedAsync()
        {
            var (userId, connectionId) = GetInfoUserContext();
            _principalBroadcaster.AddUserConnected(userId, connectionId);
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            var (userId, connectionId) = GetInfoUserContext();
            _principalBroadcaster.RemoveUserConnected(userId, connectionId);
            return base.OnDisconnectedAsync(exception);
        }

        private (string, string) GetInfoUserContext()
        {
            var connectionId = Context.ConnectionId;
            var userId = "";
            foreach (var claim in Context.User.Claims)
            {
                if (claim.Type != UserClaimConstants.UserId)
                    continue;

                userId = claim.Value;
                break;
            }

            return (userId, connectionId);
        }

    }
}