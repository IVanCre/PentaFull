namespace Messaga_Client.Interfaces
{
    public interface IUserProvider
    {
        public Task<bool> AddUser(string name);
        public void DeleteUser(string name);

        public Task<List<string>> GetUsers();
    }
}
