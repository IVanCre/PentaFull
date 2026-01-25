

namespace Penta_ClientLib
{
    internal interface IContactConverter
    {
        Task<string> GetMyContactID();
        int ExtractUserID(string userContactID);
        string ConvertUserIDToContactID(int userID);
    }
}
