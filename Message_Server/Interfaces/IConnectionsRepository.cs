namespace Message_Server.Interfaces
{
    public interface IConnectionsRepository
    {
        public void Add(int userID, string connectionID);
        public string GetConnectionID(int userID);
        public void RemoveByUserID(int userID);
    }
}
