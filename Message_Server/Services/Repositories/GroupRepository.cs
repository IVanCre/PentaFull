using Message_Server.Interfaces;
using Message_Server.Services.Repositories.Models;
using Microsoft.EntityFrameworkCore;


namespace Message_Server.Services.Repositories
{

    public class GroupRepository(IConfiguration config) : IGroupRepository
    {
        private string connStr = config["WorkDB:ConnString"];


        public async Task<bool> CreatGroupAsync(int masterUserID, string groupName)
        {
            using (DB db = new DB(connStr))
            {
                var finded = db.Groups.FirstOrDefault(x => x.Name == groupName);
                if (finded == null)
                {
                    db.Groups.Add(
                        new GroupEntity()
                        {
                            Name = groupName,
                            AdminGroupID = masterUserID,
                            UsersInGroup = $"{masterUserID}"
                        });
                    var saved = await db.SaveChangesAsync();
                    return saved == 1;
                }
            }
            return false;
        }
        public void DeleteGroup(int masterUserID, int groupID)
        {
            using (DB db = new DB(connStr))
            {
                var finded = db.Groups.FirstOrDefault(x => x.ID == groupID);
                if ( finded.AdminGroupID == masterUserID)
                {
                    db.Database.ExecuteSqlRaw($"DELETE FROM Groups WHERE ID={groupID}");
                }
            }
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
