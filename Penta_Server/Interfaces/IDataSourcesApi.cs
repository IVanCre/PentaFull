namespace Penta_Server.Interfaces
{
    public interface IDataSourcesApi
    {
        Task<Tuple<int, string>> RegistNewService(string serviceName);
        Task<bool> AddUserToNotify(string connectID, string serviceToken);
        Task<bool> SendMessage(string message, string serviceToken);
        Task<bool> DeleteService(string serviceToken);

    }
}
