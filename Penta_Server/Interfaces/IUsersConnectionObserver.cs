namespace Penta_Server.Interfaces
{
    public interface IUsersConnectionObserver
    {
        void AddToObserve(int chatID, int recieverID, int userIDToObserve);
        void DeleteFromObserve(int chatID, int recieverID, int userIDToObserve);
    }
}
