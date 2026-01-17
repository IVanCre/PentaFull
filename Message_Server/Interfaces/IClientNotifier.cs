using MessageLib;

namespace Message_Server.Interfaces
{
    /// <summary>
    /// Пересылает сообщения для клиентов(от сервера к клиенту)
    /// </summary>
    public interface IClientNotifier
    {
        Task SendAllNonSended(int userID, string connID);
        void MessageSended(int messageID);
        Task SendToUser(Message msg);
        Task SendToGroup(Message msg);
    }
}
