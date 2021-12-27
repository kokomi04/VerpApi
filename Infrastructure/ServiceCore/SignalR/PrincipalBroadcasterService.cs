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
        private readonly Dictionary<string, IList<string>> _ConnectedUsers = new Dictionary<string, IList<string>>();

        public bool IsUserConnected()
        {
            return _ConnectedUsers.Count > 0;
        }

        public bool IsUserConnected(string userId)
        {
            return _ConnectedUsers.ContainsKey(userId) ? _ConnectedUsers[userId].Count > 0 : false;
        }

        public void AddUserConnected(string userId, string connectionId)
        {
            if(string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(connectionId)) return;

            if (!_ConnectedUsers.ContainsKey(userId))
            {
                _ConnectedUsers.Add(userId, new List<string>());
            }

            if (!_ConnectedUsers[userId].Contains(connectionId))
                _ConnectedUsers[userId].Add(connectionId);
        }

        public void RemoveUserConnected(string userId, string connectionId)
        {
            if (string.IsNullOrWhiteSpace(connectionId)) return;

            if (string.IsNullOrWhiteSpace(userId))
            {
                var userConnected = _ConnectedUsers.SelectMany(x => x.Value.Select(y => new { UserId = x.Key, ConnectionId = y })).FirstOrDefault(x => x.ConnectionId == userId);
                if (userConnected != null)
                    userId = userConnected.UserId;
            }

            if (!string.IsNullOrWhiteSpace(userId) && _ConnectedUsers.ContainsKey(userId) && _ConnectedUsers[userId].Contains(connectionId))
            {
                _ConnectedUsers[userId] = _ConnectedUsers[userId].Where(eConnectionId => eConnectionId != connectionId).ToList();
                if (_ConnectedUsers[userId].Count == 0)
                    _ConnectedUsers.Remove(userId);
            }
        }

        public IReadOnlyList<string> GetAllConnectionId(string[] arrUserId)
        {
            return _ConnectedUsers.Where(x => arrUserId.Contains(x.Key)).SelectMany(x => x.Value).ToList();
        }
    }
}