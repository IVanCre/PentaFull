namespace Penta_Server.Interfaces
{
    public interface IPushManager
    {
        Task SendPushToUserDevices(int userID, string title, string text);
        Task SendPushToUserDevices(int toUserID, int fromuserID);
    }
}
