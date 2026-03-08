using MessageLib;

namespace Penta_Server.Interfaces
{
    /// <summary>
    /// Crud для работы с Сообщениями в БД
    /// </summary>
    public interface IMessageRepository
    {
        /// <summary>
        /// Сохраняет поле Message.Data как отдельную сущность(под указанным sharedDataID)
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="sharedDataID"></param>
        void SaveWithData(Message msg, long sharedDataID);

        /// <summary>
        /// поле Message.Data сохраняется как null
        /// </summary>
        /// <param name="msg"></param>
        void SaveWithoutData(Message msg);

        /// <summary>
        /// Сохраняет указанный массив данных с ограниченным числом чтения\копирования
        /// </summary>
        /// <param name="data"></param>
        /// <param name="copyCount"></param>
        /// <returns></returns>
        Task<long> SaveDataLikeShared(byte[] data, int copyCount);

        Task<List<Message>> GetNonSendedForUserAsync(int userID);
        Task<bool> HasNonSended(int userID);
        void DeleteMessage(long messageID);
    }
}
