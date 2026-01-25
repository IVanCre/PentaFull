using MessageLib;


namespace Penta_ClientLib.Interfaces
{
    public interface IMessageHolder
    {
        Task<bool> SaveMessage(Message msg);
        Task<List<Message>> GetNonSended();

        Task<bool> DeleteByChatID(int chatID);
    }
}
