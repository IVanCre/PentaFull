using Microsoft.AspNetCore.SignalR;

using Message_Server.Interfaces;

namespace Message_Server.Services.SignalR
{
    public class HubObserver : IHubObserver
    {
        private IMessageHub _hub;
        private object locker= new();
        private int _connectedClients = 0;

        public void ClientConnected(IMessageHub scopedHub)
        {
            lock (locker)
            {
                if (_hub == null)
                    _hub = scopedHub;

                _connectedClients++;
            }
        }

        public void ClientDisconnected()
        {
            lock (locker)
            {
                _connectedClients--;
                if (_connectedClients < 0)
                    _connectedClients = 0;

                if(_connectedClients==0)
                    _hub= null;
            }
        }

        public IMessageHub TryGetHub()
        {
            return _hub;
        }
    }
}
