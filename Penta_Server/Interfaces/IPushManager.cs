namespace Penta_Server.Interfaces
{
    public interface IPushManager
    {
        Task SendPushToUserDevices(int toUserID, int fromUserID, string text);
        Task SendPushToUserDevices(int toUserID, int fromuserID);
    }
}
