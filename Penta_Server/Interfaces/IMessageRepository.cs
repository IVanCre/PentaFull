using MessageLib;

namespace Penta_Server.Interfaces
{
    /// <summary>
    /// Crud для работы с Сообщениями в БД
    /// </summary>
    public interface IMessageRepository
    {
        void Add(Message msg);
        Task<List<Message>> GetNonSendedForUserAsync(int userID);
        Task<bool> HasNonSended(int userID);
        void MarkForDelete(long messageID);
    }
}
