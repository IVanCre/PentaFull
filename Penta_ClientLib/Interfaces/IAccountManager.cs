

namespace Penta_ClientLib.Interfaces
{
    internal interface IAccountManager
    {
        Task<Tuple<bool, Exception>> RegistrationAsync(string login, string password);
        Task<Tuple<bool, Exception>> LoginAsync(string login, string password);
        Task<Tuple<bool, Exception>> DeleteAccount();
    }
}
