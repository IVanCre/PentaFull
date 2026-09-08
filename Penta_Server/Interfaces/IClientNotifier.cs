using MessageLib;

namespace Penta_Server.Interfaces
{
    /// <summary>
    /// Пересылает сообщения для клиентов(от сервера к клиенту)
    /// </summary>
    public interface IClientNotifier
    {
        Task SendAllNonSended(int userID, string connID);
        void MessageSended(Guid messageID);
        void PackSended(Guid packID);
        Task SendToUserWithPush(Message msg);
        Task SendToUserWithoutPush(Message msg);
    }
}
