using Penta_ClientLib.DataStructs;

namespace Penta_ClientLib.Interfaces
{
    internal interface IChatHolder
    {
        Task<bool> AddGroupChat(int chatID, string chatName);//данные прихордят от сервера
        Task<int> CreatePrivateChat(string userConnectionID);//когда создаем чат 1to1 чисто на своем устройстве
        Task<int> GetChatID(string chatName);
        Task<List<ChatInfo>> GetAllChats();
        Task<bool> DeleteChat(int chatID);

        Task<bool> TryAddUserToChat(int userid, int chatID);
        Task<bool> RemoveUserFromChat(int userid, int chatID);
    }
}
