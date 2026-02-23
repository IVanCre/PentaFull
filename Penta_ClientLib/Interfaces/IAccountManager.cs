

namespace Penta_ClientLib.Interfaces
{
    internal interface IAccountManager
    {
        Task<Tuple<bool, Exception>> Registration(string login, string password);
        Task<Tuple<bool, Exception>> DeleteAccount();
    }
}
