using MessageLib;

namespace Penta_Server.Interfaces
{
    /// <summary>
    /// Crud для работы с Сообщениями в БД
    /// </summary>
    public interface IMessageRepository
    {
        /// <summary>
        /// Сохраняет поле Message.Data как отдельную сущность
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="sharedDataID"></param>
        void SaveWithData(Message msg, Guid sharedMarker, int copyCount);

        /// <summary>
        /// поле Message.Data сохраняется как null
        /// </summary>
        /// <param name="msg"></param>
        void SaveWithoutData(Message msg);

        Task<List<Message>> GetNonSendedForUserAsync(int userID);
        Task<bool> HasNonSended(int userID);
        void DeleteMessage(Guid messageID);
    }
}
