using MessageLib;


namespace Penta_ClientLib.Interfaces
{
    internal interface IMessageHolder
    {
        Task<bool> SaveMessage(Message msg);
        Task<List<Message>> GetNonSended();

        Task<bool> DeleteByChatID(int chatID);
    }
}
