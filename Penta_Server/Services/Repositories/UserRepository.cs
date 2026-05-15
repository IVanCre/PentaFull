using Penta_Server.Interfaces;
using Penta_Server.Services.Repositories.Models;
using Penta_Server.StaticUtilits;
using Microsoft.EntityFrameworkCore;

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
                    var addedUser = db.Users.Add(
                        new UserEntity() 
                        { 
                            Name = name,
                            MaskedPassword= maskedPass,
                            RegistrationDate = DateTime.UtcNow,
                            LastConnectDate = DateTime.UtcNow,
                        });
                    await db.SaveChangesAsync();
                    return addedUser.Entity.ID;
                }
                else
                    return -1;
            }
        }
        public async void SetUserLastConnectDate(int userID)
        {
            using (DB db = new DB(_connStr))
            {
                var user = db.Users.FirstOrDefault(x => x.ID == userID);
                if (user != null)
                {
                    user.LastConnectDate = DateTime.UtcNow;
                    await db.SaveChangesAsync();
                }
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
                var userForDelete =db.Tokens
                    .Include(x=>x.User)
                    .FirstOrDefault(x=>x.AccessHash==tokenHash)?.User;

                if (userForDelete != null)
                {
                    await db.Messages
                        .Where(x=>x.ToUserID==userForDelete.ID)//сообщения, адресованные удаленному юзеру
                        .ExecuteDeleteAsync();

                    await db.Tokens
                        .Include (x=>x.User)
                        .Where(x => x.User.ID == userForDelete.ID)
                        .ExecuteDeleteAsync();

                    await db.Users
                        .Where(x => x.ID == userForDelete.ID)
                        .ExecuteDeleteAsync();

                    _logger?.SaveInfo($"Пользователь {userForDelete.Name} удален");
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
                await db.Messages
                    .Where(x => x.ToUserID == userID)
                    .ExecuteDeleteAsync();

                var deleted = await db.Tokens
                        .Include(x => x.User)
                        .Where(x => x.User.ID == userID)
                        .ExecuteDeleteAsync();

                if (deleted == 1)
                {
                    deleted = await db.Users
                        .Where(x => x.ID == userID)
                        .ExecuteDeleteAsync();

                    if (deleted == 1)
                        return true;
                }
            }

            return false;
        }

        public async Task<List<int>> GetAllUsersID()
        {
            using (DB db=new DB(_connStr))
            {
                return await db.Users
                    .Where(x=> x.Name!="admin_1991")
                    .Select(x => x.ID)
                    .ToListAsync();
            }
        }

        public async Task<List<string>> GetAllUsersNames()
        {
            using (DB db = new DB(_connStr))
            {
                return await db.Users
                    .Where(x => x.Name != "admin_1991")
                    .Select(x => x.Name)
                    .ToListAsync();
            }
        }


    }
}
