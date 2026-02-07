namespace Penta_ClientLib.Interfaces
{
    /// <summary>
    /// Реализация в библиотеке отсутствует. Сделай сам и внедри через DI
    /// </summary>
    public interface IContactHolder
    {
        Task<bool> AddContact(string name, string contactID);
        Task<List<ContactInfo>> GetAllContacts();
        Task<bool> DeleteByContactID(string contactID);
    }
}
