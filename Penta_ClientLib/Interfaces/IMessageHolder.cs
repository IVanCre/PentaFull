using MessageLib;


namespace Penta_ClientLib.Interfaces
{
    internal interface IMessageHolder
    {
        /// <summary>
        /// Сохранение сообщения перед последющей обработкой
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="isMessageFromServer">сообщение принято от сервера </param>
        /// <returns></returns>
        Task<bool> SaveMessage(Message msg, bool isMessageFromServer);
        Task<List<Message>> GetLastMessagesByChat(int chatID, int maxLastMessageCount);


        Task<List<Message>> GetNonSended();
        Task<bool> MarkMessageLikeSended(long msgID);
    }
}
