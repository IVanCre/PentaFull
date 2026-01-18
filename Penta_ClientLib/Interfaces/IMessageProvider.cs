using MessageLib;


namespace Penta_ClientLib.Interfaces
{
    internal interface IMessageProvider
    {
        Task<bool> SaveMessage(Message msg);
        Task<bool> MarkSendedForDelete(int messageID);
        Task<List<Message>> GetNonSended();
    }
}
