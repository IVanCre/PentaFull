using Penta_ClientLib.DataStructs;

namespace Penta_ClientLib.Interfaces
{
    public interface IChatHolder
    {
        Task<bool> CreateChat(int chatID, string chatName);
        Task<int> CreateLocalChat(string userConnectionID);//когда создаем чат 1to1 чисто на своем устройстве
        Task<int> GetChatID(string chatName);
        Task<List<ChatInfo>> GetAllChats();
        Task<bool> DeleteChat(int chatID);

        Task<bool> AddUserToChat(int userid, int chatID);
        Task<bool> RemoveUserFromChat(int userid, int chatID);
    }
}
