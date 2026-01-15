using Messaga_Client.Interfaces;
using Messaga_Client.Services.Repository;


namespace Messaga_Client.Services.DataProviders
{

    internal class UserProvider(DBContext context) : IUserProvider
    {
        private DBContext _context = context;

        public async Task<bool> AddUser(string name)
        {
            var inserted = await _context.DB.InsertAsync(
                 new UserEntity()
                 {
                     Name = name
                 });
            return inserted == 1;
        }

        public void DeleteUser(string username)
        {
            object[] _params = { username };
            _context.DB.ExecuteAsync("DELETE FROM User WHERE Name=?", _params);
        }

        public async Task<List<string>> GetUsers()
        {
            List<string> users = new();
            var finded = await _context.DB.Table<UserEntity>().ToListAsync();
            foreach (var item in finded)
                users.Add(item.Name);

            return users;
        }
    }
}
