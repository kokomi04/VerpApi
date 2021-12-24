using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject.Attributes;

namespace VErp.Infrastructure.ServiceCore.SignalR
{
    public interface IBroadcastHubClient
    {
        Task BroadcastMessage();
    }

    [PatternHub("notify")]
    public class BroadcastSignalRHub : Hub<IBroadcastHubClient>
    {

    }
}