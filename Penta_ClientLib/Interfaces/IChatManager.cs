

using Penta_ClientLib.MethodResults;
using MessageLib;

namespace Penta_ClientLib.Interfaces
{
    internal interface IChatManager
    {
        Task<BOOLResult> SendCreateGroupChat(string chatName);//явная только отправка. Ответы от сервера обрабатываются отдельно(по событиям)
        event ChatChanged CreatedNewChat;

        Task<BOOLResult> SendDeleteGroupChat(string chatName);
        event ChatChanged ChatDeleted;
        
        Task<BOOLResult> SendInviteUserToGroupChat(string userID, string chatName);
        event ChatUserListChanged UserAdded;

        Task<BOOLResult> SendLeaveGroupChat(string chatName);
        Task<BOOLResult> SendDeleteUserFromGroupChat(string chatName, string userID);
        event ChatUserListChanged UserRemoved;

        Task<BOOLResult> AddMessageToChat(string chatName, string userName, MessageType type, byte[] data);
        event NewMessageInChat MessageAddedToChat;
    }
}
