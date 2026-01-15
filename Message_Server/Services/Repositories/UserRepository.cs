using Message_Server.Interfaces;
using Message_Server.Services.Repositories.Models;
using Message_Server.StaticUtilits;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Message_Server.Services.Repositories
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

                    _logger?.SaveSystemInfo($"Добавлен новый пользователь {name}");
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


        public async Task<bool> DeleteUserByTokenAsync(string token)
        {
            using (DB db= new DB(_connStr))
            {
                var user =db.Tokens
                    .Include(x=>x.User)
                    .FirstOrDefault(x=>x.Token==token)?.User;

                if (user != null)
                {
                    SqlParameter param1 = new SqlParameter("@param", user.Name);
                    db.Database.ExecuteSqlRaw($"DELETE FROM Messages WHERE ToUser=@param",param1);//del messages
                    await db.SaveChangesAsync();

                    SqlParameter param2 = new SqlParameter("@param", user.ID);
                    db.Database.ExecuteSqlRaw($"DELETE FROM Users WHERE ID=@param",param2);//тут каскадом и токен удалится
                    await db.SaveChangesAsync();

                    _logger?.SaveSystemInfo($"Пользователь {user.Name} удален");
                    return true;
                }
                else
                    return false;
            }
        }


    }
}
