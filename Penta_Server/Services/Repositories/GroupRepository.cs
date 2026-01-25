using Penta_Server.Interfaces;
using Penta_Server.Services.Repositories.Models;
using Microsoft.EntityFrameworkCore;


namespace Penta_Server.Services.Repositories
{

    public class GroupRepository(IConfiguration config) : IGroupChatRepository
    {
        private string connStr = config["WorkDB:ConnString"];


        public async Task<int> CreatGroupAsync(int masterUserID, string groupName)
        {
            using (DB db = new DB(connStr))
            {
                var finded = db.Groups.FirstOrDefault(x => x.Name == groupName);
                if (finded == null)
                {
                   var result= db.Groups.Add(
                        new GroupEntity()
                        {
                            Name = groupName,
                            AdminGroupID = masterUserID,
                            UsersInGroup = $"{masterUserID}"
                        });
                    var saved = await db.SaveChangesAsync();
                    return result.Entity.ID ;
                }
            }
            return -1;
        }
        public async Task<bool> DeleteGroup(int masterUserID, int chatID)
        {
            using (DB db = new DB(connStr))
            {
                var finded = db.Groups.FirstOrDefault(x => x.AdminGroupID==masterUserID && x.ID== chatID);
                if (finded!=null)
                {
                    var result= await db.Database.ExecuteSqlRawAsync($"DELETE FROM Groups WHERE ID={finded.ID}");
                    return result == 1;
                }
            }
            return false;
        }

        public async Task<GroupEntity> GetGroupByIDAsync(int groupID)
        {
            using (DB db = new DB(connStr))
            {
                return await db.Groups.FirstOrDefaultAsync(x => x.ID == groupID);
            }
        }
        public async Task<GroupEntity> GetGroupByNameAsync(string groupName)
        {
            using (DB db = new DB(connStr))
            {
                return await db.Groups.FirstOrDefaultAsync(x => x.Name == groupName);
            }
        }



        public async Task<bool> AddUserToGroupAsync(int userID, int groupID)
        {
            using (DB db = new DB(connStr))
            {
                var finded = db.Groups.FirstOrDefault(x => x.ID == groupID);
                if (finded != null)
                {
                    finded.AddToGroup(userID);
                    db.Groups.Update(finded);

                    var updated = await db.SaveChangesAsync();
                    return updated == 1;
                }
            }
            return false;
        }
        public async Task<bool> RemoveUserFromGroupAsync(int userID, int groupID)
        {
            using (DB db = new DB(connStr))
            {
                var finded = db.Groups.FirstOrDefault(x => x.ID == groupID);
                if (finded != null)
                {
                    finded.RemoveFromGroup(userID);
                    db.Groups.Update(finded);

                    var saved = await db.SaveChangesAsync();
                    return saved == 1;
                }
            }
            return false;
        }
    }
}
