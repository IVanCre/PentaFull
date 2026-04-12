using Penta_Server.Interfaces;
using Penta_Server.Services.Repositories.Models;
using Penta_Server.StaticUtilits;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace Penta_Server.Services.Repositories
{

    public class UserRepository(
        IConfiguration config,
        ILogWriter logger) : IUserRepository
    {
        private string _connStr = config["WorkDB:ConnString"];
        private ILogWriter _logger = logger;

        public async Task<int> AddNewUserAsync(string name, string password)
        {
            using (DB db = new DB(_connStr))
            {
                string maskedPass = PasswordManager.Encrypt(password,name);
                var user =db.Users.FirstOrDefault(x => x.Name == name && x.MaskedPassword == maskedPass);
                if (user == null)
                {
                    var addedUser = db.Users.Add(new UserEntity() { Name = name,MaskedPassword= maskedPass });
                    await db.SaveChangesAsync();
                    return addedUser.Entity.ID;
                }
                else
                    return -1;
            }
        }
        public async Task<int> FindUserAsync(string name, string password)
        {
            using (DB db = new DB(_connStr))
            {
                string maskedPass = PasswordManager.Encrypt(password, name);
                var user = db.Users.FirstOrDefault(x => x.Name == name && x.MaskedPassword == maskedPass);
                return user.ID;
            }
        }
        public async Task<string> FindUserNameByIDAsync(int userID)
        {
            using (DB db = new DB(_connStr))
            {
                var user = await db.Users.FirstOrDefaultAsync(x => x.ID == userID);
                return user.Name;
            }
        }


        public async Task<bool> DeleteUserByTokenAsync(string tokenHash)
        {
            using (DB db= new DB(_connStr))
            {
                var user =db.Tokens
                    .Include(x=>x.User)
                    .FirstOrDefault(x=>x.AccessHash==tokenHash)?.User;

                if (user != null)
                {
                    SqlParameter param1 = new SqlParameter("@param", user.ID);
                    db.Database.ExecuteSqlRaw($"DELETE FROM Messages WHERE ToUserID=@param",param1);//del messages
                    await db.SaveChangesAsync();

                    SqlParameter param2 = new SqlParameter("@param", user.ID);
                    db.Database.ExecuteSqlRaw($"DELETE FROM Tokens WHERE UserID=@param", param2);
                    await db.SaveChangesAsync();

                    SqlParameter param3 = new SqlParameter("@param", user.ID);//не хочу париться с настройкой каскадного удаления
                    db.Database.ExecuteSqlRaw($"DELETE FROM Users WHERE ID=@param", param3);
                    await db.SaveChangesAsync();

                    _logger?.SaveInfo($"Пользователь {user.Name} удален");
                    return true;
                }
                else
                    return false;
            }
        }

        public async Task<bool> DeleteUserByIDAsync(int userID)
        {
            using (DB db= new DB(_connStr))
            {
                await db.Database.ExecuteSqlRawAsync($"DELETE FROM Messages WHERE ToUserID={userID}");
                var deleted = await db.Database.ExecuteSqlAsync($"DELETE FROM Tokens WHERE UserID={userID}");
                if (deleted == 1)
                {
                    deleted = await db.Database.ExecuteSqlAsync($"DELETE FROM Users WHERE ID={userID}");
                    if (deleted == 1)
                        return true;
                }
            }

            return false;
        }

        public async Task<List<int>> GetAllUsers()
        {
            using (DB db=new DB(_connStr))
            {
                return await db.Users.Select(x => x.ID).ToListAsync();
            }
        }
    }
}
