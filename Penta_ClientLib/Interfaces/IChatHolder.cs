using Penta_ClientLib.DataStructs;

namespace Penta_ClientLib.Interfaces
{
    internal interface IChatHolder
    {
        Task<bool> CreateGroupChat(int chatID, string chatName, bool requestFromGroupAdmin);//данные прихордят от сервера
        Task<int> CreatePrivateChat(string userConnectionID);//когда создаем чат 1to1 чисто на своем устройстве
        
        Task<List<ChatInfo>> GetAllChats();
        Task<int> GetChatIDByName(string chatName);
        Task<ChatInfo> GetChatByID(int chatID);
        Task<bool> DeleteChat(int chatID);

        Task<bool> AmCreatedThisGroupChat(int chatID);

        Task<bool> TryAddUserToChat(int userid, int chatID);
        Task<bool> RemoveUserFromChat(int userid, int chatID);
    }
}
