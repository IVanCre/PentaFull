using Penta_Server.Services.Repositories.Models;


namespace Penta_Server.Interfaces
{
    /// <summary>
    /// Сложный crud для работы с Группами в БД
    /// </summary>
    public interface IGroupChatRepository
    {
        Task<int> CreatGroupAsync(int masterUserID, string groupName);
        Task<GroupEntity> GetGroupByIDAsync(int groupID);
        Task<GroupEntity> GetGroupByNameAsync(string chatName);

        Task<bool> DeleteGroup(int masterUserID, int chatID);

        Task<bool> AddUserToGroupAsync(int userID, int groupID);
        Task<bool> RemoveUserFromGroupAsync(int userID, int groupID);
    }
}
