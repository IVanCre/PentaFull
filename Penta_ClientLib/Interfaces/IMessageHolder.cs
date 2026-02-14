using MessageLib;


namespace Penta_ClientLib.Interfaces
{
    /// <summary>
    /// Реализация в библиотеке отсутствует. Сделай сам и внедри через DI
    /// </summary>
    public interface IMessageHolder
    {
        Task<bool> SaveMessage(Message msg);
        Task<List<Message>> GetNonSended();

        Task<List<Message>> GetMessagesByChat(int chatID, int maxLastMessageCount);
        Task<bool> DeleteByChatID(int chatID);
    }
}
