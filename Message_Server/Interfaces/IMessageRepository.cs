using MessageLib;

namespace Message_Server.Interfaces
{
    public interface IMessageRepository
    {
        void Add(Message msg);
        Task<List<Message>> GetNonSendedForUserAsync(int userID);
        bool HasNonSended(int userID);
        void MarkForDelete(int messageID);
    }
}
