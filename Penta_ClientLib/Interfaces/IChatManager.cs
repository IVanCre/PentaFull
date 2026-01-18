
using MessageLib;

namespace Penta_ClientLib.Interfaces
{
    internal interface IChatManager
    {
        Task<Tuple<bool, Exception>> SendCreateGroupChat(string chatName);//явная только отправка. Ответы от сервера обрабатываются отдельно(по событиям)
        event ChatChanged CreatedNewChat;

        Task<Tuple<bool, Exception>> SendDeleteGroupChat(string chatName);
        event ChatChanged ChatDeleted;
        
        Task<Tuple<bool, Exception>> SendInviteUserToGroupChat(string userID, string chatName);
        event ChatUserListChanged UserAdded;

        Task<Tuple<bool, Exception>> SendLeaveGroupChat(string chatName);
        Task<Tuple<bool, Exception>> SendDeleteUserFromGroupChat(string chatName, string userID);
        event ChatUserListChanged UserRemoved;

        Task<Tuple<bool, Exception>> AddMessageToChat(string chatName, string userName, MessageType type, byte[] data);
        event NewMessageInChat MessageAddedToChat;
    }
}
