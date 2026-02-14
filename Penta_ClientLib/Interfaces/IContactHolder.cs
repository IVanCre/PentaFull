namespace Penta_ClientLib.Interfaces
{
    /// <summary>
    /// Реализация в библиотеке отсутствует. Сделай сам и внедри через DI
    /// </summary>
    public interface IContactHolder
    {
        Task<bool> AddContact(string name, string contactID);
        Task<List<ContactInfo>> GetAllContacts();
        Task<string> GetUserNameByContactID(string contactID);
        Task<string> GetContactIDByName(string name);
        Task<int> GetUserIDByName(string name);//возвращает чистый идентификатор(id юзера на сервере) как есть
        Task<bool> DeleteByContactID(string contactID);
    }
}
