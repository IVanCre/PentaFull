using MessageLib;


namespace Penta_ClientLib.Interfaces
{
    internal interface IMessageHolder
    {
        Task<bool> SaveMessage(Message msg);
        Task<List<Message>> GetMessagesByChat(int chatID, int maxLastMessageCount);


        Task<List<Message>> GetNonSended();
        Task MarkMessageLikeSended(long msgID);
    }
}
