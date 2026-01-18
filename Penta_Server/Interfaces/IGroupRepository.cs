using Penta_Server.Services.Repositories.Models;


namespace Penta_Server.Interfaces
{
    /// <summary>
    /// Сложный crud для работы с Группами в БД
    /// </summary>
    public interface IGroupRepository
    {
        Task<int> CreatGroupAsync(int masterUserID, string groupName);
        Task<GroupEntity> GetGroupByIDAsync(int groupID);
        Task<GroupEntity> GetGroupByNameAsync(string groupName);

        Task<bool> DeleteGroup(int masterUserID, int groupID);

        Task<bool> AddUserToGroupAsync(int userID, int groupID);
        Task<bool> RemoveUserFromGroupAsync(int userID, int groupID);
    }
}
