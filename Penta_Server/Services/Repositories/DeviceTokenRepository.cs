using Microsoft.EntityFrameworkCore;
using Penta_Server.Interfaces;
using Penta_Server.Services.Repositories.Models;

namespace Penta_Server.Services.Repositories
{
    public class DeviceTokenRepository(IConfiguration config) : IDeviceTokenRepository
    {
        private string connStr = config["WorkDB:ConnString"];

        public async Task<List<string>> GetTokenDeviceByID(int userID)
        {
            using (DB db = new DB(connStr))
            {
                return  await db.UsersDevices.Where(x => x.UserID == userID)
                    .Select(x=>x.DeviceToken)
                    .ToListAsync();
            }
        }

        public async Task<bool> RemoveUserDevice(int userID, string tokenDevice)
        {
            using (DB db = new DB(connStr))
            {
                var deleted = await db.UsersDevices
                    .Where(x => x.UserID == userID && x.DeviceToken == tokenDevice)
                    .ExecuteDeleteAsync(); 

                return deleted == 1;
            }
        }

        public async Task<bool> SaveDeviceToken(int userID, string tokenDevice)
        {
            using (DB db = new DB(connStr))
            {
                var finded = db.UsersDevices.Where(x => x.ID == userID)
                    .FirstOrDefault();

                if (finded == null)
                {
                    db.UsersDevices.Add(
                       new UserDeviceEntity()
                       {
                           UserID = userID,
                           DeviceToken = tokenDevice
                       });
                }
                else
                {
                    finded.DeviceToken = tokenDevice;
                }
                var inserted= await db.SaveChangesAsync();
                return inserted == 1;
            }
        }
    }
}
