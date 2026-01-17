namespace MessageClientLib.Interfaces
{
    internal interface IChatManager
    {
        Task<INT32Result> CreateGroupChat(string chatName);
        Task<BOOLEANResult> InviteUserToGroupChat(string userID, string chatName);
        Task<BOOLEANResult> LeaveGroupChat(string chatName);
        Task<BOOLEANResult> DeleteUserFromGroupChat(string chatName, string userID);
        Task<BOOLEANResult> DeleteGroupChat(string chatName);
        Task<BOOLEANResult> DeletePrivateChat(int localChatID);
    }
}
