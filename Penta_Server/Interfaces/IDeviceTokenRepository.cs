namespace Penta_Server.Interfaces
{
    public interface IDeviceTokenRepository
    {
        Task<bool> SaveDeviceToken(int userID, string tokenDevice);
        Task<List<string>> GetTokenDeviceByID(int userID);
        Task<bool> RemoveUserDevice(int userID, string tokenDevice);
    }
}
