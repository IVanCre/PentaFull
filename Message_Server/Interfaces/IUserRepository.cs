namespace Message_Server.Interfaces
{
    /// <summary>
    /// Crud для работы с юзерами в БД
    /// </summary>
    public interface IUserRepository
    {
        public Task<int> AddNewUserAsync(string name,string password);
        public Task<int> FindUserAsync(string username,string password);
        public Task<string> FindUserNameByIDAsync(int userID);
        public Task<bool> DeleteUserByTokenAsync(string token);
    }
}
