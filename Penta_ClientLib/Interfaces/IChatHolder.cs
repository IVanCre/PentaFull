namespace Penta_ClientLib.Interfaces
{
    internal interface IChatHolder
    {
        Task<bool> CreateChat(string chatName, int chatID);
        Task<int> GetChatID(string chatName);
        Task<bool> DeleteChat(int chatID);

        Task<bool> AddUserToChat(int userid, int chatID);
        Task<bool> RemoveUserFromChat(int userid, int chatID);
    }
}
