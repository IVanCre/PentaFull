using Penta_ClientLib.MethodResults;

namespace Penta_ClientLib.Interfaces
{
    internal interface IAccountManager
    {
        Task<BOOLResult> Registration(string login, string password);
        Task<BOOLResult> Login(string login, string password);
        Task<BOOLResult> DeleteAccount();
    }
}
