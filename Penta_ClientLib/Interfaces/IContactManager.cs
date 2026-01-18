using Penta_ClientLib.Interfaces;
using Penta_ClientLib.MethodResults;

namespace Penta_ClientLib
{
    internal interface IContactManager
    {
        Task<STRResult> GetMyContactString();
        Task<BOOLResult> AddNewUserContact(string userName, string userContactID);
        Task<bool> AutoAddNewUserContact(string userName, int userID);
        Task<BOOLResult> DeleteUserContact(string userName);
        Task<int> GetUserID(string userName);
    }
}
