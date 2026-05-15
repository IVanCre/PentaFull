using Penta_Server.Services.Repositories.Models;

namespace Penta_Server.Interfaces
{
    public interface IDataSourceRepository
    {
        Task<DataSourceEntity> AddNewDataSource(string sourceName, int groupID);
        Task<bool> DeleteDataSource(string sourceToken);
        Task<DataSourceEntity> FindByName(string sourceName);
        Task<DataSourceEntity> FindByToken(string sourceName);
    }
}
