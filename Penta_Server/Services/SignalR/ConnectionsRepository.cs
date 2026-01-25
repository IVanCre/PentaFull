
using Penta_Server.Interfaces;
using System.Collections.Concurrent;

namespace Penta_Server.Services.SignalR
{
    internal class ConnectionsRepository: IConnectionsRepository
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
