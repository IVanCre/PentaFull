namespace Penta_Server.Interfaces
{
    public delegate void UserConnectionStateChanged(int userID, bool connected);
    public interface IConnectionsRepository
    {
        void Add(int userID, string connectionID);
        string GetConnectionID(int userID);
        void RemoveByUserID(int userID);
        event UserConnectionStateChanged UserConnectionStateChanged;
    }
}
