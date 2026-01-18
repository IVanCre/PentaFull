

namespace Penta_ClientLib
{
    internal interface IContactManager
    {
        Task<Tuple<string, Exception>> GetMyContactString();
        Task<Tuple<bool, Exception>> AddNewUserContact(string userName, string userContactID);
        Task<bool> AutoAddNewUserContact(string userName, int userID);
        Task<Tuple<List<string>, Exception>> GetAllContacts();
        Task<Tuple<bool, Exception>> DeleteUserContact(string userName);
        Task<int> GetUserID(string userName);
    }
}
