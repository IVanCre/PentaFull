namespace Penta_ClientLib.Interfaces
{
    internal interface IMessageReciever
    {
        event ChatChanged CreatedNewChat;        
        event ChatChanged ChatDeleted;        
        event ChatUserListChanged UserAdded;        
        event ChatUserListChanged UserRemoved;        
        event NewMessageInChat MessageAddedToChat;        
        event AccountDeleted AccountDeleted;
        event RecieverConnectedChanged UserConnectionChanged;
    }
}
