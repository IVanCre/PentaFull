using Microsoft.EntityFrameworkCore;

namespace Penta_Server.Services.Repositories.Models
{
    /// <summary>
    /// Внешние сервисы, которые присылают свои данные для уведомления клиентов
    /// </summary>
    [Index("ID", IsUnique = true)]
    [Index("Name")]
    [Index("AccessToken")]
    public class DataSourceEntity
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string AccessToken { get; set; }
        public int GroupID { get; set; }
    }
}
