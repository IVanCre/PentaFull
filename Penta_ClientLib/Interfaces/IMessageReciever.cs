namespace Penta_ClientLib.Interfaces
{
    internal interface IMessageReciever
    {
        event ChatChanged CreatedNewChat;        
        event ChatChanged ChatDeleted;        
        event ChatUserListChanged UserAdded;        
        event ChatUserListChanged UserRemoved;        
        event NewMessageInChat MessageAddedToChat;        
        event InvitedToChat RecieveInvite;
        event AccountDeleted AccountDeleted;
    }
}
