using Message_Server.Interfaces;
using System.Collections.Concurrent;

namespace Message_Server.Services.SignalR
{
    public class ConnectionsRepository : IConnectionsRepository
    {
        private ConcurrentDictionary<int, string> _connections = new();

        public void Add(int userID, string connectionID)
        {
            _connections.TryAdd(userID, connectionID);
        }

        public string GetConnectionID(int userID)
        {
            string ID=string.Empty;
            _connections.TryGetValue(userID, out ID);
            return ID;
        }

        public void RemoveByUserID(int userID)
        {
            _connections.Remove(userID, out string val);     
        }
    }
}
