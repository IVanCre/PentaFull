namespace Penta_Server.Interfaces
{
    public interface IConnectionsRepository
    {
        void Add(int userID, string connectionID);
        string GetConnectionID(int userID);
        void RemoveByUserID(int userID);
    }
}
