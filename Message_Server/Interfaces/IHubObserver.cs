using Microsoft.AspNetCore.SignalR;
using Message_Server.Services.SignalR;

namespace Message_Server.Interfaces
{
    /// <summary>
    /// Отслеживает факт существования sugnalR-хаба в системе на основе счетчика подключений.
    /// </summary>
    public interface IHubObserver
    {
        void ClientConnected(IMessageHub scopedHub);
        IMessageHub TryGetHub();
        void ClientDisconnected();
    }
}
