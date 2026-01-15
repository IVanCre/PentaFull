namespace Message_Server.Interfaces
{
    public interface ICopyUserDetector
    {
        bool IsClientAlreadyInSystem(int userID);
    }
}
