using MessageClientLib.Interfaces;

namespace MessageClientLib
{
    public interface IContactManager
    {
        Task<INT32Result> GetMyContactID();
        Task<BOOLEANResult> AddNewUserContact(string userName, int userID);
        Task<BOOLEANResult> DeleteUserContact(int userName);
    }
}
