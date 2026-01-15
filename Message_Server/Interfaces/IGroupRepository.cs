using Message_Server.Services.Repositories.Models;


namespace Message_Server.Interfaces
{
    public interface IGroupRepository
    {
        Task<bool> CreatGroupAsync(int masterUserID, string groupName);
        Task<GroupEntity> GetGroupByIDAsync(int groupID);
        Task<GroupEntity> GetGroupByNameAsync(string groupName);

        void DeleteGroup(int masterUserID, int groupID);

        Task<bool> AddUserToGroupAsync(int userID, int groupID);
        Task<bool> RemoveUserFromGroupAsync(int userID, int groupID);
    }
}
