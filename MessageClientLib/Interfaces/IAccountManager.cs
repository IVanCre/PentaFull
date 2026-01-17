namespace MessageClientLib.Interfaces
{
    internal interface IAccountManager
    {
        Task<BOOLEANResult> Registration(string login, string password);
        Task<BOOLEANResult> Login(string login, string password);
        Task<BOOLEANResult> DeleteAccount();
    }
}
