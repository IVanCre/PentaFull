using MessageLib;

namespace Messaga_Client.Interfaces
{
    public interface IMessageProvider
    {
        Task<bool> SaveMessage(Message message, bool IsSended);

        Task<List<Message>> GetAllMessagesFromUser(string username);
        Task<List<Message>> GetAllMessagesToUser(string username);

        void DeleteMessagesFromUser(string user);
        void DeleteMessagesToUser(string user);
    }
}
