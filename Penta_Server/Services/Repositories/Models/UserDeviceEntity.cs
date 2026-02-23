using Microsoft.EntityFrameworkCore;

namespace Penta_Server.Services.Repositories.Models
{
    [Index("ID", IsUnique = true)]
    [Index("UserID","DeviceToken")]
    public class UserDeviceEntity
    {
        public int ID { get; set; }
        public int UserID { get; set; }
        public string DeviceToken { get; set; }
    }
}
