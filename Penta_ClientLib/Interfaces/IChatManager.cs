using Penta_ClientLib.DataStructs;
using MessageLib;

namespace Penta_ClientLib.Interfaces
{
    internal interface IChatManager
    {
        Task<Tuple<bool, Exception>> SendCreateGroupChat(string chatName);//явная только отправка. Ответы от сервера обрабатываются отдельно(по событиям)
        Task<Tuple<bool, Exception>> SendDeleteGroupChat(int chatID);
        Task<Tuple<bool, Exception>> SendAddUserToGroupChat(int chatID, string userConnectID);
        Task<Tuple<bool, Exception>> SendLeaveGroupChat(int chatID);
        Task<Tuple<bool, Exception>> SendDeleteUserFromGroupChat(int chatID, string userConnectID);

        Task<Tuple<bool, Exception>> AddMessageToChat(int chatID, int recieverID, MessageType type, byte[] data);
        Task<bool> AmCreatedThisGroupChat(int chatID);

        Task<bool> DeletePrivateChat(int chatID);
        Task<int> CreatePrivateChat(string chatName);

        Task<List<ChatInfo>> GetAllChatsInfo();
        Task<ChatInfo> GetChatByID(int chatID);
    }
}
