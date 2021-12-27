using System.Collections.Generic;
using System.Linq;

namespace VErp.Infrastructure.ServiceCore.SignalR
{
    public interface IPrincipalBroadcasterService
    {
        void AddUserConnected(string userId, string connectionId);
        IReadOnlyList<string> GetAllConnectionId(string[] arrUserId);
        bool IsUserConnected();
        bool IsUserConnected(string userId);
        void RemoveUserConnected(string userId, string connectionId);
    }

    public class PrincipalBroadcasterService : IPrincipalBroadcasterService
    {
        private readonly Dictionary<string, IList<string>> ConnectedUsers = new Dictionary<string, IList<string>>();

        public bool IsUserConnected()
        {
            return ConnectedUsers.Count > 0;
        }

        public bool IsUserConnected(string userId)
        {
            return ConnectedUsers.ContainsKey(userId) ? ConnectedUsers[userId].Count > 0 : false;
        }

        public void AddUserConnected(string userId, string connectionId)
        {
            if(string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(connectionId)) return;

            if (!ConnectedUsers.ContainsKey(userId))
            {
                ConnectedUsers.Add(userId, new List<string>());
            }

            if (!ConnectedUsers[userId].Contains(connectionId))
                ConnectedUsers[userId].Add(connectionId);
        }

        public void RemoveUserConnected(string userId, string connectionId)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(connectionId)) return;

            if (ConnectedUsers.ContainsKey(userId) && ConnectedUsers[userId].Contains(connectionId))
            {
                ConnectedUsers[userId] = ConnectedUsers[userId].Where(eConnectionId => eConnectionId != connectionId).ToList();
            }
        }

        public IReadOnlyList<string> GetAllConnectionId(string[] arrUserId)
        {
            return ConnectedUsers.Where(x => arrUserId.Contains(x.Key)).SelectMany(x => x.Value).ToList();
        }
    }
}