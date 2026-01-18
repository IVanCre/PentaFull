using Microsoft.AspNetCore.SignalR;
using Penta_Server.Services.SignalR;

namespace Penta_Server.Interfaces
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
