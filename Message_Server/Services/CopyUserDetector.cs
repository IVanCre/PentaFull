using Message_Server.Interfaces;

namespace Message_Server.Services
{
    public class CopyUserDetector(
        IConnectionsRepository connRepo) : ICopyUserDetector
    {
        private readonly IConnectionsRepository _connRepo = connRepo;

        public bool IsClientAlreadyInSystem(int userID)
        {
           return !string.IsNullOrEmpty(_connRepo.GetConnectionID(userID));
        }
    }
}
