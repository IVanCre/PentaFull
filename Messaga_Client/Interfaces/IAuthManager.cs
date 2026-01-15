namespace Messaga_Client.Interfaces
{
    public interface IAuthManager
    {
        bool IsRegistred();
        Task<bool> TryRegister(string userName, string password);
        Task<bool> TryLogin(string username, string password);
        Task<bool> TryDeleteAccount();
    }
}
