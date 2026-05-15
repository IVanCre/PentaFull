using Penta_Server.Interfaces;
using Penta_Server.Services.Repositories.Models;
using Microsoft.EntityFrameworkCore;


namespace Penta_Server.Services.Repositories
{
    public class DataSourcesRepository(IConfiguration config) : IDataSourceRepository
    {
        private string connStr = config["WorkDB:ConnString"];

        public async Task<DataSourceEntity> AddNewDataSource(string sourceName, int groupID)
        {
            DataSourceEntity created;
            using (DB db = new DB(connStr))
            {
                created = new DataSourceEntity()
                {

                    AccessToken = Guid.NewGuid().ToString(),
                    GroupID = groupID,
                };
                db.DataSources.Add(created);
                await db.SaveChangesAsync();

                return created;
            }
        }

        public async Task<bool> DeleteDataSource(string sourceToken)
        {
            using (DB db = new DB(connStr))
            {
                var deleted = await db.DataSources
                    .Where(x => x.AccessToken == sourceToken)
                    .ExecuteDeleteAsync();

                return deleted == 1;
            }
        }

        public async Task<DataSourceEntity> FindByName(string sourceName)
        {
            using (DB db = new DB(connStr))
            {
                return await db.DataSources.FirstOrDefaultAsync(x => x.Name == sourceName);
            }
        }

        public async Task<DataSourceEntity> FindByToken(string sourceToken)
        {
            using (DB db = new DB(connStr))
            {
                return await db.DataSources.FirstOrDefaultAsync(x => x.AccessToken == sourceToken);
            }
        }
    }
}
