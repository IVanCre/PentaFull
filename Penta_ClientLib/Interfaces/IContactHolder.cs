namespace Penta_ClientLib.Services
{
    internal interface IContactHolder
    {
        Task<bool> AddContactAsync(string userName, int userID);
        Task<int> GetContactIDAsync(string userName);
        Task<List<string>> GetAllContacts();
        Task<bool> DeleteContact(string userName);
    }
}
